using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// 内部の通信で使われるメッセージクラス
/// </summary>
[Serializable]
public class CeVIOTalkMessage
{
	/// <summary>
	/// メッセージの識別子ID
	/// </summary>
	public Guid Id { get; set; } = Guid.NewGuid();

	/// <summary>
	/// メッセージの対象CeVIO製品
	/// </summary>
	public Product Product { get; set; } = Product.CeVIO_AI;

	/// <summary>
	/// A command message text for server
	/// </summary>
	/// <seealso cref="Common.ServerCommand"/>
	public string? ServerCommand { get; set; }

	/// <inheritdoc cref="Host"/>
	public Host? ServerHost { get; set; }

	/// <summary>
	/// やり取りする関数の名前
	/// </summary>
	public string? ServerCallName { get; set; }
	//public Type? ServerCallType { get; set; }

	/// <summary>
	/// やり取りする値
	/// </summary>
	public dynamic? ServerCallValue { get; set; }

	/// <summary>
	/// やり取りする場合の関数の引数
	/// </summary>
	public ReadOnlyCollection<dynamic>? ServerCallArgValues { get; set; }

	/// <inheritdoc />
	public override string ToString(){
		return $"msg id:{Id}, product:{Product}";
	}
}
