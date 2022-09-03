namespace FluentCeVIOWrapper.Common.Talk.Environment;

/// <summary>
/// CeVIO Creative Studio (64bit) environment
/// </summary>
[System.ComponentModel.Description("CeVIO Creative Studio (64bit) environment records")]
public record CS : IEnvironment
{
	public Product Product => Product.CeVIO_CS;

	public string DllName => "CeVIO.Talk.RemoteService.dll";

	internal string ProgDir => System.Environment.ExpandEnvironmentVariables("%ProgramW6432%");

	public string DllPath => $"{ProgDir}/CeVIO/CeVIO Creative Studio (64bit)/";

	public string Service => "CeVIO.Talk.RemoteService.ServiceControl";

	public string Talker => "CeVIO.Talk.RemoteService.Talker";

	public string Agent => "CeVIO.Talk.RemoteService.TalkerAgent";
}