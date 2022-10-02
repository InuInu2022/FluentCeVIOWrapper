using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

namespace processtest;

public class ProcessTest
{
	readonly Dictionary<string, Process?> process = new();

	[Theory]
	[InlineData("CeVIO_AI","Release")]
    [InlineData("CeVIO_AI","Debug")]
	[InlineData("CeVIO_CS","Debug")]
    public async void AwakeProcessOnce(string TTS, string build)
	{
		if (process is not null && process.ContainsKey(TTS) && !process[TTS]!.HasExited)
		{
			return;
		}

		var ps = new ProcessStartInfo()
		{
			FileName = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory,
				@"Server\FluentCeVIOWrapper.Server.exe"
			),
			Arguments = $"-cevio {TTS}",
			//CreateNoWindow = true
			//UseShellExecute = false
		};
		process!.Add(TTS, Process.Start(ps));
		//process![TTS] = Process.Start(ps);
		await Task.Delay(2000);

		Assert.NotNull(process[TTS]);
		Assert.True(!process[TTS]?.HasExited);

		process[TTS]?.Kill();
	}

	[Theory]
    [InlineData("CeVIO_AI")]
	[InlineData("CeVIO_CS")]
    public async void AwakeProcessKuchiPaku(string TTS)
	{
		if (process is not null && process.ContainsKey(TTS) && !process[TTS]!.HasExited)
		{
			return;
		}

		var ps = new ProcessStartInfo()
		{
			FileName = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory,
				@"Server\FluentCeVIOWrapper.Server.exe"
			),
			Arguments = $"-cevio {TTS}",
			//CreateNoWindow = true
			//UseShellExecute = false
		};
		process!.Add(TTS, Process.Start(ps));
		//process![TTS] = Process.Start(ps);
		await Task.Delay(2000);

		Assert.NotNull(process[TTS]);
		Assert.True(!process[TTS]?.HasExited);

		process[TTS]?.Kill();
	}

	[Theory]
	[InlineData("CeVIO_AI","夏色花梨","夏色花梨です")]
	public async void GetPhoneme(
		string TTS,
		string cast,
		string text)
	{

	}
}