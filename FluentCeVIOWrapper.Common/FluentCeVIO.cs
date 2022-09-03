using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FluentCeVIOWrapper.Common;
using FluentCeVIOWrapper.Common.Talk;
using H.Pipes;
using H.Pipes.Args;
using Microsoft.CSharp;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// A wrapper library for Client
/// </summary>
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class FluentCeVIO : IDisposable
{
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

	public Task<HostStartResult?> StartAsync()
		=> CallWrapAsync<HostStartResult?>(Host.Service, "StartHost");

	public Task<bool> CloseAsync()
		=> CallWrapAsync<bool>(Host.Service, "CloseHost");

	public Task<System.Version> GetHostVersionAsync()
		=> GetWrapAsync<System.Version>(Host.Service, "HostVersion");

	public Task<bool> GetIsHostStartedAsync()
		=> GetWrapAsync<bool>(Host.Service, "IsHostStarted");

	/** ------------------------------------------------------------ **/
	#endregion

	#region Host.Talker
	/** ------------------------------------------------------------ **/

	/// <summary>
	/// キャスト(話者)を設定します。
	/// </summary>
	/// <param name="castName">キャスト名</param>
	/// <seealso cref="GetCastAsync"/>
	public async ValueTask SetCastAsync(string castName)
		=> await SetWrapAsync(Host.Talker, "Cast", castName);

	/// <summary>
	/// キャスト(話者)を取得します。
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

	public Task<uint> GetVolumeAsync()
		=> GetWrapAsync<uint>(Host.Talker, "Volume");
	public async ValueTask SetVolumeAsync([Range(0,100)] uint volume)
		=> await SetWrapAsync<uint>(Host.Talker, "Volume", volume);
	// 音の大きさ（0～100）を取得または設定します。

	public Task<uint> GetSpeedAsync()
		=> GetWrapAsync<uint>(Host.Talker, "Speed");
	public async ValueTask SetSpeedAsync([Range(0,100)] uint value)
		=> await SetWrapAsync<uint>(Host.Talker, "Speed", value);
	// 話す速さ（0～100）を取得または設定します。

	public Task<uint> GetToneAsync()
		=> GetWrapAsync<uint>(Host.Talker, "Tone");
	public async ValueTask SetToneAsync([Range(0,100)] uint value)
		=> await SetWrapAsync<uint>(Host.Talker, "Tone", value);
	// 音の高さ（0～100）を取得または設定します。

	public Task<uint> GetAlphaAsync()
		=> GetWrapAsync<uint>(Host.Talker, "Alpha");
	public async ValueTask SetAlphaAsync([Range(0,100)] uint value)
		=> await SetWrapAsync<uint>(Host.Talker, "Alpha", value);
	// 声質（0～100）を取得または設定します。

	public Task<uint> GetToneScaleAsync()
		=> GetWrapAsync<uint>(Host.Talker, "ToneScale");
	public async ValueTask SetToneScaleAsync([Range(0,100)] uint value)
		=> await SetWrapAsync<uint>(Host.Talker, "ToneScale", value);
	// 抑揚（0～100）を取得または設定します。

	public async Task<ReadOnlyCollection<TalkerComponent>> GetComponentsAsync()
		=> (ReadOnlyCollection<TalkerComponent>)await GetWrapAsync<dynamic>(Host.Talker, "Components");

	/// <summary>
	/// 利用可能なキャスト名 <c>string[]</c> を取得します。
	/// </summary>
	/// <returns></returns>
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

	public Task<bool> StopAsync()
		=> CallWrapAsync<bool>(Host.Talker,nameof(ITalker.Stop));

	public Task<double> GetTextDurationAsync([StringLength(200)] string text)
		=> CallWrapAsync<double>(Host.Talker,nameof(ITalker.GetTextDuration),new(new[]{text}));

	public Task<ReadOnlyCollection<IPhonemeData>> GetPhonemesAsync([StringLength(200)] string text)
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
