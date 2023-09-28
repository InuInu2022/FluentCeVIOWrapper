namespace FluentCeVIOWrapper.Common;

/// <summary>
/// セリフをしゃべる際の分割方法を指定します
/// </summary>
/// <remarks>
/// 文字数制限はバージョンと言語によって異なります。文字数制限を超えたセリフを喋らせた場合、そのままではエラーが発生し、はみ出した文字は発声されません。
/// <see cref="FluentCeVIO.SpeakAsync(string, bool, SpeakSegment, System.Threading.CancellationToken?)"/>の<c>segment</c>でこの指定をすることでエラーを発生させずに長文読み上げが可能になります。
/// <list type="bullet">
///     <listheader>
///        <term>CeVIO CS</term>
///        <description>150文字</description>
///     </listheader>
///     <item>
///        <term>CeVIO AI ver.8.1.19より前</term>
///        <description>150文字</description>
///     </item>
///     <item>
///        <term>CeVIO AI 日本語 ver.8.1.19以降</term>
///        <description>200文字</description>
///     </item>
///     <item>
///        <term>CeVIO AI 英語 ver.8.1.19以降</term>
///        <description>500文字</description>
///     </item>
/// </list>
/// </remarks>
/// <seealso cref="FluentCeVIO.SpeakAsync(string, bool, SpeakSegment, System.Threading.CancellationToken?)"/>
public enum SpeakSegment
{
	/// <summary>
	/// セリフ分割せず、文字数制限もチェックしません。
	/// 文字数制限を超えていた場合はエラーになります。
	/// 文字数が短いことがわかっている場合や、分割処理を自分で行う場合に指定してください。
	/// </summary>
	NoCheck,

	/// <summary>
	/// セリフを最初の句読点で分割し、なるべく短くなるようにします。
	/// </summary>
	Short,

	/// <summary>
	/// セリフをなるべく文字数制限に近い文字数で分割し、なるべく長くなるようにします。
	/// </summary>
	Long,
}