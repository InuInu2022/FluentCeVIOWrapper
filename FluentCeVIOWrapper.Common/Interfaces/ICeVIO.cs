#nullable disable

namespace FluentCeVIOWrapper.Common.Talk;

/// <summary>
/// メソッド・プロパティ名共有のためのInterface
/// Interface for <c>Talker</c>
/// </summary>
public interface ITalker
{
	/// <summary>
	/// 音の大きさ（0～100）を取得または設定します。
	/// </summary>
	uint Volume { get; set; }
	/// <summary>
	/// 話す速さ（0～100）を取得または設定します。
	/// </summary>
	uint Speed { get; set; }
	/// <summary>
	/// 音の高さ（0～100）を取得または設定します。
	/// </summary>
	uint Tone { get; set; }
	/// <summary>
	/// 声質（0～100）を取得または設定します。
	/// </summary>
	uint Alpha { get; set; }

	/// <inheritdoc/>
	uint ToneScale { get; set; }

	/// <inheritdoc/>
	System.Collections.ObjectModel.ReadOnlyCollection<TalkerComponent> Components { get; }

	/// <inheritdoc/>
	string Cast { get; set; }

	/// <inheritdoc/>
	string[] AvailableCasts { get; }

	/// <inheritdoc/>
	ISpeakingState Speak(string text);

	/// <inheritdoc/>

	bool Stop();

	/// <inheritdoc/>

	double GetTextDuration(string text);

	/// <inheritdoc/>

	IPhonemeData[] GetPhonemes(string text);

	/// <inheritdoc/>

	bool OutputWaveToFile(string text, string path);
}

/// <summary>
/// メソッド・プロパティ名共有のためのInterface
/// for ServiceControl
/// </summary>
public interface IServiceControl
{
	/// <summary>
	///【CeVIO AI】のバージョンを取得します。
	/// </summary>
	string HostVersion { get; }

	/// <summary>
	/// 【CeVIO AI】にアクセス可能かどうか取得します。
	/// </summary>
	bool IsHostStarted { get; }

	/// <summary>
	/// 【CeVIO AI】を起動
	/// </summary>
	/// <param name="noWait"></param>
	/// <returns></returns>
	HostStartResult StartHost(bool noWait);
	// 【CeVIO AI】を起動します。起動済みなら何もしません。
	// 引数：
	// 　noWait - trueは起動のみ行います。アクセス可能かどうかはIsHostStartedで確認します。
	// 　　　　　　falseは起動後に外部からアクセス可能になるまで制御を戻しません。
	// 戻り値：
	// 　結果コード。

	/// <summary>
	/// 終了を要求
	/// </summary>
	/// <param name="mode"></param>
	void CloseHost(HostCloseMode mode = HostCloseMode.Default);
	// 【CeVIO AI】に終了を要求します。
	// 引数：
	// 　mode - 処理モード。
}

/// <summary>
/// [<see cref="FluentCeVIOWrapper"/>] 利用しません。
/// </summary>
public interface ISpeakingState
{
	///<inheritdoc/>
	bool IsCompleted { get; }
	// 再生が完了したかどうかを取得します。
	// 完了した場合はtrue。（失敗を含む）それ以外の場合はfalse。

	///<inheritdoc/>
	bool IsSucceeded { get; }
	// 再生が成功したかどうかを取得します。
	// 成功した場合はtrue。それ以外の場合はfalse。

	///<inheritdoc/>
	void Wait();
	// 再生終了を待ちます。

	///<inheritdoc/>
	void Wait(double timeout);
	// 再生終了を待ちます。
	// 引数：
	// 　timeout - 最大待機時間。単位は秒。（0未満は無制限）
}

/// <summary>
/// 音素データの単位オブジェクト。
/// </summary>
public interface IPhonemeData
{
	/// <summary>
	/// 音素を取得します。
	/// </summary>
	string Phoneme { get; }

	/// <summary>
	/// 開始時間を取得します。単位は秒。
	/// </summary>
	double StartTime { get; }

	/// <summary>
	/// 終了時間を取得します。単位は秒。
	/// </summary>
	double EndTime { get; }
}

/// <summary>
/// [<see cref="FluentCeVIOWrapper"/>] 利用しません。
/// </summary>
public enum HostCloseMode
{
	///<inheritdoc/>
	Default = 0,
}
