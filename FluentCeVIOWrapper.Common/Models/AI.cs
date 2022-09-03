namespace FluentCeVIOWrapper.Common.Talk.Environment;

/// <summary>
/// CeVIO AI environment
/// </summary>
[System.ComponentModel.Description("CeVIO AI environment records")]
public record AI : IEnvironment
{
	public Product Product => Product.CeVIO_AI;

	public string DllName => "CeVIO.Talk.RemoteService2.dll";

	internal string ProgDir => System.Environment.ExpandEnvironmentVariables("%ProgramW6432%");

	public string DllPath => $"{ProgDir}/CeVIO/CeVIO AI/";

	public string Service => "CeVIO.Talk.RemoteService2.ServiceControl2";

	public string Talker => "CeVIO.Talk.RemoteService2.Talker2";

	public string Agent => "CeVIO.Talk.RemoteService2.TalkerAgent2";
}
