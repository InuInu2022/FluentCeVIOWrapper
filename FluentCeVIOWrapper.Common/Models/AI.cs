namespace FluentCeVIOWrapper.Common.Talk.Environment;

/// <summary>
/// CeVIO AI environment
/// </summary>
[System.ComponentModel.Description("CeVIO AI environment records")]
public record AI : IEnvironment
{
	/// <inheritdoc />
	public Product Product => Product.CeVIO_AI;

	/// <inheritdoc />
	public string DllName => "CeVIO.Talk.RemoteService2.dll";

	/// <inheritdoc />
	internal string ProgDir => System.Environment.ExpandEnvironmentVariables("%ProgramW6432%");

	/// <inheritdoc />
	public string DllPath => $"{ProgDir}/CeVIO/CeVIO AI/";

	/// <inheritdoc />
	public string Service => "CeVIO.Talk.RemoteService2.ServiceControl2";

	/// <inheritdoc />
	public string Talker => "CeVIO.Talk.RemoteService2.Talker2";

	/// <inheritdoc />
	public string Agent => "CeVIO.Talk.RemoteService2.TalkerAgent2";
}
