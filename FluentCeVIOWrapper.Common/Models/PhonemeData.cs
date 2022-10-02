using System;
using FluentCeVIOWrapper.Common.Talk;

namespace FluentCeVIOWrapper.Common.Models;

/// <inheritdoc/>
[Serializable()]
public class PhonemeData : IPhonemeData
{
	/// <inheritdoc/>
	/// <param name="startTime"></param>
	/// <param name="endTime"></param>
	/// <param name="phoneme"></param>
	public PhonemeData(
		double startTime,
		double endTime,
		string phoneme
	)
	{
		StartTime = startTime;
		EndTime = endTime;
		Phoneme = phoneme;
	}

	/// <inheritdoc/>
	public string Phoneme { get; }

	/// <inheritdoc/>
	public double StartTime { get; }

	/// <inheritdoc/>
	public double EndTime { get; }
}
