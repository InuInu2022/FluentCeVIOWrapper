using System.Collections.Generic;

namespace FluentCeVIOWrapper.Common.Talk;

internal class TalkerComponentCollection : IEnumerable<TalkerComponent>, ITalkerComponentArray
{
	public TalkerComponentCollection()
	{
	}

	/// <inheritdoc/>
	public ITalkerComponent this[int index] => this[index];


	/// <inheritdoc/>
	public ITalkerComponent this[string name] => this[name];

	/// <inheritdoc/>
	public int Length { get; }

	/// <inheritdoc/>
	public int Count { get; }

	/// <inheritdoc/>
	public ITalkerComponent At(int index)
	{
		return this[index];
	}

	/// <inheritdoc/>
	public ITalkerComponent ByName(string name)
	{
		return this[name];
	}

	/// <inheritdoc/>
	public ITalkerComponentArray Duplicate()
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc/>
	public IEnumerator<TalkerComponent> GetEnumerator()
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc/>
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		throw new System.NotImplementedException();
	}
}
