using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using FluentCeVIOWrapper.Common.Talk;

using H.Pipes;
using H.Pipes.Args;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// A wrapper library for FluentCeVIOWrapper Client
/// </summary>
/// <remarks>
/// * .NET Core系(.NET 5/6/...)でもCeVIOの.NET Framework APIにアクセスできるラッパーライブラリ
/// * async/awaitに対応したモダンな書き方ができます
/// </remarks>
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class FluentCeVIO : IDisposable
{
	/// <summary>
	/// 内部のプロセス間通信で使用される名前です。
	/// 複数サーバーを立てる場合はこの名前を変更します。
	/// </summary>
	public const string PipeName = "FluentCeVIOPipe";

	private readonly CancellationTokenSource source;
	private readonly PipeClient<CeVIOTalkMessage> client;

	internal FluentCeVIO()
	{
		source = new CancellationTokenSource();

		client = new PipeClient<CeVIOTalkMessage>(PipeName);
		client.Disconnected += (o, args)
			=> Console.WriteLine($"{nameof(FluentCeVIO)}:Disconnected from server");
		client.Connected += (o, args)
			=> Console.WriteLine($"{nameof(FluentCeVIO)}:Connected to server");
		var _ = client
			.ConnectAsync(source.Token)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 呼び出しのファクトリメソッド
	/// </summary>
	/// <returns></returns>
	public static async ValueTask<FluentCeVIO> FactoryAsync()
	{
		return await Task.Run(() => new FluentCeVIO());
	}

	void IDisposable.Dispose()
	{
		//client dispose
		var _ = client.DisposeAsync();

		source.Dispose();
	}

	private Task<T> CallWrapAsync<T>(
		Host host,
		string callName,
		ReadOnlyCollection<dynamic>? args = null
	)
	{
		var tcs = new TaskCompletionSource<T>();
		void handler(object? _, ConnectionMessageEventArgs<CeVIOTalkMessage?> args)
		{
			var result = args?.Message?.ServerCallValue;
			client.MessageReceived -= handler;
			tcs.TrySetResult(result);
		}

		client.MessageReceived += handler;

		client.WriteAsync(
			new CeVIOTalkMessage
				{
				 ServerCommand = ServerCommand.CallMethod,
				 ServerHost = host,
				 ServerCallName = callName,
				 ServerCallArgValues = args
				}
		);

		return tcs.Task;
	}

	private Task<T> GetWrapAsync<T>(
		Host host,
		string propName
	)
	{
		var tcs = new TaskCompletionSource<T>();
		void handler(object? _, ConnectionMessageEventArgs<CeVIOTalkMessage?> args)
		{
			var result = args?.Message?.ServerCallValue;
			client.MessageReceived -= handler;
			tcs.TrySetResult(result);
		}
		client.MessageReceived += handler;
		client.WriteAsync(
			new CeVIOTalkMessage
			{
				ServerCommand = ServerCommand.GetProperty,
				ServerHost = host,
				ServerCallName = propName,
			}
		);
		return tcs.Task;
	}

	private Task SetWrapAsync<T>(
		Host host,
		string propName,
		T value
	)
	{
		return client.WriteAsync(
			new CeVIOTalkMessage
				{
				 ServerCommand = ServerCommand.SetProperty,
				 ServerHost = host,
				 ServerCallName = propName,
				 ServerCallValue = value
				}
		);
	}

	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	private string DebuggerDisplay
	{
		get { return ToString(); }
	}

	#region Host.Service

	/** ------------------------------------------------------------ **/

	/// <summary>
	/// 非同期で起動
	/// 元の<c>StartHost</c>
	/// </summary>
	public Task<HostStartResult?> StartAsync()
		=> CallWrapAsync<HostStartResult?>(Host.Service, "StartHost");

	/// <summary>
	/// 非同期で終了処理
	/// </summary>
	/// <example>
	/// <code>
	/// await CloseHost();		//通常の使い方
	/// var _ = CloseHost();	//同期的に処理
	/// </code>
	/// </example>
	/// <returns></returns>
	public Task<bool> CloseAsync()
		=> CallWrapAsync<bool>(Host.Service, "CloseHost");

	/// <summary>
	/// ホストアプリ（CeVIO）のバージョンを<see cref="System.Version" />型で返す
	/// </summary>
	/// <returns>バージョン</returns>
	/// <seealso cref="System.Version"/>
	public Task<System.Version> GetHostVersionAsync()
		=> GetWrapAsync<Version>(Host.Service, nameof(IServiceControl.HostVersion));

	/// <summary>
	/// ホストアプリ（CeVIO）が起動中かどうか
	/// </summary>
	/// <returns></returns>
	public Task<bool> GetIsHostStartedAsync()
		=> GetWrapAsync<bool>(Host.Service, "IsHostStarted");

	/** ------------------------------------------------------------ **/

	#endregion

	#region Host.Talker

	/** ------------------------------------------------------------ **/

	/// <summary>
	/// キャスト(話者)を設定します。
	/// </summary>
	/// <param name="castName">キャスト名。利用可能なキャスト名の文字列は<see cref="GetAvailableCastsAsync"/>で取得可。</param>
	/// <seealso cref="GetCastAsync"/>
	/// <seealso cref="GetAvailableCastsAsync"/>
	public async ValueTask SetCastAsync(string castName)
		=> await SetWrapAsync(Host.Talker, "Cast", castName);

	/// <summary>
	/// 現在のキャスト(話者)を取得します。
	/// </summary>
	/// <returns>キャスト名</returns>
	/// <seealso cref="SetCastAsync(string)"/>
	public Task<string> GetCastAsync()
		=> GetWrapAsync<string>(Host.Talker, "Cast");

	/// <summary>
	/// キャストを取得または設定します。<br />
	/// ※互換性の為に残されています。<see cref="GetCastAsync"/>,<see cref="SetCastAsync(string)"/>を利用して下さい。
	/// </summary>
	/// <seealso cref="GetCastAsync"/>
	/// <seealso cref="SetCastAsync(string)"/>
	[Obsolete($"非同期版の {nameof(GetCastAsync)}(), {nameof(SetCastAsync)}() を使用して下さい。")]
	public string Cast
	{
		get => GetCastAsync().Result;
		set => SetCastAsync(value).AsTask().Wait();
	}

	/// <summary>
	/// 音の大きさ（0～100）を取得します。
	/// </summary>
	/// <returns>音の大きさ（0～100）</returns>
	public Task<uint> GetVolumeAsync()
		=> GetWrapAsync<uint>(Host.Talker, "Volume");

	/// <summary>
	/// 音の大きさ（0～100）を設定します。
	/// </summary>
	/// <param name="volume">音の大きさ（0～100）</param>
	/// <returns></returns>
	public async ValueTask SetVolumeAsync([Range(0,100)] uint volume)
		=> await SetWrapAsync<uint>(Host.Talker, "Volume", volume);

	/// <summary>
	/// 話す速さ（0～100）を取得します。
	/// </summary>
	/// <returns>話す速さ（0～100）</returns>
	public Task<uint> GetSpeedAsync()
		=> GetWrapAsync<uint>(Host.Talker, "Speed");

	/// <summary>
	/// 話す速さ（0～100）を設定します。
	/// </summary>
	/// <param name="value">話す速さ（0～100）</param>
	/// <returns></returns>
	public async ValueTask SetSpeedAsync([Range(0,100)] uint value)
		=> await SetWrapAsync<uint>(Host.Talker, "Speed", value);

	/// <summary>
	/// 音の高さ（0～100）を取得します。
	/// </summary>
	/// <returns>音の高さ（0～100）</returns>
	public Task<uint> GetToneAsync()
		=> GetWrapAsync<uint>(Host.Talker, "Tone");

	/// <summary>
	/// 音の高さ（0～100）を設定します。
	/// </summary>
	/// <param name="value">音の高さ（0～100）</param>
	/// <returns></returns>
	public async ValueTask SetToneAsync([Range(0,100)] uint value)
		=> await SetWrapAsync<uint>(Host.Talker, "Tone", value);

	/// <summary>
	/// 声質（0～100）を取得します。
	/// </summary>
	/// <returns>声質（0～100）</returns>
	public Task<uint> GetAlphaAsync()
		=> GetWrapAsync<uint>(Host.Talker, "Alpha");

	/// <summary>
	/// 声質（0～100）を設定します。
	/// </summary>
	/// <param name="value">声質（0～100）</param>
	/// <returns></returns>
	public async ValueTask SetAlphaAsync([Range(0,100)] uint value)
		=> await SetWrapAsync<uint>(Host.Talker, "Alpha", value);

	/// <summary>
	/// 抑揚（0～100）を取得します。
	/// </summary>
	/// <returns>抑揚（0～100）</returns>
	public Task<uint> GetToneScaleAsync()
		=> GetWrapAsync<uint>(Host.Talker, "ToneScale");

	/// <summary>
	/// 抑揚（0～100）を設定します。
	/// </summary>
	/// <param name="value">抑揚（0～100）</param>
	/// <returns></returns>
	public async ValueTask SetToneScaleAsync([Range(0,100)] uint value)
		=> await SetWrapAsync<uint>(Host.Talker, "ToneScale", value);

	/// <summary>
	/// 現在のキャストの感情パラメータマップコレクションを取得します。
	/// </summary>
	/// <remarks>
	/// 戻り値は、元のAPIと異なり、汎用の<see cref="ReadOnlyCollection{T}"/>で返ります。
	/// </remarks>
	/// <seealso cref="TalkerComponent"/>
	/// <seealso cref="GetCastAsync"/>
	/// <seealso cref="SetCastAsync(string)"/>
	/// <returns>感情パラメータの管理オブジェクト<see cref="TalkerComponent"/>の<see cref="ReadOnlyCollection{T}"/></returns>
	public async Task<ReadOnlyCollection<TalkerComponent>> GetComponentsAsync()
		=> (ReadOnlyCollection<TalkerComponent>)await GetWrapAsync<dynamic>(Host.Talker, nameof(ITalker.Components));

	/// <summary>
	/// 利用可能なキャスト名 <c>string[]</c> を取得します。
	/// </summary>
	/// <remarks>
	/// 備考：インストールされているボイスによります。
	/// </remarks>
	/// <seealso cref="SetCastAsync(string)"/>
	/// <returns>利用可能なキャスト名の配列</returns>
	public Task<string[]> GetAvailableCastsAsync()
		=> GetWrapAsync<string[]>(Host.Agent, nameof(ITalker.AvailableCasts));

	/// <summary>
	/// 指定したセリフの再生を開始します。
	/// </summary>
	/// <example>
	/// await SpeakAsync("こんにちは");
	/// //再生終了まで待つ
	/// await SpeakAsync("こんにちは",true);
	/// </example>
	/// <param name="text">セリフ。日本語は最大200文字（古いバージョンは150文字）。</param>
	/// <param name="isWait">再生終了まで待つかどうか</param>
	/// <returns></returns>
	public Task<bool> SpeakAsync(string text, bool isWait = true)
		=> CallWrapAsync<bool>(
			Host.Talker,
			nameof(ITalker.Speak),
			new(new dynamic[]{text, isWait})
		);

	/// <summary>
	/// 再生を停止します。
	/// </summary>
	/// <returns>成功した場合はtrue。それ以外の場合はfalse。</returns>
	public Task<bool> StopAsync()
		=> CallWrapAsync<bool>(Host.Talker,nameof(ITalker.Stop));

	/// <summary>
	/// 指定したセリフの長さを取得します。
	/// </summary>
	/// <param name="text">セリフ。日本語は最大200文字（古いバージョンは150文字）。</param>
	/// <returns>長さ。単位は秒。</returns>
	public Task<double> GetTextDurationAsync(string text)
		=> CallWrapAsync<double>(Host.Talker,nameof(ITalker.GetTextDuration),new(new[]{text}));

	/// <summary>
	/// 指定したセリフの音素単位のデータを取得します。
	/// </summary>
	/// <remarks>
	/// リップシンク等に利用できます。
	/// 戻り値は、元のAPIと異なり、汎用の<see cref="ReadOnlyCollection{T}"/>で返ります。
	/// </remarks>
	/// <param name="text">セリフ。日本語は最大200文字（古いバージョンは150文字）。</param>
	/// <returns>音素単位のデータのコレクション</returns>
	/// <seealso cref="IPhonemeData"/>
	public Task<ReadOnlyCollection<IPhonemeData>> GetPhonemesAsync(string text)
		=> CallWrapAsync<ReadOnlyCollection<IPhonemeData>>(Host.Talker,nameof(ITalker.GetPhonemes),new(new[]{text}));

	/// <summary>
	/// 指定したセリフをWAVファイルとして出力します。
	/// </summary>
	/// <remarks>出力形式はサンプリングレート48kHz, ビットレート16bit, モノラルです。</remarks>
	/// <param name="text">セリフ。</param>
	/// <param name="path">出力先パス。</param>
	/// <returns>成功した場合はtrue。それ以外の場合はfalse。</returns>
	public Task<bool> OutputWaveToFileAsync(
		[StringLength(200)] string text,
		string path
	)
		=> CallWrapAsync<bool>(Host.Talker,nameof(ITalker.OutputWaveToFile),new(new[]{text, path}));

	/** ------------------------------------------------------------ **/

	#endregion

}
