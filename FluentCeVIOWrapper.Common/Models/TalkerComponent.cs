using System;
using System.ComponentModel.DataAnnotations;

namespace FluentCeVIOWrapper.Common.Talk;

/// <summary>
/// 感情パラメータの単位オブジェクト。
/// </summary>
[Serializable]
public record TalkerComponent : ITalkerComponent
{
	/// <inheritdoc/>
	public string Id { get; set; }

	/// <inheritdoc/>
	public string Name { get; set; }

	/// <inheritdoc/>
	[Range(0, 100)]
	public uint Value { get; set; }

	/// <summary>
	/// 感情パラメータの単位オブジェクトのコンストラクタ
	/// </summary>
	/// <param name="id">
	/// 	<inheritdoc cref="Id" path="/summary"/></param>
	/// <param name="name">
	/// 	<inheritdoc cref="Name" path="/summary"/>
	/// 	</param>
	/// <param name="value">
	/// 	<inheritdoc cref="Value" path="/summary"/>
	/// 	</param>
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