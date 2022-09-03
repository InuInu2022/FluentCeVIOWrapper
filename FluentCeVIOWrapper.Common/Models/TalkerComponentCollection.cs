using System.Collections.Generic;

namespace FluentCeVIOWrapper.Common.Talk;

public class TalkerComponentCollection : IEnumerable<TalkerComponent>, ITalkerComponentArray
{
	public TalkerComponentCollection()
	{
	}

	public ITalkerComponent this[int index] => this[index];

	public ITalkerComponent this[string name] => this[name];

	public int Length { get; }

	public int Count { get; }

	public ITalkerComponent At(int index)
	{
		return this[index];
	}

	public ITalkerComponent ByName(string name)
	{
		return this[name];
	}

	public ITalkerComponentArray Duplicate()
	{
		throw new System.NotImplementedException();
	}

	public IEnumerator<TalkerComponent> GetEnumerator()
	{
		throw new System.NotImplementedException();
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		throw new System.NotImplementedException();
	}
}
