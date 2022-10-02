using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using FluentCeVIOWrapper.Common;
using FluentCeVIOWrapper.Common.Models;
using FluentCeVIOWrapper.Common.Talk;
using FluentCeVIOWrapper.Common.Talk.Environment;

namespace FluentCeVIOWrapper.Server;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class RemoteHost
{
    public Product Product { get; internal set; }
    public IEnvironment Environment { get; set; }

    public bool Exists { get; private set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string DebuggerDisplay => ToString();

	private static readonly List<RemoteHost> insts = new();
	private Type? service;
	private Assembly? assembly;
	private Type? agent;
	private Type? talkerType;
	private dynamic? talker;
	//private dynamic? agentInst;
	//private dynamic? serviceInst;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <remarks>
	/// Dont call directory. Use <see cref="RemoteHost.Factory"/>
	/// </remarks>
	/// <param name="product"></param>
	private RemoteHost(IEnvironment env)
	{
		Product = env.Product;
		Environment = env;
	}

	/// <summary>
	/// <see cref="RemoteHost"/>インスタンスを非同期に生成する
	/// </summary>
	/// <param name="env">別パスインストール時は書き換えたIEnvironmentインスタンスを渡す</param>
	/// <returns><see cref="RemoteHost"/>インスタンス</returns>
	public static async ValueTask<RemoteHost> CreateAsync(
		IEnvironment env
    ){
		env ??= new AI();
		return await Task.Run(async () =>
		{
			var hasInstance = insts.Exists(m => m.Product == env.Product);
            if(hasInstance){
				return insts.FindLast(m => m.Product == env.Product);
			}else{
				var remoteHost = new RemoteHost(env)
				{
					Exists = await CheckExistsAsync(env)
				};
				insts.Add(remoteHost);
				return remoteHost;
			}
		});
	}

    public static async ValueTask<bool> CheckExistsAsync(
        IEnvironment env
	){
        return await Task.Run(() =>
            System.IO.File.Exists(
                System.IO.Path.Combine(
                    env.DllPath,
                    env.DllName
                )
            )
        );
    }

    public static List<RemoteHost> GetInstances(){
		return insts;
	}
	public static RemoteHost GetInstance(Product product){
		return insts.FindLast(m => m.Product == product);
	}

    public async ValueTask<(bool success, HostStartResult result, string reason)> TryStartAsync(){
        if(!this.Exists){
            return (
                success:Exists,
                result:HostStartResult.FileNotFound,
                reason:$"dll not found. path:{Environment.DllPath}, dll:{Environment.DllName}"
			);
        }

        string cevioPath = System.IO.Path.Combine(Environment.DllPath,Environment.DllName);

        //dll load check
		try{
			assembly = await Task.Run(()=> Assembly.LoadFrom(cevioPath));
        }catch(Exception e){
            return (false, HostStartResult.FileNotFound,$"Load error: {e.Message}");
        }

		//service check
		try
		{
			service = assembly.GetType(Environment.Service);

			if (service is null)
			{
				//TODO:logging
				return (false, HostStartResult.FileNotFound,"dll call error: serice is null");
			}
			//serviceInst = Activator.CreateInstance(service, new object[] { });
		}
		catch (Exception e)
		{
            //TODO:logging
            return (false, HostStartResult.HostError,$"failed to get service. error: {e.Message}");
		}

		//try start
		try
        {
			var result = await StartHostAsync();
			//MethodInfo startHost = service.GetMethod("StartHost");
			//var result = await Task.Run(()=> startHost.Invoke(null, new object[] { false }));

			if ((int)result > 1)
            {
                //TODO:logging
                return (false, result, $"failed to awake. reason code:{result.ToString()}");
            }
        }
        catch (System.Exception e)
        {
            //TODO:logging
            return (false, HostStartResult.StartingFailed, $"failed to awake. error: {e.Message}");
		}

		//talk agent check
		string[] names = new string[]{};
		try
        {
            agent = assembly.GetType(Environment.Agent);
			//agentInst = Activator.CreateInstance(agent, new object[] { });
			names = await GetAgentPropertyAsync<string[]>("AvailableCasts");
			//PropertyInfo property = agent.GetProperty("AvailableCasts");
			//names = await Task.Run(()=> (string[])property.GetValue(null, new object[] { }));

            if(names is null || names.Length == 0){
				return (false, HostStartResult.NotRegistered, "no talk cast available.");
			}
        }
        catch (System.Exception e)
        {
            //TODO:logging
            return (false, HostStartResult.HostError, $"agent error: {e.Message}");
        }


        //talker check
        try
        {
            talkerType = assembly.GetType(Environment.Talker);
            talker = Activator.CreateInstance(talkerType, new object[] { names[0] });
        }
        catch (Exception ex)
        {
            //TODO:logging
            return (false, HostStartResult.HostError, $"can't awake cevio talker. {ex.Message}");
        }

		//success
		return (true, HostStartResult.Succeeded, "start sucess.");
	}

    internal async ValueTask<T> GetPropertyAsync<T>(Type type, string name,dynamic? instance = null){
		try{
			PropertyInfo property = type.GetProperty(name);
			var target = instance;

			/*
			var value = await Task.Run(()=> property.GetValue(target, new object[] { }));
			if(target is not null)
			{
				target[name]
			}*/

			var value = target switch
			{
				//not null => target[name],
				_ => await Task.Run(()=> property.GetValue(null, new object[] { }))
			};

			return (T)value;
		}catch (Exception e){
			Console.WriteLine($"[{name}] {e}: {type}");
			throw new Exception($"[{name}] {e}: '{type}'");
		}
	}

	internal async ValueTask SetPropertyAsync<T>(Type type, string name, T value){
		PropertyInfo property = type.GetProperty(name);
		await Task.Run(() => property.SetValue(null, value));
	}

    internal async ValueTask<T> CallMethodAsync<T>(Type type, string name, object[]? args = null, dynamic? instance = null){
        MethodInfo method = type.GetMethod(name);
        var result = await Task.Run(()=> method.Invoke(instance, args));
		return (T)result;
	}


	private Type SwitchHost(Host host)
	{
		return host switch
		{
			Host.Agent => agent!,
			Host.Service => service!,
			Host.Talker => talkerType!,
			_ => service!
		};
	}

	private dynamic? SwitchInstance(Host host){
		return host switch
		{
			Host.Talker => talker,
			Host.Agent => null,
			Host.Service => null,	//ServiceControl(2) has no instance prop/method
			_ => null,
		};
	}

	/// <summary>
	/// プロパティ所得汎用
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="host"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	public async ValueTask<T> GetPropertyByHostAsync<T>(Host host, string name, bool isInstance = false)
	{
		Type type = SwitchHost(host);
		var inst = isInstance ? SwitchInstance(host) : null;
		return await GetPropertyAsync<T>(type, name, inst).ConfigureAwait(false);
	}

	/// <summary>
	/// プロパティ設定汎用
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="host"></param>
	/// <param name="name"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public async ValueTask SetPropertyByHostAsync<T>(Host host, string name, T value){
		var type = SwitchHost(host);
		await SetPropertyAsync<T>(type!, name, value);
	}

	/// <summary>
	/// メソッド呼び出し汎用
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="host"></param>
	/// <param name="name"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public async ValueTask<T> CallStaticMethodByHostAsync<T>(
		Host host,
		string name,
		ReadOnlyCollection<dynamic>? args = null
	){
		var type = SwitchHost(host);
		var array = (args?.Count > 0)
			? new List<dynamic>(args).ToArray()
			: Array.Empty<dynamic>();
		return await CallMethodAsync<T>(type!, name, array);
	}

	public async ValueTask<T> CallInstanceMethodByHostAsync<T>(
		Host host,
		string name,
		ReadOnlyCollection<dynamic>? args = null
	){
		var type = SwitchHost(host);
		var inst = SwitchInstance(host);
		var array = (args?.Count > 0)
			? new List<dynamic>(args).ToArray()
			: Array.Empty<dynamic>();
		return await CallMethodAsync<T>(type!, name, array, inst);
	}

	public async ValueTask<HostStartResult> StartHostAsync(){
		var arg = new ReadOnlyCollection<dynamic>(new dynamic[]{ false });
		return await CallStaticMethodByHostAsync<HostStartResult>(Host.Service, "StartHost", arg);
	}

	public async ValueTask<T> GetServicePropertyAsync<T>(string name)
        => await GetPropertyAsync<T>(service!, name);

	public async ValueTask<T> CallServiceMethodAsync<T>(string name, object[]? args)
        => await CallMethodAsync<T>(service!, name, args);

	public async ValueTask<T> GetAgentPropertyAsync<T>(string name)
        => await GetPropertyAsync<T>(agent!, name);

	public async ValueTask<T> CallAgentMethodAsync<T>(string name, object[]? args)
        => await CallMethodAsync<T>(agent!, name, args);

	public async ValueTask<T> GetTalkerPropertyAsync<T>(string name)
        => await GetPropertyAsync<T>(talker!, name);

	public async ValueTask<T> SetTalkerPropertyAsync<T>(string name, T value)
        => await SetPropertyAsync<T>(talker!, name, value);

	public async ValueTask<T> CallTalkerMethodAsync<T>(string name, object[]? args)
        => await CallMethodAsync<T>(talker!, name, args);


	public async ValueTask<uint> GetVolume(){
		if(talker is null)
		{
			throw new NullReferenceException("GetVolume: talker is null");
		}

		return await Task.Run( ()=> this.talker?.Volume);
	}

	public uint Volume {
		get => this.talker?.Volume;
		set => this.talker!.Volume = value;
	}


	/// <summary>
	/// 話す速さ（0～100）を取得または設定します。
	/// </summary>
	public uint Speed{
		get => this.talker?.Speed;
		set => this.talker!.Speed = value;
	}

	/// <summary>
	/// 音の高さ（0～100）を取得または設定します。
	/// </summary>
	public uint Tone{
		get => this.talker?.Tone;
		set => this.talker!.Tone = value;
	}

	/// <summary>
	/// 声質（0～100）を取得または設定します。
	/// </summary>
	public uint Alpha{
		get => this.talker?.Alpha;
		set => this.talker!.Alpha = value;
	}

	/// <summary>
	/// 抑揚（0～100）を取得または設定します。※バージョン4.0.7.0追加。
	/// </summary>
	public uint ToneScale{
		get => this.talker?.ToneScale;
		set => this.talker!.ToneScale = value;
	}

	public ReadOnlyCollection<TalkerComponent>? Components{
		get {
			var comps = this.talker?.Components;
			if(comps is null)return null;

			List<TalkerComponent> list = new();
			foreach(var i in comps){
				//TalkerComponent2も同じ型にキャスト
				var tc = new TalkerComponent(i.Id, i.Name, i.Value);
				list.Add(tc);
			}
			return new(list);
		}

		set {
			var comps = this.talker?.Components;
			if(comps is null)return;
			var news = value.ToList();

			foreach(var i in comps){
				i.Value = news
					.First(v => v.Id == i.Id).Value;
			}

			talker!.Components = comps;
		}
	}

	public async ValueTask<ReadOnlyCollection<PhonemeData>> GetPhonemesAsync(string text)
	{
		return await Task.Run(() =>
		{
			var data = this.talker?.GetPhonemes(text);

			var list = new List<PhonemeData>();
			if(data is null)
			{
				return list.AsReadOnly();
			}

			foreach (var item in data)
			{
				list.Add(new PhonemeData(
					item.StartTime,
					item.EndTime,
					item.Phoneme
				));
			}

			return list.AsReadOnly();
		});

	}

	/// <summary>
	/// キャストを取得または設定します。
	/// </summary>
	public string Cast{
		get => this.talker?.Cast ?? "";
		set => this.talker!.Cast = value;
	}

	public async ValueTask<bool> SpeakAsync(string text, bool isWait){
		return await Task.Run(() =>
		{
			var result = talker?.Speak(text);
			if(result is null)
			{
				return false;
			}

			if(isWait){
				result.Wait();
				return (bool)result.IsSucceeded;
			}else{
				//await Task.Delay(100);
				return (bool)result.IsSucceeded;
			}
		});
	}

}
