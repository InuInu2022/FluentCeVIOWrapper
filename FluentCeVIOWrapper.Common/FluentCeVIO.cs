using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentCeVIOWrapper.Common.Models;
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
	/// 複数サーバーを立てる場合はサーバーと共にこの名前を変更します。
	/// </summary>
	public const string PipeName = "FluentCeVIOPipe";

	/// <summary>
	/// 現在の内部のプロセス間通信で使用される名前です。
	/// </summary>
	public string CurrentPipeName { get; set; }

	/// <summary>
	/// 現在の制御CeVIO製品
	/// </summary>
	public Product CurrentProduct { get; }

	/// <summary>
	/// 現在の制御しているCeVIOのバージョン
	/// </summary>
	public Version? CurrentVersion { get; private set; }

	private readonly CancellationTokenSource source;
	private readonly PipeClient<CeVIOTalkMessage> client;
	private readonly Version CheckLengthVer = new(8, 1, 19);

	internal FluentCeVIO(string? newPipeName, Product product)
	{
		source = new CancellationTokenSource();

		CurrentPipeName = newPipeName ?? PipeName;
		CurrentProduct = product;
		var formatter = new FCWFormatter();
		client = new PipeClient<CeVIOTalkMessage>(
			CurrentPipeName,
			formatter: formatter);
		client.Disconnected += (o, args)
			=> Console.WriteLine($"{nameof(FluentCeVIO)}:Disconnected from server");
		client.Connected += (o, args)
			=> Console.WriteLine($"{nameof(FluentCeVIO)}:Connected to server");
		client.ExceptionOccurred += (o, args)
			=> Console.Error.WriteLine($"Clinet error! : {args.Exception}");
		var _ = client
			.ConnectAsync(source.Token)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 呼び出しのファクトリメソッド
	/// </summary>
	/// <param name="pipeName"><inheritdoc cref="PipeName" path="/summary"/></param>
	/// <param name="product">呼び出すCeVIO製品</param>
	/// <returns></returns>
	public static async ValueTask<FluentCeVIO> FactoryAsync(
		string? pipeName = PipeName,
		Product product = Product.CeVIO_AI
	) => await Task.Run(() => new FluentCeVIO(pipeName, product))
		.ConfigureAwait(false);

	/// <summary>
	/// まとめてパラメータを設定するための準備
	/// メソッドチェーンでパラメータを指定できます
	/// </summary>
	/// <example>
	/// <code>
	/// var fcw = FluentCeVIO.FactoryAsync();
	/// await fcw.CreateParam()
	/// 	.Cast("さとうささら")
	/// 	.Alpha(50)
	/// 	.Speed(75)
	/// 	.SendAsync();
	/// </code>
	/// </example>
	/// <returns></returns>
	public FluentCeVIOParam CreateParam()
		=> FluentCeVIOParam.Create(this);

	void IDisposable.Dispose()
	{
		//client dispose
		var _ = client.DisposeAsync();

		source.Dispose();
	}

	private Task<T> CallWrapAsync<T>(
		Host host,
		string callName,
		ReadOnlyCollection<dynamic>? callArgs = null,
		CancellationToken token = default
	)
	{
		var tcs = new TaskCompletionSource<T>();
		void Handler(object? _, ConnectionMessageEventArgs<CeVIOTalkMessage?> args)
		{
			var result = args?.Message?.ServerCallValue;
			client.MessageReceived -= Handler;

			if(result is null){
				tcs.TrySetException(new NullReferenceException("received message has null referrence."));
			}else if(typeof(T) == typeof(HostStartResult)){
				//castに失敗することがある
				tcs.TrySetResult((T)(object)result);
			}
			else{
				tcs.TrySetResult(result);
			}
		}
		client.MessageReceived += Handler;

		void ErrorHandler(object? _, ExceptionEventArgs args)
		{
			Console.Error.WriteLine($"Error: {args.Exception.Message}");
			client.ExceptionOccurred -= ErrorHandler;
			tcs.TrySetCanceled(token);
		}
		client.ExceptionOccurred += ErrorHandler;

		_ = client.WriteAsync(
			new CeVIOTalkMessage
			{
				ServerCommand = ServerCommand.CallMethod,
				ServerHost = host,
				ServerCallName = callName,
				ServerCallArgValues = callArgs,
				Product = CurrentProduct,
			},
			token
		);

		return tcs.Task;
	}

	private Task<T> GetWrapAsync<T>(
		Host host,
		string propName,
		CancellationToken token = default
	)
	{
		var tcs = new TaskCompletionSource<T>();
		void Handler(
			object? _,
			ConnectionMessageEventArgs<CeVIOTalkMessage?> args)
		{
			var result = args?.Message?.ServerCallValue;
			client.MessageReceived -= Handler;
			if(result is null){
				tcs.TrySetException(new NullReferenceException("received message has null referrence."));
			}else if(typeof(T) == typeof(HostStartResult)){
				//castに失敗することがある
				tcs.TrySetResult((T)(object)result);
			}
			else{
				tcs.TrySetResult(result);
			}
		}
		client.MessageReceived += Handler;
		void ErrorHandler(object? _, ExceptionEventArgs args)
		{
			Console.Error.WriteLine($"Error: {args.Exception.Message}");
			client.ExceptionOccurred -= ErrorHandler;
			tcs.TrySetCanceled(token);
		}
		client.ExceptionOccurred += ErrorHandler;
		_ = client.WriteAsync(
			new CeVIOTalkMessage
			{
				ServerCommand = ServerCommand.GetProperty,
				ServerHost = host,
				ServerCallName = propName,
				Product = CurrentProduct,
			},
			token
		);
		return tcs.Task;
	}

	private Task SetWrapAsync<T>(
		Host host,
		string propName,
		T value,
		CancellationToken token = default
	)
	{
		return client.WriteAsync(
			new CeVIOTalkMessage
			{
				ServerCommand = ServerCommand.SetProperty,
				ServerHost = host,
				ServerCallName = propName,
				ServerCallValue = value,
				Product = CurrentProduct,
			},
			token
		);
	}

	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	private string DebuggerDisplay
	{
		get { return ToString(); }
	}

	#region Host.Service

	//** ------------------------------------------------------------ **/

	/// <summary>
	/// 非同期で起動
	/// 元の<c>StartHost</c>
	/// </summary>
	/// <param name="token"></param>
	public async Task<HostStartResult> StartAsync(
		CancellationToken token = default)
		=> await CallWrapAsync<HostStartResult>(
			Host.Service,
			nameof(IServiceControl.StartHost),
			token:token)
			.ConfigureAwait(false);

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
	public Task<bool> CloseAsync(
		CancellationToken token = default)
		=> CallWrapAsync<bool>(Host.Service, nameof(IServiceControl.CloseHost),token:token);

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
		=> GetWrapAsync<bool>(Host.Service, nameof(IServiceControl.IsHostStarted));

	//** ------------------------------------------------------------ **/

	#endregion

	#region Host.Talker

	//** ------------------------------------------------------------ **/

	/// <summary>
	/// キャスト(話者)を設定します。
	/// </summary>
	/// <param name="castName">キャスト名。利用可能なキャスト名の文字列は<see cref="GetAvailableCastsAsync"/>で取得可。</param>
	/// <param name="noWait"></param>
	/// <seealso cref="GetCastAsync"/>
	/// <seealso cref="GetAvailableCastsAsync"/>
	/// <seealso cref="FluentCeVIOParam.Cast(string)"/>
	public async ValueTask SetCastAsync(string castName, bool noWait = false)
	{
		await SetWrapAsync(Host.Talker, nameof(ITalker.Cast), castName)
			.ConfigureAwait(false);
		if (noWait) return;
		//少し待たないと反映されない
		await Task.Delay(10)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 現在のキャスト(話者)を取得します。
	/// </summary>
	/// <returns>キャスト名</returns>
	/// <seealso cref="SetCastAsync(string,bool)"/>
	public Task<string> GetCastAsync()
		=> GetWrapAsync<string>(Host.Talker, nameof(ITalker.Cast));

	/*
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
	*/

	/// <summary>
	/// 音の大きさ（0～100）を取得します。
	/// </summary>
	/// <returns>音の大きさ（0～100）</returns>
	/// <seealso cref="SetVolumeAsync(uint,bool)"/>
	public Task<uint> GetVolumeAsync()
		=> GetWrapAsync<uint>(Host.Talker, nameof(ITalker.Volume));

	/// <summary>
	/// 音の大きさ（0～100）を設定します。
	/// </summary>
	/// <param name="volume">音の大きさ（0～100）</param>
	/// <param name="noWait"></param>
	/// <seealso cref="GetVolumeAsync"/>
	/// <seealso cref="FluentCeVIOParam.Volume(uint)"/>
	/// <returns></returns>
	public async ValueTask SetVolumeAsync([Range(0, 100)] uint volume, bool noWait = false)
	{
		await SetWrapAsync<uint>(Host.Talker, nameof(ITalker.Volume), volume)
			.ConfigureAwait(false);
		if (noWait) return;
		await Task.Delay(10)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 話す速さ（0～100）を取得します。
	/// </summary>
	/// <returns>話す速さ（0～100）</returns>
	/// <seealso cref="SetSpeedAsync(uint,bool)"/>
	public Task<uint> GetSpeedAsync()
		=> GetWrapAsync<uint>(Host.Talker, nameof(ITalker.Speed));

	/// <summary>
	/// 話す速さ（0～100）を設定します。
	/// </summary>
	/// <param name="value">話す速さ（0～100）</param>
	/// <param name="noWait"></param>
	/// <seealso cref="GetSpeedAsync"/>
	/// <seealso cref="FluentCeVIOParam.Speed(uint)"/>
	/// <returns></returns>
	public async ValueTask SetSpeedAsync([Range(0, 100)] uint value, bool noWait = false)
	{
		await SetWrapAsync<uint>(Host.Talker, nameof(ITalker.Speed), value)
			.ConfigureAwait(false);
		if (noWait) return;
		await Task.Delay(10)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 音の高さ（0～100）を取得します。
	/// </summary>
	/// <returns>音の高さ（0～100）</returns>
	/// <seealso cref="SetToneAsync(uint,bool)"/>
	public Task<uint> GetToneAsync()
		=> GetWrapAsync<uint>(Host.Talker, nameof(ITalker.Tone));

	/// <summary>
	/// 音の高さ（0～100）を設定します。
	/// </summary>
	/// <param name="value">音の高さ（0～100）</param>
	/// <param name="noWait"></param>
	/// <seealso cref="GetToneAsync"/>
	/// <seealso cref="FluentCeVIOParam.Tone(uint)"/>
	/// <returns></returns>
	public async ValueTask SetToneAsync([Range(0, 100)] uint value, bool noWait = false)
	{
		await SetWrapAsync<uint>(Host.Talker, nameof(ITalker.Tone), value)
			.ConfigureAwait(false);
		if (noWait) return;
		await Task.Delay(10)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 声質（0～100）を取得します。
	/// </summary>
	/// <returns>声質（0～100）</returns>
	/// <seealso cref="SetAlphaAsync(uint,bool)"/>
	public Task<uint> GetAlphaAsync()
		=> GetWrapAsync<uint>(Host.Talker, nameof(ITalker.Alpha));

	/// <summary>
	/// 声質（0～100）を設定します。
	/// </summary>
	/// <param name="value">声質（0～100）</param>
	/// <param name="noWait"></param>
	/// <seealso cref="GetAlphaAsync"/>
	/// <seealso cref="FluentCeVIOParam.Alpha(uint)"/>
	/// <returns></returns>
	public async ValueTask SetAlphaAsync([Range(0, 100)] uint value, bool noWait = false)
	{
		await SetWrapAsync<uint>(Host.Talker, nameof(ITalker.Alpha), value)
			.ConfigureAwait(false);
		if (noWait) return;
		await Task.Delay(10)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 抑揚（0～100）を取得します。
	/// </summary>
	/// <returns>抑揚（0～100）</returns>
	/// <seealso cref="SetToneScaleAsync(uint,bool)"/>
	public Task<uint> GetToneScaleAsync()
		=> GetWrapAsync<uint>(Host.Talker, nameof(ITalker.ToneScale));

	/// <summary>
	/// 抑揚（0～100）を設定します。
	/// </summary>
	/// <param name="value">抑揚（0～100）</param>
	/// <param name="noWait"></param>
	/// <seealso cref="GetToneScaleAsync"/>
	/// <seealso cref="FluentCeVIOParam.ToneScale(uint)"/>
	/// <returns></returns>
	public async ValueTask SetToneScaleAsync([Range(0, 100)] uint value, bool noWait = false)
	{
		await SetWrapAsync<uint>(Host.Talker, nameof(ITalker.ToneScale), value)
			.ConfigureAwait(false);
		if (noWait) return;
		await Task.Delay(10)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 現在のキャストの感情パラメータマップコレクションを取得します。
	/// </summary>
	/// <remarks>
	/// 戻り値は、元のAPIと異なり、汎用の<see cref="ReadOnlyCollection{T}"/>で返ります。
	/// </remarks>
	/// <seealso cref="TalkerComponent"/>
	/// <seealso cref="GetCastAsync"/>
	/// <seealso cref="SetCastAsync(string,bool)"/>
	/// <seealso cref="SetComponentsAsync"/>
	/// <returns>感情パラメータの管理オブジェクト<see cref="TalkerComponent"/>の<see cref="ReadOnlyCollection{T}"/></returns>
	public async Task<ReadOnlyCollection<TalkerComponent>> GetComponentsAsync()
	{
		try
		{
			return await GetWrapAsync<ReadOnlyCollection<TalkerComponent>>(Host.Talker, nameof(ITalker.Components))
				.ConfigureAwait(false);
		}
		catch (System.Exception e)
		{
			Debug.WriteLine(e.Message);
			throw;
		}
	}

	/// <summary>
	/// 現在のキャストの感情パラメータマップコレクションを設定します。
	/// </summary>
	/// <param name="value">感情パラメータの管理オブジェクト</param>
	/// <seealso cref="TalkerComponent"/>
	/// <seealso cref="GetComponentsAsync"/>
	public async ValueTask SetComponentsAsync(IEnumerable<TalkerComponent> value){
		await SetWrapAsync(Host.Talker, nameof(ITalker.Components), value)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// 利用可能なキャスト名 <c>string[]</c> を取得します。
	/// </summary>
	/// <remarks>
	/// 備考：インストールされているボイスによります。
	/// </remarks>
	/// <seealso cref="SetCastAsync(string,bool)"/>
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
	/// <param name="segment">セリフ文字列の分割モードの指定。</param>
	/// <param name="token"></param>
	/// <returns>セリフの再生に成功したかどうか。</returns>
	/// <seealso cref="SpeakSegment"/>
	public async Task<bool> SpeakAsync(
		string text,
		bool isWait = true,
		SpeakSegment segment = SpeakSegment.Short,
		CancellationToken token = default
	)
	{
		//null, empty, whitespace
		if(string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)){
			return false;
		}
		//文字数制限しないで高速処理
		if(segment == SpeakSegment.NoCheck){
			token.ThrowIfCancellationRequested();
			return await SpeakInternalAsync(text, isWait)
				.ConfigureAwait(false);
		}

		//文字数制限の確認
		CurrentVersion ??= await this.GetHostVersionAsync()
			.ConfigureAwait(false);
		var isEnglish = await this.IsEnglishCastNameAsync()
			.ConfigureAwait(false);

		var limit = CurrentVersion >= CheckLengthVer ?
			isEnglish ?
				500 : 200 :
					150;

		if(segment == SpeakSegment.Short){
			//なるべく短く
			return await SpeakSplitAsync(text, limit, isWait, token:token)
				.ConfigureAwait(false);
		}

		//なるべく長く
		throw new NotSupportedException($"Not supported yet: {nameof(SpeakSegment.Long)}");
	}

	private async ValueTask<bool> SpeakSplitAsync(
		string longText,
		int limit,
		bool isWait,
		CancellationToken token = default
	)
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(longText));
		using var reader = new StreamReader(stream, Encoding.UTF8);
		var buffer = new char[limit];
		var read = 0;
		var result = true;

		HashSet<char> sep = new(new[] { '。', '、', '．', '，', '.', ',', '！', '？', '!', '?','[',']','「','」','『','』','(',')','（','）','｛','｝','{','}' });

		while(true)
		{
			token.ThrowIfCancellationRequested();

			buffer.Initialize();

			var bytesRead = await reader
				.ReadAsync(buffer, 0, buffer.Length)
				.ConfigureAwait(false);
			if(bytesRead <= 0){
				break;
			}
			ReadOnlyMemory<char> span = buffer.AsMemory(0, bytesRead);
			var index = IndexOf(span, sep);
			var sliceEnd = (index >= 0) ? index + 1 : bytesRead;
			var sliced = new string(buffer, 0, sliceEnd);

			var text = sliced
				.Replace("\n","")
				.Replace("\r","");

			if(!string.IsNullOrEmpty(text)){
				result = await SpeakInternalAsync(text, isWait, token)
					.ConfigureAwait(false);
			}

			read += Encoding.UTF8.GetByteCount(sliced);
			reader.BaseStream.Seek(read, SeekOrigin.Begin);
			reader.DiscardBufferedData();
			Debug.WriteLine($"[{index}],read:{read}, slliceEnd:{sliceEnd},pos:{reader.BaseStream.Position}\n - {sliced}");
		}
		return result;

		static int IndexOf(ReadOnlyMemory<char> span, HashSet<char> sep)
		{
			var index = -1;
			for (var i = 0; i < span.Length; i++)
			{
				var isContains = sep.Contains(span.Span[i]);
				if (isContains)
				{
					index = span.Span.IndexOf(span.Span[i]);
					break;
				}
			}
			return index;
		}
	}

	// internal use
	private Task<bool> SpeakInternalAsync(
		string text,
		bool isWait = true,
		CancellationToken token = default
	)
		=> CallWrapAsync<bool>(
			Host.Talker,
			nameof(ITalker.Speak),
			new(new dynamic[]{text, isWait}),
			token:token
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
	/// <seealso cref="PhonemeData"/>
	public async Task<ReadOnlyCollection<PhonemeData>> GetPhonemesAsync(string text)
	{
		var data = await CallWrapAsync<dynamic>(Host.Talker, nameof(ITalker.GetPhonemes), new(new[] { text }))
			.ConfigureAwait(false);

		var list = new List<PhonemeData>();
		foreach (var item in data)
		{
			list.Add(new PhonemeData(
				item.StartTime,
				item.EndTime,
				item.Phoneme
			));
		}
		return list.AsReadOnly();
	}

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

	//** ------------------------------------------------------------ **/

	#endregion

}
