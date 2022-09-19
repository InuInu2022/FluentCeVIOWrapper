using System;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// ホストアプリ（CeVIO）の内部的なコンポーネント種別
/// </summary>
[Serializable]
public enum Host{
	/// <summary>
	/// Agent
	/// </summary>
	Agent = 0,

	/// <summary>
	/// Service
	/// </summary>
	Service = 1,

	/// <summary>
	/// Talker
	/// </summary>
	Talker = 2
}