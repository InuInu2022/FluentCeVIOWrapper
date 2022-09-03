namespace FluentCeVIOWrapper.Common.Talk.Environment;

public interface IEnvironment{
	public Product Product { get; }
	public string DllName { get; }
	public string DllPath { get; }
	public string Service { get; }
	public string Talker { get; }
	public string Agent { get; }
}
