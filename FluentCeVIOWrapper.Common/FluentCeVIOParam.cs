using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using FluentCeVIOWrapper.Common.Talk;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// CeVIOにまとめてメソッドチェーンでパラメータを指定できるパラメータクラス
/// </summary>
public class FluentCeVIOParam
{
	private readonly FluentCeVIO _fcw;
	private readonly Dictionary<string, dynamic> sendParams
		= new();
	private Dictionary<string, uint> sendEmotions
		= new();

	internal FluentCeVIOParam(FluentCeVIO fcw){
		_fcw = fcw;
	}

	/// <summary>
	/// メソッドチェーンでパラメータを指定できるパラメータクラスのファクトリメソッド
	/// 最後に <see cref="SendAsync"/>を呼ぶ
	/// </summary>
	/// <seealso cref="FluentCeVIO.CreateParam"/>
	/// <seealso cref="SendAsync"/>
	/// <param name="fcw"><see cref="FluentCeVIO"/>インスタンス</param>
	public static FluentCeVIOParam Create(FluentCeVIO fcw)
		=> new(fcw);

	/// <inheritdoc cref="FluentCeVIO.SetAlphaAsync(uint)"/>
	public FluentCeVIOParam Alpha([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetAlphaAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetCastAsync(string)"/>
	public FluentCeVIOParam Cast(string castName)
		=> SetParam(nameof(FluentCeVIO.SetCastAsync), castName);

	///<inheritdoc cref="FluentCeVIO.SetComponentsAsync(IEnumerable{TalkerComponent})"/>
	///<see cref="FluentCeVIO.GetComponentsAsync"/>
	public FluentCeVIOParam Components(IEnumerable<TalkerComponent> value)
		=> SetParam(nameof(FluentCeVIO.SetComponentsAsync), value);

	/// <summary>
	/// <c>Components</c>の簡易版。
	/// </summary>
	/// <example>
	/// .Emotions(new(){["怒り"]=15,["普通"]=50})
	/// </example>
	/// <param name="list">感情名、値（0~100）のDictionaryを与えてください</param>
	/// <see cref="Components(IEnumerable{TalkerComponent})"/>
	public FluentCeVIOParam Emotions(Dictionary<string,uint> list){
		sendEmotions = list;
		return SetParam(nameof(FluentCeVIOParam.Emotions), sendEmotions);
	}

	/// <inheritdoc cref="FluentCeVIO.SetSpeedAsync(uint)"/>
	public FluentCeVIOParam Speed([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetSpeedAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetToneAsync(uint)"/>
	public FluentCeVIOParam Tone([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetToneAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetToneScaleAsync(uint)"/>
	public FluentCeVIOParam ToneScale([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetToneScaleAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetVolumeAsync(uint)"/>
	public FluentCeVIOParam Volume([Range(0,100)] uint volume)
		=> SetParam(nameof(FluentCeVIO.SetVolumeAsync), volume);

	/// <summary>
	/// メソッドチェーンで指定したパラメータをまとめて設定する
	/// 必ず最後に呼ぶ
	/// </summary>
    /// <seealso cref="SendAndSpeakAsync(string, bool)"/>
	/// <returns></returns>
	public async ValueTask SendAsync(){
		foreach (var v in sendParams.ToList())
		{
			switch(v.Key){
				case nameof(FluentCeVIO.SetAlphaAsync):
					{
						await _fcw.SetAlphaAsync(v.Value);
						break;
					}

				case nameof(FluentCeVIO.SetCastAsync):
					{
						await _fcw.SetCastAsync(v.Value);
						break;
					}

				case nameof(FluentCeVIO.SetComponentsAsync):
					{
						await _fcw.SetComponentsAsync(v.Value);
						break;
					}

				case nameof(FluentCeVIO.SetSpeedAsync):
					{
						await _fcw.SetSpeedAsync(v.Value);
						break;
					}

				case nameof(FluentCeVIO.SetToneAsync):
					{
						await _fcw.SetToneAsync(v.Value);
						break;
					}

				case nameof(FluentCeVIO.SetToneScaleAsync):
					{
						await _fcw.SetToneScaleAsync(v.Value);
						break;
					}

				case nameof(FluentCeVIO.SetVolumeAsync):
					{
						await _fcw.SetVolumeAsync(v.Value);
						break;
					}

				case nameof(FluentCeVIOParam.Emotions):
					{
						await SetComponentsByNameAsync();
						break;
					}

				default:
					break;
			}
		}

		//clean up
		sendParams.Clear();
		sendEmotions.Clear();
	}

	/// <summary>
    /// メソッドチェーンで指定したパラメータをまとめて設定してすぐに発声する
    /// 必ず最後に呼ぶ
    /// </summary>
    /// <inheritdoc cref="FluentCeVIO.SpeakAsync(string, bool)"/>
    /// <seealso cref="SendAsync"/>
	public async ValueTask SendAndSpeakAsync(
		string text,
		bool isWait = true)
	{
		await SendAsync();
		await _fcw.SpeakAsync(text, isWait);
	}

	private FluentCeVIOParam SetParam<T>(string callName, T value)
		where T : notnull
	{
		sendParams[callName] = value;
		return this;
	}

	private async ValueTask SetComponentsByNameAsync(){
		var comps = await _fcw.GetComponentsAsync();
		var sendComps = comps
			.Select(v =>
			{
				if (sendEmotions.ContainsKey(v.Name))
				{
					v.Value = sendEmotions[v.Name];
				}

				return v;
			})
			.ToList()
			.AsReadOnly();
		sendComps.ToList().ForEach(v => System.Console.WriteLine($"{v.Name}::{v.Value}"));
		await _fcw.SetComponentsAsync(sendComps);
	}
}