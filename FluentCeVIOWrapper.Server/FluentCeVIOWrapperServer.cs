using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using FluentCeVIOWrapper.Common;
using FluentCeVIOWrapper.Common.Talk;

using Microsoft.CSharp;

using H.Pipes;

namespace FluentCeVIOWrapper.Server;

internal static class FluentCeVIOWrapperServer{
	const string pipeName = FluentCeVIO.PipeName;

	private static async Task Main(string[] arguments){
		var host = await RemoteHost.CreateAsync(new Common.Talk.Environment.AI());
		var result = await host.TryStartAsync();
		await Task.Run(
			async () => await RunAsync(pipeName).ConfigureAwait(false));
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
		var host = (Host)args.Message.ServerHost;

		//var rHost = await RemoteHost.CreateAsync(new Common.Talk.Environment.AI());
		var rHost = RemoteHost.GetInstance(product);
		var isReturnBack = true;

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
						await rHost.CallInstanceMethodByHostAsync<ReadOnlyCollection<IPhonemeData>>(host, name, argValues),
					nameof(ITalker.OutputWaveToFile) =>
						await rHost.CallInstanceMethodByHostAsync<string>(host, name, argValues),
					_ => null
				},
			_ => null,
		};

		Console.WriteLine($"- summary: args.msg:{args.Message} cmd:{cmd} name:{name} host:{host} rHost:{rHost} value:{value ?? "no-value"}");
		Console.WriteLine($" {await rHost.GetPropertyByHostAsync<System.Version>(Host.Service, "HostVersion")}");

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

							Console.WriteLine($"Sent to {server.ConnectedClients.Count.ToString()} clients");

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
