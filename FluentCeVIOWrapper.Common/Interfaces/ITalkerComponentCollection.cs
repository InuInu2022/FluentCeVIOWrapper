namespace FluentCeVIOWrapper.Common.Talk;

public interface ITalkerComponentCollection{
	int Count { get; }
	ITalkerComponent this[int index]{ get; }
	ITalkerComponent this[string name]{ get; }
}
