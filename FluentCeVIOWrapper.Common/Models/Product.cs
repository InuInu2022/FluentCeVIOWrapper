using System;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// CeVIO products
/// </summary>
[Serializable]
public enum Product
{
	/// <summary>
	/// CeVIO Creative Studio ver. 7.0 >=
	/// </summary>
	CeVIO_CS = 1,

	/// <summary>
	/// CeVIO AI ver.8.0 >=
	/// </summary>
	CeVIO_AI = 2
}
