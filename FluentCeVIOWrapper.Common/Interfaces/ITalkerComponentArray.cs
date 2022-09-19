namespace FluentCeVIOWrapper.Common.Talk;
internal interface ITalkerComponentArray
{
	int Length { get; }

	ITalkerComponent At(int index);
	ITalkerComponent ByName(string name);
	ITalkerComponentArray Duplicate();
}