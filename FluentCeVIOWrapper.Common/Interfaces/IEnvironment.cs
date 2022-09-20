namespace FluentCeVIOWrapper.Common.Talk.Environment;

/// <summary>
/// Product env path default values
/// </summary>
public interface IEnvironment{
	/// <summary>
	/// CeVIOの種類
	/// </summary>
	public Product Product { get; }

	/// <summary>
	/// dllの名称
	/// </summary>
	public string DllName { get; }

	/// <summary>
	/// デフォルトのdllのpath
	/// </summary>
	public string DllPath { get; set; }

	/// <summary>
	/// ServiceControll class
	/// </summary>
	public string Service { get; }

	/// <summary>
	/// Talker class
	/// </summary>
	public string Talker { get; }

	/// <summary>
	/// TalkerAgent class
	/// </summary>
	public string Agent { get; }
}
