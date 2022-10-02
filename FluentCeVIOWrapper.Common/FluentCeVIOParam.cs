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
	private readonly Dictionary<string, dynamic> sendParams = new();

	internal FluentCeVIOParam(FluentCeVIO fcw){
		_fcw = fcw;
	}

	/// <summary>
	/// メソッドチェーンでパラメータを指定できるパラメータクラスのファクトリメソッド
	/// 最後に <see cref="SendAsync"/>を呼ぶ
	/// </summary>
	/// <see cref="FluentCeVIO.CreateParam"/>
	/// <see cref="SendAsync"/>
	/// <param name="fcw"></param>
	public static FluentCeVIOParam Create(FluentCeVIO fcw)
		=> new(fcw);

	/// <inheritdoc cref="FluentCeVIO.SetAlphaAsync(uint)"/>
	public FluentCeVIOParam Alpha([Range(0,100)] uint value)
		=> SetParam(nameof(FluentCeVIO.SetAlphaAsync), value);

	/// <inheritdoc cref="FluentCeVIO.SetCastAsync(string)"/>
	public FluentCeVIOParam Cast(string castName)
		=> SetParam(nameof(FluentCeVIO.SetCastAsync), castName);

	///<inheritdoc cref="FluentCeVIO.SetComponentsAsync(ReadOnlyCollection{TalkerComponent})"/>
	///<see cref="FluentCeVIO.GetComponentsAsync"/>
	public FluentCeVIOParam Components(ReadOnlyCollection<TalkerComponent> value)
		=> SetParam(nameof(FluentCeVIO.SetComponentsAsync), value);

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

				default:
					break;
			}
		}

		//clean up
		sendParams.Clear();
	}

	private FluentCeVIOParam SetParam<T>(string callName, T value)
		where T : notnull
	{
		sendParams[callName] = value;
		return this;
	}
}