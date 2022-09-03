namespace FluentCeVIOWrapper.Common.Talk;

public interface ITalkerComponent
{
	string Id { get; }
	string Name { get; }
	uint Value { get; set; }
}
