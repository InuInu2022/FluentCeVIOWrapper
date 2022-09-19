namespace FluentCeVIOWrapper.Common.Talk;

/// <summary>
/// <see cref="FluentCeVIO.StartAsync()"/> の結果コードを表します。
/// </summary>
public enum HostStartResult
{
	/// <summary>
	/// アプリケーション起動後、エラーにより終了。
	/// </summary>
	HostError = -4,
	/// <summary>
	/// プロセスの起動に失敗。
	/// </summary>
	StartingFailed = -3,
	/// <summary>
	/// 実行ファイルが見つからない。
	/// </summary>
	FileNotFound = -2,
	/// <summary>
	/// インストール状態が不明。
	/// </summary>
	NotRegistered = -1,
	/// <summary>
	///  成功。起動済みの場合も含みます。
	/// </summary>
	Succeeded = 0
}
