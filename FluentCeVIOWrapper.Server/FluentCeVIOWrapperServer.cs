using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using FluentCeVIOWrapper.Common;
using FluentCeVIOWrapper.Common.Talk;

using Microsoft.CSharp;
using Microsoft.Extensions.Hosting;

using H.Pipes;
using ConsoleAppFramework;
using FluentCeVIOWrapper.Common.Models;

namespace FluentCeVIOWrapper.Server;

internal class FluentCeVIOWrapperServer : ConsoleAppBase, IDisposable{
	const string pipeName = FluentCeVIO.PipeName;

	private static async Task Main(string[] arguments){
		await Microsoft.Extensions.Hosting.Host
			.CreateDefaultBuilder()
			.RunConsoleAppFrameworkAsync<FluentCeVIOWrapperServer>(arguments);
	}

	public async ValueTask AwakeAsync(
		[Option("cv",$"select a calling CeVIO product. '{nameof(Product.CeVIO_AI)}' or '{nameof(Product.CeVIO_CS)}'")]
		Product cevio = Product.CeVIO_AI,
		string? dirPath = null,
		[Option("p","another server connection pipe name.")]
			string pipename = pipeName
	){
		Console.WriteLine($"Hello, this is the {nameof(FluentCeVIOWrapperServer)}.");

		Common.Talk.Environment.IEnvironment env = cevio switch
		{
			Product.CeVIO_CS
				=> new Common.Talk.Environment.CS(),
			Product.CeVIO_AI or _
				=> new Common.Talk.Environment.AI()
		};
		Console.WriteLine($"	env.Product:{env.Product}");
		Console.WriteLine($"	env.DllPath:{env.DllPath}");
		//Console.WriteLine($"dirPath:{dirPath}");

		if(dirPath is not null){
			env.DllPath = dirPath;
			//Console.WriteLine($"dll path: {dirPath}");
			//Console.WriteLine($"cenv.DllPath:{env.DllPath}");
		}

		var host = await RemoteHost.CreateAsync(env);
		var result = await host.TryStartAsync();
		Console.WriteLine($"RESULT:{result}");
		await Task.Run(
			async () => await RunAsync(pipename).ConfigureAwait(false));

	}

	void IDisposable.Dispose() // NOTE: can not implement `public void Dispose()`
    {
        Console.WriteLine("DISPOSED");
    }

	private static async ValueTask MessageReceivedAsync(
		H.Pipes.Args.ConnectionMessageEventArgs<CeVIOTalkMessage>? args
	)
	{

		Console.WriteLine($"Client {args?.Connection.PipeName} says: {args?.Message}");

		if (args is null)
		{
			return;
		}

		var product = args.Message.Product;
		var cmd = args.Message.ServerCommand;
		//var type = args.Message.ServerCallType;
		var name = args.Message.ServerCallName;

		var setValue = args.Message.ServerCallValue;

		//var argTypes = args.Message.ServerCallArgTypes;
		var argValues = args.Message.ServerCallArgValues;

		//var host = args.Message.ServerHost;
		if (args.Message.ServerHost is null)
		{
			return; //ホスト指定が無ければ何もしない
		}
		var host = (Common.Host)args.Message.ServerHost;

		//var rHost = await RemoteHost.CreateAsync(new Common.Talk.Environment.AI());
		var rHost = RemoteHost.GetInstance(product);
		var isReturnBack = true;

		Console.WriteLine($"■cmd:{cmd},{product},{name},set:{setValue},arg:{argValues},rHost:{rHost.Environment}");

		var value = cmd switch
		{
			ServerCommand.Echo => null,
			ServerCommand.GetProperty =>
				name switch
				{
					//string

					//bool
					nameof(IServiceControl.IsHostStarted) =>
						await rHost.GetPropertyByHostAsync<bool>(host, name),

					//string[]
					nameof(ITalker.AvailableCasts) =>
					//"AvailableCasts" =>
						await rHost.GetPropertyByHostAsync<string[]>(host, name),

					//uint
					nameof(ITalker.Cast) => rHost.Cast,
					nameof(ITalker.Volume) => rHost.Volume,
					nameof(ITalker.Speed) => rHost.Speed,
					nameof(ITalker.Tone) => rHost.Tone,
					nameof(ITalker.Alpha) => rHost.Alpha,
					nameof(ITalker.ToneScale) => rHost.ToneScale,
					//System.Version
					nameof(IServiceControl.HostVersion) =>
						await rHost.GetPropertyByHostAsync<System.Version>(host, name),
					//ReadOnlyCollection<TalkerComponent>
					nameof(ITalker.Components) => rHost.Components,
					_ => null
				},
			ServerCommand.SetProperty =>
				//no waiting
				SetPropertyInternalAsync(name, rHost, setValue),
			ServerCommand.CallMethod =>
				name switch
				{
					//void
					nameof(IServiceControl.CloseHost) =>
						await rHost.CallStaticMethodByHostAsync<ValueTask>(host, name, argValues),
					//HostStartResult
					nameof(IServiceControl.StartHost) =>
						await rHost.StartHostAsync(),
					nameof(ITalker.Speak) =>
						await rHost.SpeakAsync(argValues?[0],true),
					nameof(ITalker.Stop) =>
						await rHost.CallInstanceMethodByHostAsync<bool>(host, name, argValues),
					nameof(ITalker.GetTextDuration) =>
						await rHost.CallInstanceMethodByHostAsync<double>(host, name, argValues),
					nameof(ITalker.GetPhonemes) =>
						await rHost.GetPhonemesAsync(argValues?[0]),
						//await rHost.CallInstanceMethodByHostAsync<ReadOnlyCollection<PhonemeData>>(host, name, argValues),
					nameof(ITalker.OutputWaveToFile) =>
						await rHost.CallInstanceMethodByHostAsync<bool>(host, name, argValues),
					_ => null
				},
			_ => null,
		};

		Console.WriteLine($"- summary: args.msg:{args.Message} cmd:{cmd} name:{name} host:{host} rHost:{rHost} value:{value ?? "no-value"}");
		Console.WriteLine($" {await rHost.GetPropertyByHostAsync<System.Version>(Common.Host.Service, "HostVersion")}");

		if(cmd == ServerCommand.SetProperty){isReturnBack = false;}
		if(isReturnBack){
			await ReturnToClientAsync(
				args,
				new CeVIOTalkMessage
				{
					Product = product,
					ServerCommand = cmd,
					ServerCallName = name,
					//ServerCallType = type,
					ServerCallValue = value,
					//ServerCallArgTypes = argTypes,
					ServerCallArgValues = argValues
				}
			);
			Console.WriteLine($" ★ Return: {cmd}, {name}, {value ?? "no value"}, {argValues}");
		}
	}

	private static async ValueTask<int?> SetPropertyInternalAsync(
		string name,
		RemoteHost rHost,
		dynamic setValue
	){
		await Task.Run(()=>{
			switch (name)
			{
				case nameof(ITalker.Cast):
					{
						rHost.Cast = setValue.ToString();
						break;
					}
				case nameof(ITalker.Volume):
					{
						rHost.Volume = setValue;
						break;
					}

				case nameof(ITalker.Speed):
					{
						rHost.Speed = setValue;
						break;
					}

				case nameof(ITalker.Tone):
					{
						rHost.Tone = setValue;
						break;
					}

				case nameof(ITalker.Alpha):
					{
						rHost.Alpha = setValue;
						break;
					}

				case nameof(ITalker.ToneScale):
					{
						rHost.ToneScale = setValue;
						break;
					}
				case nameof(ITalker.Components):
					{
						rHost.Components = setValue;
						break;
					}

				default:
					break;
			}
		});
		return null;
	}

	private static async ValueTask ReturnToClientAsync(
		H.Pipes.Args.ConnectionMessageEventArgs<CeVIOTalkMessage>? args,
		CeVIOTalkMessage cevioMsg,
		CancellationToken? token = null
	){
		if(args is null)
		{
			Console.WriteLine($" {nameof(ReturnToClientAsync)} args is null!");
			return;
		}

		if(token is null){
			await args.Connection.WriteAsync(cevioMsg).ConfigureAwait(false);
		}else if(token is not null){
			await args.Connection.WriteAsync(cevioMsg, (CancellationToken)token).ConfigureAwait(false);
		}
	}

	public static async ValueTask RunAsync(string pipeName)
	{
		try
		{
			using var source = new CancellationTokenSource();

			Console.WriteLine($"Running in SERVER mode. PipeName: {pipeName}");
			Console.WriteLine("Enter 'q' to exit");

			await using var server = new PipeServer<CeVIOTalkMessage>(pipeName);
			server.ClientConnected += async (_, args) =>
			{
				await Task.Run(() =>
				Console.WriteLine($"Client {args.Connection.PipeName} is now connected!"));


				/*
				try
				{
					await args.Connection.WriteAsync(
						new CeVIOTalkMessage
							{
								Product = Product.CeVIO_AI,
								ServerCommand = "Hello"
							},
						source.Token)
						.ConfigureAwait(false);
				}
				catch (Exception exception)
				{
					OnExceptionOccurred(exception);
				}
				*/
			};
			server.ClientDisconnected += (_, args)
				=> Console.WriteLine($"Client {args.Connection.PipeName} disconnected");
			server.MessageReceived += async (_, args)
				=> await MessageReceivedAsync(args!);//MessageReceivedAsync;
			/*
			(_, args)
				=> Console.WriteLine($"Client {args.Connection.PipeName} says: {args.Message}");
			*/
			server.ExceptionOccurred += (_, args)
				=> OnExceptionOccurred(args.Exception);

			var _ = Task.Run(
				async () =>
				{
					while (true)
					{
						try
						{
							var message = await Console.In.ReadLineAsync().ConfigureAwait(false);
							if (message == "q")
							{
								source.Cancel();
								break;
							}

							Console.WriteLine($"Sent to {server.ConnectedClients.Count} clients");

							await server.WriteAsync(
								new CeVIOTalkMessage
									{
										ServerCommand = "quit"
									},
								source.Token)
								.ConfigureAwait(false);
						}
						catch (Exception exception)
						{
							OnExceptionOccurred(exception);
						}
					}
				},
				source.Token);

			Console.WriteLine("Server starting...");

			await server.StartAsync(cancellationToken: source.Token).ConfigureAwait(false);

			Console.WriteLine("Server is started!");

			await Task.Delay(Timeout.InfiniteTimeSpan, source.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception)
		{
			OnExceptionOccurred(exception);
		}
	}

	private static void OnExceptionOccurred(Exception exception)
	{
		Console.Error.WriteLine($"Exception: {exception}");
	}

}
