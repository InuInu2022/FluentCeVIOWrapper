using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FluentCeVIOWrapper.Common;

[Serializable]
public class CeVIOTalkMessage
{
	public Guid Id { get; set; } = Guid.NewGuid();

	public Product Product { get; set; } = Product.CeVIO_AI;

	/// <summary>
	/// A command message text for server
	/// </summary>
	public string? ServerCommand { get; set; }

	public Host? ServerHost { get; set; }

	public string? ServerCallName { get; set; }
	//public Type? ServerCallType { get; set; }
	public dynamic? ServerCallValue { get; set; }

	//public ReadOnlyCollection<Type>? ServerCallArgTypes { get; set; }
	public ReadOnlyCollection<dynamic>? ServerCallArgValues { get; set; }


	public override string ToString(){
		return $"msg id:{Id.ToString()}, product:{Product.ToString()}";
	}
}
