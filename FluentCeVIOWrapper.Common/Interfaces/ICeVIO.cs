#nullable disable

namespace FluentCeVIOWrapper.Common.Talk;

public interface ITalker
{
    uint Volume { get; set; }

    uint Speed { get; set; }

    uint Tone { get; set; }

    uint Alpha { get; set; }

    uint ToneScale { get; set; }

    ITalkerComponentCollection Components { get; }

    string Cast { get; set; }

    string[] AvailableCasts { get; }

	ISpeakingState Speak(string text);

    bool Stop();

    double GetTextDuration(string text);

    IPhonemeData[] GetPhonemes(string text);

    bool OutputWaveToFile(string text, string path);
}

public interface IServiceControl
{
    string HostVersion { get; }
    // 【CeVIO AI】のバージョンを取得します。

    bool IsHostStarted { get; }
    // 【CeVIO AI】にアクセス可能かどうか取得します。

    HostStartResult StartHost(bool noWait);
    // 【CeVIO AI】を起動します。起動済みなら何もしません。
    // 引数：
    // 　noWait - trueは起動のみ行います。アクセス可能かどうかはIsHostStartedで確認します。
    // 　　　　　　falseは起動後に外部からアクセス可能になるまで制御を戻しません。
    // 戻り値：
    // 　結果コード。

    void CloseHost(HostCloseMode mode = HostCloseMode.Default);
    // 【CeVIO AI】に終了を要求します。
    // 引数：
    // 　mode - 処理モード。
}

public interface ISpeakingState
{
    bool IsCompleted { get; }
    // 再生が完了したかどうかを取得します。
    // 完了した場合はtrue。（失敗を含む）それ以外の場合はfalse。

    bool IsSucceeded { get; }
    // 再生が成功したかどうかを取得します。
    // 成功した場合はtrue。それ以外の場合はfalse。

    void Wait();
    // 再生終了を待ちます。

    void Wait(double timeout);
    // 再生終了を待ちます。
    // 引数：
    // 　timeout - 最大待機時間。単位は秒。（0未満は無制限）
}

public interface IPhonemeData
{
    string Phoneme { get; }
    // 音素を取得します。

    double StartTime { get; }
    // 開始時間を取得します。単位は秒。

    double EndTime { get; }
    // 終了時間を取得します。単位は秒。
}

public enum HostCloseMode
{
    Default = 0
    // 【CeVIO AI】が編集中の場合、保存や終了キャンセルが可能。
}
