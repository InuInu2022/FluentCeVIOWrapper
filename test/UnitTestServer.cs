
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
	public async Task StartDirectAsync(){
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

		result.Should().NotBeNull();
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

		Assert.True(cast == result);
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
}
