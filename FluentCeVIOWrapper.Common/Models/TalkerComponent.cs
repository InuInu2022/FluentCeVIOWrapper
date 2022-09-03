using System;
using System.ComponentModel.DataAnnotations;

namespace FluentCeVIOWrapper.Common.Talk;

/// <summary>
/// 感情パラメータの単位オブジェクト。
/// </summary>
[Serializable]
public record TalkerComponent : ITalkerComponent
{
	/// <summary>
	/// 識別子を取得します。
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// 感情の名前を取得します。
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// 感情の値（0～100）を取得または設定します。
	/// </summary>
	[Range(0, 100)]
	public uint Value { get; set; }

	public TalkerComponent(
		string id,
		string name,
		[Range(0, 100)] uint value
	)
	{
		Id = id;
		Name = name;
		Value = value;
	}
}