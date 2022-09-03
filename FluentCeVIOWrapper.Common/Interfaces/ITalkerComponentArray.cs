namespace FluentCeVIOWrapper.Common.Talk;
public interface ITalkerComponentArray
{
	int Length { get; }

	ITalkerComponent At(int index);
	ITalkerComponent ByName(string name);
	ITalkerComponentArray Duplicate();
}