using System;
using System.Collections.Generic;
using SharedLibrary.Logger;

namespace SharedLibrary.Validation;

public class YMLValidationRecord
{
	public class CYMLValidationFailure : IEquatable<CYMLValidationFailure>
	{
		public string Message;

		public string FailureLocationUID;

		public int MinimumRequired;

		public EYMLValidationError FailureType;

		public bool Equals(CYMLValidationFailure other)
		{
			return FailureLocationUID == other.FailureLocationUID;
		}

		public override int GetHashCode()
		{
			return FailureLocationUID.GetHashCode();
		}
	}

	public List<CYMLValidationFailure> RecordedFailures;

	public bool HideLogErrorMessage;

	public YMLValidationRecord()
	{
		RecordedFailures = new List<CYMLValidationFailure>();
	}

	public void ResetRecord()
	{
		RecordedFailures.Clear();
	}

	public void RecordFailure(CYMLValidationFailure recordedFailure)
	{
		if (!RecordedFailures.Contains(recordedFailure))
		{
			RecordedFailures.Add(recordedFailure);
		}
	}

	public void RecordParseFailure(string failureLocation, string message)
	{
		CYMLValidationFailure recordedFailure = new CYMLValidationFailure
		{
			Message = message,
			FailureLocationUID = failureLocation,
			FailureType = EYMLValidationError.ParseFailure
		};
		DLLDebug.LogError(message, !HideLogErrorMessage);
		RecordFailure(recordedFailure);
	}

	public void RecordMinimumNotMetFailure(string failureLocation, string message, int minimumRequired)
	{
		CYMLValidationFailure recordedFailure = new CYMLValidationFailure
		{
			Message = message,
			FailureLocationUID = failureLocation,
			MinimumRequired = minimumRequired,
			FailureType = EYMLValidationError.MinimumNotMetFailure
		};
		DLLDebug.LogError(message);
		RecordFailure(recordedFailure);
	}

	public void RecordCombinationFailure(string failureLocation, string message)
	{
		CYMLValidationFailure recordedFailure = new CYMLValidationFailure
		{
			Message = message,
			FailureLocationUID = failureLocation,
			FailureType = EYMLValidationError.CombinationFailure
		};
		DLLDebug.LogError(message);
		RecordFailure(recordedFailure);
	}
}
