using System;

namespace FluentCeVIOWrapper.Common;

/// <summary>
/// 通信で使われるコマンド定義
/// </summary>
[Serializable]
public static class ServerCommand
{
    /// <summary>
	/// メソッド呼び出しコマンド
	/// </summary>
	public const string CallMethod = "CALL_METHOD";

    /// <summary>
	/// プロパティ取得コマンド
	/// </summary>
    public const string GetProperty = "GET_PROPERTY";

    /// <summary>
    /// プロパティ設定コマンド
    /// </summary>
    public const string SetProperty = "SET_PROPERTY";

    /// <summary>
	/// debug use only
	/// </summary>
    public const string Echo = "ECHO";
}
