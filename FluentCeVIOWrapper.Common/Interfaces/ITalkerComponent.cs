namespace FluentCeVIOWrapper.Common.Talk;

/// <summary>
/// 感情パラメータの単位オブジェクトインターフェイス。
/// </summary>
public interface ITalkerComponent
{
	/// <summary>
	/// キャストの識別子を取得します。
	/// </summary>
	string Id { get; }

	/// <summary>
	/// 感情の名前を取得します。
	/// </summary>
	string Name { get; }


	/// <summary>
	/// 感情の値（0～100）を取得または設定します。
	/// </summary>
	uint Value { get; set; }
}
