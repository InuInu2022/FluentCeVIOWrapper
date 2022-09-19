namespace FluentCeVIOWrapper.Common.Talk;

/// <inheritdoc/>
internal interface ITalkerComponentCollection
{
	/// <inheritdoc/>
	int Count { get; }

	/// <inheritdoc/>
	ITalkerComponent this[int index]{ get; }

	/// <inheritdoc/>
	ITalkerComponent this[string name]{ get; }
}
