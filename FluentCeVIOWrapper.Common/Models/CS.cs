namespace FluentCeVIOWrapper.Common.Talk.Environment;

/// <summary>
/// CeVIO Creative Studio (64bit) environment
/// </summary>
[System.ComponentModel.Description("CeVIO Creative Studio (64bit) environment records")]
public class CS : IEnvironment
{
	/// <inheritdoc />
	public Product Product => Product.CeVIO_CS;

	/// <inheritdoc />
	public string DllName => "CeVIO.Talk.RemoteService.dll";

	/// <inheritdoc />
	internal static string ProgDir => System.Environment.ExpandEnvironmentVariables("%ProgramW6432%");

	/// <inheritdoc />
	public string DllPath { get; set; } = $"{ProgDir}/CeVIO/CeVIO Creative Studio (64bit)/";

	/// <inheritdoc />
	public string Service => "CeVIO.Talk.RemoteService.ServiceControl";

	/// <inheritdoc />
	public string Talker => "CeVIO.Talk.RemoteService.Talker";

	/// <inheritdoc />
	public string Agent => "CeVIO.Talk.RemoteService.TalkerAgent";
}