using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentCeVIOWrapper.Common.Talk;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// CeVIOにまとめてメソッドチェーン(Buiderパターン)でパラメータを指定できるパラメータクラス
/// </summary>
public class FluentCeVIOParam
{
	private readonly FluentCeVIO _fcw;

	private readonly Dictionary<string, dynamic> sendParams
		= new(comparer: System.StringComparer.Ordinal);

	private Dictionary<string, uint> sendEmotions
		= new(comparer: System.StringComparer.Ordinal);

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

	/// <inheritdoc cref="FluentCeVIO.SetAlphaAsync(uint,bool)"/>
	/// <seealso cref="FluentCeVIO.SetAlphaAsync(uint,bool)"/>
	public FluentCeVIOParam Alpha([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetAlphaAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetCastAsync(string,bool)"/>
	/// <seealso cref="FluentCeVIO.SetCastAsync(string,bool)"/>
	public FluentCeVIOParam Cast(string castName)
		=> SetParam(nameof(FluentCeVIO.SetCastAsync), castName);

	///<inheritdoc cref="FluentCeVIO.SetComponentsAsync(IEnumerable{TalkerComponent})"/>
	///<seealso cref="FluentCeVIO.SetComponentsAsync(IEnumerable{TalkerComponent})"/>
	///<seealso cref="FluentCeVIO.GetComponentsAsync"/>
	public FluentCeVIOParam Components(IEnumerable<TalkerComponent> value)
		=> SetParam(nameof(FluentCeVIO.SetComponentsAsync), value);

	/// <summary>
	/// <c>Components</c>の簡易版。
	/// </summary>
	/// <example>
	/// <code>
	/// .Emotions(new(){["怒り"]=15,["普通"]=50})
	/// </code>
	/// </example>
	/// <param name="list">感情名、値（0~100）のDictionaryを与えてください</param>
	/// <seealso cref="Components(IEnumerable{TalkerComponent})"/>
	public FluentCeVIOParam Emotions(IDictionary<string,uint> list){
		sendEmotions = new Dictionary<string, uint>(list, System.StringComparer.Ordinal);
		return SetParam(nameof(FluentCeVIOParam.Emotions), sendEmotions);
	}

	/// <inheritdoc cref="FluentCeVIO.SetSpeedAsync(uint,bool)"/>
	/// <seealso cref="FluentCeVIO.SetSpeedAsync(uint,bool)"/>
	public FluentCeVIOParam Speed([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetSpeedAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetToneAsync(uint,bool)"/>
	/// <seealso cref="FluentCeVIO.SetToneAsync(uint,bool)"/>
	public FluentCeVIOParam Tone([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetToneAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetToneScaleAsync(uint,bool)"/>
	/// <seealso cref="FluentCeVIO.SetToneScaleAsync(uint,bool)"/>
	public FluentCeVIOParam ToneScale([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetToneScaleAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetVolumeAsync(uint,bool)"/>
	/// <seealso cref="FluentCeVIO.SetVolumeAsync(uint,bool)"/>
	public FluentCeVIOParam Volume([Range(0,100)] uint volume)
		=> SetParam(nameof(FluentCeVIO.SetVolumeAsync), volume);

	/// <summary>
	/// メソッドチェーンで指定したパラメータをまとめて設定する
	/// 必ず最後に呼ぶ
	/// </summary>
	/// <seealso cref="Create(FluentCeVIO)"/>
	/// <seealso cref="SendAndSpeakAsync(string, bool, SpeakSegment, CancellationToken)"/>
	public async ValueTask SendAsync(){
		List<KeyValuePair<string, dynamic>> list = sendParams.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			KeyValuePair<string, dynamic> v = list[i];
			switch (v.Key){
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
	/// <inheritdoc cref="FluentCeVIO.SpeakAsync(string, bool, SpeakSegment, System.Threading.CancellationToken)"/>
	/// <seealso cref="SendAsync"/>
	public async ValueTask SendAndSpeakAsync(
		string text,
		bool isWait = true,
		SpeakSegment segment = SpeakSegment.NoCheck,
		CancellationToken token = default
	)
	{
		await SendAsync().ConfigureAwait(false);
		_ = await _fcw
			.SpeakAsync(text, isWait, segment, token)
			.ConfigureAwait(false);
	}

	private FluentCeVIOParam SetParam<T>(string callName, T value)
		where T : notnull
	{
		sendParams[callName] = value;
		return this;
	}

	private async ValueTask SetComponentsByNameAsync(){
		var comps = await _fcw
			.GetComponentsAsync()
			.ConfigureAwait(false);
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
		await _fcw
			.SetComponentsAsync(sendComps)
			.ConfigureAwait(false);
	}
}