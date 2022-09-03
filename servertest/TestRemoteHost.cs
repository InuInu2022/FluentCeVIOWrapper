using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentCeVIOWrapper;
using FluentCeVIOWrapper.Common;
using FluentCeVIOWrapper.Common.Talk;
using FluentCeVIOWrapper.Server;

using Xunit;
using Xunit.Abstractions;

namespace FluentCeVIOWrapper.TestNet48;

public class TestRemoteHost : IDisposable
{
	private readonly ITestOutputHelper output;
	private RemoteHost? host;

	public TestRemoteHost(ITestOutputHelper output){
		this.output = output;

	}
	public void Dispose(){

	}

	private async ValueTask InitAsync(){
		this.host = await RemoteHost.CreateAsync(new Common.Talk.Environment.AI());
		var result = await host.TryStartAsync();
		output.WriteLine($"result:{result}");
		Assert.Equal(HostStartResult.Succeeded, result.result);
	}

	[Fact]

	public async ValueTask SampleTestAsync(){
		await Task.Run(()=>Assert.True(true));
	}

	[Fact]
    public async Task TestAsync()
    {
        await Task.Delay(1000);

        Assert.True(true);

		output.WriteLine($"result");
    }

	[Fact]
    public async void CodeThrowsAsync()
    {
        Func<Task> testCode = () => Task.Factory.StartNew(ThrowingMethod);

        var ex = await Assert.ThrowsAsync<NotImplementedException>(testCode);

        Assert.IsType<NotImplementedException>(ex);
		output.WriteLine($"result:{ex}");
    }
	void ThrowingMethod()
    {
        throw new NotImplementedException();
    }

	[Fact]
	public void TryStart()
	{
		var result = host.TryStartAsync().Result;
		output.WriteLine($"result:{result}");
		Assert.Equal(HostStartResult.Succeeded, result.result);
	}

	[Fact]
	public async void CreateRemoteHostAsync(){

		//var host = await RemoteHost.CreateAsync(new Common.Talk.Environment.AI());
		var result = await host.TryStartAsync();
		Assert.Equal(HostStartResult.Succeeded, result.result);
		output.WriteLine($"result:{result}");	//could not call.... why?
	}

	[Fact]
	public async void GetHostVersionAsync()
	{
		await InitAsync();
		output.WriteLine($"name:{nameof(GetHostVersionAsync)}");
		//var host = await RemoteHost.CreateAsync(new Common.Talk.Environment.AI());
		//var result = await host.TryStartAsync().ConfigureAwait(false);
		//Assert.Equal(HostStartResult.Succeeded, result.result);
		//output.WriteLine($"result:{result}");

		var version = await host.GetPropertyByHostAsync<System.Version>(Host.Service, "HostVersion");
		output.WriteLine($"result:{version}");
		Assert.NotNull(version);
		Assert.Equal(8, version.Major);


	}


	[Fact]
	public async void GetIsHostStartedAsync()
	{
		await InitAsync();
		//IsHostStarted
		//host = await RemoteHost.CreateAsync(new Common.Talk.Environment.AI());
		//Assert.NotNull(host);
		//await host.StartHostAsync();
		var result = await host.GetPropertyByHostAsync<bool>(Host.Service, "IsHostStarted");
		Assert.IsType<bool>(result);
		Assert.True(result);
		await Task.Delay(1000);
		output.WriteLine($"result:{result}");
	}

	[Theory]
	[InlineData("Volume")]
	[InlineData("Speed")]
	public async void GetTalkerPropertyAsync(string name){
		await InitAsync();
		var result = await host.GetPropertyByHostAsync<uint>(Host.Talker, name, true);
		output.WriteLine($"result:{result}");
	}

	[Fact]
	public async void GetVolume()
	{
		await InitAsync();
		var result = await host.GetVolume();
		output.WriteLine($"result:{result}");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	public async void Volume(uint value)
	{
		await InitAsync();
		host.Volume = value;
		var result = host.Volume;
		output.WriteLine($"result:{result}");
		Assert.Equal(value, result);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	public async void Speed(uint value)
	{
		await InitAsync();
		host.Speed = value;
		var result = host.Speed;
		output.WriteLine($"result:{result}");
		Assert.Equal(value, result);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	public async void Tone(uint value)
	{
		await InitAsync();
		host.Tone = value;
		var result = host.Tone;
		output.WriteLine($"result:{result}");
		Assert.Equal(value, result);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	public async void Alpha(uint value)
	{
		await InitAsync();
		host.Alpha = value;
		var result = host.Alpha;
		output.WriteLine($"result:{result}");
		Assert.Equal(value, result);
	}

	[Fact]
	public async void Components()
	{
		await InitAsync();
		var comp = host.Components;

		Assert.IsType<ReadOnlyCollection<TalkerComponent>>(comp);

		comp
			.ToList()
			.ForEach(v => output.WriteLine($"{v.Id},{v.Name},{v.Value}"));
	}

	[Fact]
	public async void AvailableCastsAsync()
	{
		await InitAsync();
		var result = await host.GetPropertyByHostAsync<string[]>(Host.Agent, nameof(ITalker.AvailableCasts));

		Assert.NotNull(result);
		Assert.True(result.Length > 0);
	}

	[Theory]
	[InlineData("さようなら",false)]
	[InlineData("こんにちは",true)]
	[InlineData("本日は晴れのち時々くもりです。",true)]
	public async void SpeakAsync(string text, bool isWait)
	{
		await SpeakInternalAsync(text, isWait);
	}

	[Theory]
	[InlineData("小春六花","本日は晴のち時々くもりです。")]
	[InlineData("IA","本日は晴れのち時々くもりです。")]
	[InlineData("フィーちゃん","本日は晴れのち時々くもりです。")]
	[InlineData("夏色花梨","本日は晴れのち時々くもりです。")]
	public async void SetCastAndSpeak(string cast, string text)
	{
		await InitAsync();
		//await SpeakInternalAsync(text, true);
		Assert.NotNull(host);
		if(host is null)return;

		host.Cast = cast;
		var result = await host.SpeakAsync(text, true);
		Assert.True(result);
	}

	private async Task SpeakInternalAsync(string text, bool isWait = true)
	{
		await InitAsync();
		//var args = new ReadOnlyCollection<dynamic>(new string[] { text });
		//var result = await host!.CallInstanceMethodByHostAsync<dynamic>(Host.Talker, nameof(ITalker.Speak), args);
		var result = await host!.SpeakAsync(text, isWait);
		//Assert.NotNull(result);
		Assert.True(result);

		/*
		if(isWait){
			await Task.Run(() => result.Wait());
			Assert.True(result.IsCompleted);
		}else{
			Assert.False(result.IsCompleted);
		}
		*/
	}

	[Fact]
	public async void Stop()
	{
		await InitAsync();
		var result = await host!.CallInstanceMethodByHostAsync<bool>(Host.Talker, "Stop");

		Assert.True(result);
	}
}
