
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using FluentCeVIOWrapper;
using FluentCeVIOWrapper.Common;
using FluentCeVIOWrapper.Common.Talk;

using H.Pipes;
using H.Pipes.Args;

using Xunit;
using Xunit.Abstractions;

namespace test;

public class UnitTestAwakeServer
{
	public readonly Stopwatch sw;

	//public readonly ITestOutputHelper output;
	public readonly CancellationTokenSource source;
	public readonly PipeClient<CeVIOTalkMessage> client;

	public Process? process;
	public UnitTestAwakeServer()
	{
		//全体で1回初期化
		//this.output = output;

		sw = new System.Diagnostics.Stopwatch();

		source = new CancellationTokenSource();

        client = new PipeClient<CeVIOTalkMessage>("FluentCeVIOPipe");
        client.Disconnected += (o, args)
            => Console.WriteLine("Disconnected from server");
        client.Connected += (o, args)
            => Console.WriteLine("Connected to server");

		client
			.ConnectAsync(source.Token)
			.ConfigureAwait(false);


		//await Task.Delay(2000);

		//
		/*
		Task.Run(async () =>
		{

			await ProcessX
				.StartAsync("cd ..\test")
				.WaitAsync();
            await ProcessX
                      .StartAsync("dotnet run")
                      .WaitAsync();

			//output.WriteLine("test message");
		});
		*/
	}
	public void Dispose()
    {
		// テスト後の処理
		Console.WriteLine("dispose tests.");

        //client dispose
		var _ = client.DisposeAsync();
		//process?.Kill();
		process?.WaitForExit();
		//process = null;

        source.Dispose();
	}
}

public class UnitTestServer : IClassFixture<UnitTestAwakeServer>, IDisposable
{
	private readonly ITestOutputHelper output;
	private readonly CancellationTokenSource source;
	private readonly PipeClient<CeVIOTalkMessage> client;
	private readonly Stopwatch sw;

	public UnitTestServer(UnitTestAwakeServer testAwake, ITestOutputHelper output)
	{
		//set ups
		this.output = output;
		this.source = testAwake.source;
		this.client = testAwake.client;
		this.sw = testAwake.sw;

		output.WriteLine("init tests.");


	}

    public void Dispose()
    {
		// テスト後の処理
		output.WriteLine("dispose tests.");
	}


	[Fact]
    public void TestSample()
    {
		const int num = 1 + 1;
		num.Should().Be(2);

		output.WriteLine("finish");
	}

	[Trait("Category", "Simple")]
    [Fact(Skip = "コンストラクタで実施済み")]
    public async void ConnectAsync(){
        output.WriteLine("Client connecting...");
		await client
			.ConnectAsync(/*source.Token*/)
			.ConfigureAwait(false);
        //await Task.Delay(100, source.Token).ConfigureAwait(false);

		//source.Cancel();
		//true.Is(true);
	}

	[Trait("Category", "Simple")]
	[Fact]
	public async void MessageReceivedAsync(){
		client.MessageReceived += (_, args) =>
		{
			output.WriteLine($"Client {args?.Connection.PipeName} says: {args?.Message}");
		};

		//await client
		//	.ConnectAsync(/*source.Token*/)
		//	.ConfigureAwait(false);


		await client.WriteAsync(
			new CeVIOTalkMessage
				{
					ServerCommand = ServerCommand.Echo,
					ServerHost = Host.Service
				});
	}

	//[Trait("Category", "ServiceMethod")]
	//[Fact]
	private async Task StartDirectAsync(){
		HostStartResult? result = null;

		client.MessageReceived += async (_, args) =>
		{
			output.WriteLine($"StartDirectAsync msg received. ");

			await Task.Run(() =>
			{
				output.WriteLine($"StartDirectAsync msg received.2");
				output.WriteLine($"StartDirectAsync msg received.2.1");
				result = args?.Message?.ServerCallValue;
				output.WriteLine($"StartDirectAsync msg received.2.2");
				output.WriteLine($"StartDirectAsync result : {result?.ToString()}");
				result.Should().NotBeNull();
				result.Should().Be(
					HostStartResult.Succeeded,
					$"ホストが正しく起動していない：{result.ToString()}");
			});
			output.WriteLine($"StartDirectAsync msg received. 3");
			output.WriteLine($"StartDirectAsync result 2: {result?.ToString()}");
		};

		await client.WriteAsync(
			new CeVIOTalkMessage{
				ServerCommand = ServerCommand.CallMethod,
				ServerHost = Host.Service,
				ServerCallName = "StartHost",
			}
		);

		output.WriteLine($"StartDirectAsync result 3: {result?.ToString()}");

		await Task.Delay(100);

		output.WriteLine($"StartDirectAsync result 4: {result?.ToString()}");

	}

	//[Fact]
	public async ValueTask StartRxAsync(){



		output.WriteLine($"StartRxAsync msg received.1");



		output.WriteLine($"StartRxAsync msg received.2");



		var t = InternalAsync();
		var t2 = client.WriteAsync(
			new CeVIOTalkMessage{
				ServerCommand = ServerCommand.CallMethod,
				ServerHost = Host.Service,
				ServerCallName = "StartHost",
			}
		);

		var result2 = await InternalAsync();

		await Task.Delay(100);
		output.WriteLine($"StartRxAsync msg received.3");
		output.WriteLine($"StartRxAsync result : {result2?.ToString()}");
		result2.Should().NotBeNull();
		result2.Should().Be(
			HostStartResult.Succeeded,
			$"ホストが正しく起動していない：{result2.ToString()}");

		output.WriteLine($"StartRxAsync msg received.4");

		Assert.Equal(HostStartResult.Succeeded, result2);
	}

	internal Task<HostStartResult?> InternalAsync(){
		var tcs = new TaskCompletionSource<HostStartResult?>();
		//HostStartResult? result = null;
		void handler(object? _, ConnectionMessageEventArgs<CeVIOTalkMessage?> args)
		{
			var result = args?.Message?.ServerCallValue;
			client.MessageReceived -= handler;
			tcs.TrySetResult(result);
		}

		client.MessageReceived += handler;

		return tcs.Task;
	}


	[Fact]
	public async void StartWrapperAsync(){
		output.WriteLine($"{nameof(StartWrapperAsync)}: test start.");
		var fcw = await FluentCeVIO.FactoryAsync();
		output.WriteLine($"{nameof(StartWrapperAsync)}: call factory");
		var result = await fcw.StartAsync();
		output.WriteLine($"{nameof(StartWrapperAsync)}: call start host");
		//await Task.Delay(100);
		output.WriteLine($"{nameof(StartWrapperAsync)}: delay");

		result.Should().Be(
			HostStartResult.Succeeded,
			$"ホストが正しく起動していない：{result.ToString()}");

		output.WriteLine($"{nameof(StartWrapperAsync)}: test finished. {result}");

		Assert.Equal(HostStartResult.Succeeded, result);
	}

	[Fact]
	public async void GetHostVersionAsync()
	{
		var fcw = await FluentCeVIO.FactoryAsync();
		var result = await fcw.GetHostVersionAsync();
		//System.Version? result = null;
		//output.WriteLine($"result: {result}");
		Console.WriteLine($"result: {result}");
		Debug.WriteLine($"result: {result}");
		//Assert.True(result is null);

		Assert.Equal(8, actual: result.Major);
		output.WriteLine($"result: {result}");
		Console.WriteLine($"result: {result}");
		Debug.WriteLine($"result: {result}");


	}

	[Theory]
	[InlineData("さとうささら")]
	[InlineData("小春六花")]
	public async void CastAsync(string cast){
		output.WriteLine($"{nameof(CastAsync)}: cast: {cast}");
		var fcw = await FluentCeVIO.FactoryAsync();
		await fcw.SetCastAsync(cast);

		var result = await fcw.GetCastAsync();

		result.Should().NotBeNull();
		result.Should().Be(cast);
	}

	[Fact]
	public async void GetCastAsync()
	{
		var fcw = await FluentCeVIO.FactoryAsync();

		var result = await fcw.GetCastAsync();
		output.WriteLine($"{nameof(GetCastAsync)}: cast: {result}");

		Assert.False(string.IsNullOrEmpty(result));
	}

	[Theory]
	[InlineData("さとうささら")]
	[InlineData("小春六花")]
	public async void CastTestAsync(string value)
	{
		var fcw = await FluentCeVIO.FactoryAsync();
		sw.Restart();
		sw.Start();
		//fcw.Cast = value;
		await fcw.SetCastAsync(value);
		sw.Stop();
		output.WriteLine($"set cast[{value}]: {sw.ElapsedMilliseconds} msec.");
		sw.Start();
		//var result = fcw.Cast;
		var result = await fcw.GetCastAsync();
		sw.Stop();
		output.WriteLine($"get cast[{result}]: {sw.ElapsedMilliseconds} msec.");

		Assert.True(value == result);
	}

	private class TestDataRand0to100 : TheoryData<uint>{
		public TestDataRand0to100(){
			var rand = new Random();
			Add((uint)rand.Next(0, 100));
			Add((uint)rand.Next(0, 100));
			Add((uint)rand.Next(0, 100));
		}
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	[ClassData(typeof(TestDataRand0to100))]
	public async void VolumeAsync(uint value)
	{
		output.WriteLine($"{nameof(VolumeAsync)}: value:{value}");
		var fcw = await FluentCeVIO.FactoryAsync();
		await fcw.SetVolumeAsync(value);
		var result = await fcw.GetVolumeAsync();
		output.WriteLine($"set {value}, get {result}, result: {value == result}");
		Assert.True(value == result);

		output.WriteLine($"test finished. {result}");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	public async void SpeedAsync(uint value)
	{
		output.WriteLine($"{nameof(SpeedAsync)}: value:{value}");
		var fcw = await FluentCeVIO.FactoryAsync();
		await fcw.SetSpeedAsync(value);
		var result = await fcw.GetSpeedAsync();
		output.WriteLine($"set {value}, get {result}, result: {value == result}");
		Assert.True(value == result);

		//await Task.Delay(100);

		output.WriteLine($"test finished. {result}");
	}

	[Theory]
	[InlineData("さとうささら",4)]
	[InlineData("フィーちゃん",5)]
	[InlineData("小春六花",5)]
	[InlineData("ONE",4)]
	[InlineData("OИE",4)]
	public async void GetComponentsAsync(string cast, int num)
	{
		var fcw = await FluentCeVIO.FactoryAsync();
		//fcw.Cast = cast;
		await fcw.SetCastAsync(cast);
		var result = await fcw.GetComponentsAsync();

		var currentCast = await fcw.GetCastAsync();
		output.WriteLine($"cast:{currentCast}");

		Assert.NotNull(result);
		Assert.True(result.Count > 0);

		Assert.Equal(num, result.Count);

		result
			.ToList()
			.ForEach(v => output.WriteLine($"comp[{v.Name}]:{v.Value}"));
	}

	[Theory]
	[InlineData("夏色花梨")]
	public async void SetComponentsAsync(string cast){
		var fcw = await FluentCeVIO.FactoryAsync();

		await fcw.SetCastAsync(cast);
		var result = await fcw.GetComponentsAsync();

		try{
			await fcw.SetComponentsAsync(result);
			await fcw.SpeakAsync("感情を設定しました。");
		}catch{
			Debug.WriteLine("e");
		}
	}

	[Fact]
	public async void GetAvailableCastsAsync()
	{
		var fcw = await FluentCeVIO.FactoryAsync();
		var result = await fcw.GetAvailableCastsAsync();

		Assert.True(result.Length > 0);

		result.ToList().ForEach(v =>
			output.WriteLine($"casts:{v}")
		);
	}

	[Theory]
	[InlineData("こんにちは")]
	public async void SpeakAsync(string text)
	{
		var fcw = await FluentCeVIO.FactoryAsync();
		await fcw.SetSpeedAsync(50);
		await fcw.SetToneAsync(50);
		await fcw.SetAlphaAsync(50);
		var result = await fcw.SpeakAsync(text);

		Assert.True(result);
	}

	[Theory]
	[InlineData("夏色花梨","夏色花梨です")]
	public async void GetPhonemesAsync(string cast, string text)
	{
		var fcw = await FluentCeVIO.FactoryAsync();
		await fcw.SetCastAsync(cast);
		var comp = await fcw.GetComponentsAsync();
		comp.ToList().ForEach(v =>
		{
			output.WriteLine($"{v.Name}:{v.Value}");
		});
		await fcw.SetSpeedAsync(50);
		await fcw.SetToneAsync(50);
		await fcw.SetAlphaAsync(50);
		var speed = await fcw.GetSpeedAsync();
		output.WriteLine($"speed: {speed}");

		var result = await fcw.GetPhonemesAsync(text);

		result.ToList().ForEach(v => {
			var ts = TimeSpan.FromSeconds(v.EndTime - v.StartTime);

			output.WriteLine($"phonemes: {v.StartTime * 1000 * 1000 * 10}	{v.EndTime * 1000 * 1000 * 10}	{v.Phoneme}");
		});

		Assert.True(result.Count > 0);
	}

	[Theory]
	[InlineData("こんにちは",@"..\..\..\out\こんにちは.wav")]
	public async void SaveWaveAsync(string serif, string path)
	{
		// Given
		var fcw = await FluentCeVIO.FactoryAsync();
		var npath = Path.GetFullPath(path);
		var result = await fcw.OutputWaveToFileAsync(serif, npath);

		// When
		output.WriteLine($"npath:{npath}");

		// Then
		//Assert.True(Directory.Exists(npath));
		Assert.True(result);
	}

	[Theory]
	[InlineData("こんにちは","すずきつづみ", 45, 33,0,100,100)]
	public async void SendAndSpeakTestAsync(
		string text,
		string cast,
		uint speed,
		uint alpha,
		uint tone,
		uint toneScale,
		uint vol
	)
	{
		var fcw = await FluentCeVIO.FactoryAsync();
		await fcw
			.CreateParam()
			.Alpha(alpha)
			.Cast(cast)
			.Tone(tone)
			.ToneScale(toneScale)
			.Speed(speed)
			.Volume(vol)
			.SendAndSpeakAsync(text);

		var tmpAlpha = await fcw.GetAlphaAsync();
		tmpAlpha.Should().Be(alpha);

		var tmpCast = await fcw.GetCastAsync();
		tmpCast.Should().Be(cast);

		var tmpSpeed = await fcw.GetSpeedAsync();
		tmpSpeed.Should().Be(speed);

		var tmpTone = await fcw.GetToneAsync();
		tmpTone.Should().Be(tone);

		var tmpToneScale = await fcw.GetToneScaleAsync();
		tmpToneScale.Should().Be(toneScale);

		var tmpVol = await fcw.GetVolumeAsync();
		tmpVol.Should().Be(vol);
	}

	[Theory]
	[InlineData("さとうささら", Product.CeVIO_AI, "CTNV-JPF-A" )]
	//[InlineData("さとうささら", Product.CeVIO_CS, "A" )]
	public async void GetCastIdAsync(
		string name, Product product, string id
	){
		var fcw = await FluentCeVIO
			.FactoryAsync(FluentCeVIO.PipeName,product);
		await fcw
			.CreateParam()
			.Cast(name)
			.SendAsync();

		var comps = await fcw.GetComponentsAsync();
		comps.ToList().ForEach(v => output.WriteLine($"id:{v.Id}, {v.Name}, {v.Value}"));
		//output.WriteLine(comps[0].Id);

		var getId = await fcw.GetCastIdAsync(name);
		getId.Should().Be(id);
	}

	[Theory]
	[InlineData(Product.CeVIO_AI)]
	public async void SpeakLongTextAsync(
		Product product
	)
	{
		var fcw = await FluentCeVIO
			.FactoryAsync(FluentCeVIO.PipeName,product);

		var text = @"
吾輩は猫である。名前はまだ無い。
　どこで生れたかとんと見当がつかぬ。何でも薄暗いじめじめした所でニャーニャー泣いていた事だけは記憶している。吾輩はここで始めて人間というものを見た。しかもあとで聞くとそれは書生という人間中で一番獰悪な種族であったそうだ。この書生というのは時々我々を捕えて煮て食うという話である。しかしその当時は何という考もなかったから別段恐しいとも思わなかった。ただ彼の掌に載せられてスーと持ち上げられた時何だかフワフワした感じがあったばかりである。掌の上で少し落ちついて書生の顔を見たのがいわゆる人間というものの見始であろう。この時妙なものだと思った感じが今でも残っている。第一毛をもって装飾されべきはずの顔がつるつるしてまるで薬缶だ。その後猫にもだいぶ逢ったがこんな片輪には一度も出会わした事がない。のみならず顔の真中があまりに突起している。そうしてその穴の中から時々ぷうぷうと煙を吹く。どうも咽せぽくて実に弱った。これが人間の飲む煙草というものである事はようやくこの頃知った。
この書生の掌の裏でしばらくはよい心持に坐っておったが、しばらくすると非常な速力で運転し始めた。書生が動くのか自分だけが動くのか分らないが無暗に眼が廻る。胸が悪くなる。到底助からないと思っていると、どさりと音がして眼から火が出た。それまでは記憶しているがあとは何の事やらいくら考え出そうとしても分らない。
　ふと気が付いて見ると書生はいない。たくさんおった兄弟が一疋も見えぬ。肝心の母親さえ姿を隠してしまった。その上今までの所とは違って無暗に明るい。眼を明いていられぬくらいだ。はてな何でも容子がおかしいと、のそのそ這い出して見ると非常に痛い。吾輩は藁の上から急に笹原の中へ棄てられたのである。
";
		var result = await fcw.SpeakAsync(text);

		result.Should().Be(true);
	}
}
