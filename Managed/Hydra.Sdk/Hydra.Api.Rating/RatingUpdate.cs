using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public sealed class RatingUpdate : IMessage<RatingUpdate>, IMessage, IEquatable<RatingUpdate>, IDeepCloneable<RatingUpdate>, IBufferMessage
{
	public enum ResultOneofCase
	{
		None = 0,
		IndividualResults = 2,
		TeamResults = 3
	}

	private static readonly MessageParser<RatingUpdate> _parser = new MessageParser<RatingUpdate>(() => new RatingUpdate());

	private UnknownFieldSet _unknownFields;

	public const int IndividualResultsFieldNumber = 2;

	public const int TeamResultsFieldNumber = 3;

	private object result_;

	private ResultOneofCase resultCase_ = ResultOneofCase.None;

	[DebuggerNonUserCode]
	public static MessageParser<RatingUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[7];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public SessionIndividualResult IndividualResults
	{
		get
		{
			return (resultCase_ == ResultOneofCase.IndividualResults) ? ((SessionIndividualResult)result_) : null;
		}
		set
		{
			result_ = value;
			resultCase_ = ((value != null) ? ResultOneofCase.IndividualResults : ResultOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public SessionTeamResult TeamResults
	{
		get
		{
			return (resultCase_ == ResultOneofCase.TeamResults) ? ((SessionTeamResult)result_) : null;
		}
		set
		{
			result_ = value;
			resultCase_ = ((value != null) ? ResultOneofCase.TeamResults : ResultOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public ResultOneofCase ResultCase => resultCase_;

	[DebuggerNonUserCode]
	public RatingUpdate()
	{
	}

	[DebuggerNonUserCode]
	public RatingUpdate(RatingUpdate other)
		: this()
	{
		switch (other.ResultCase)
		{
		case ResultOneofCase.IndividualResults:
			IndividualResults = other.IndividualResults.Clone();
			break;
		case ResultOneofCase.TeamResults:
			TeamResults = other.TeamResults.Clone();
			break;
		}
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RatingUpdate Clone()
	{
		return new RatingUpdate(this);
	}

	[DebuggerNonUserCode]
	public void ClearResult()
	{
		resultCase_ = ResultOneofCase.None;
		result_ = null;
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RatingUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(RatingUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(IndividualResults, other.IndividualResults))
		{
			return false;
		}
		if (!object.Equals(TeamResults, other.TeamResults))
		{
			return false;
		}
		if (ResultCase != other.ResultCase)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (resultCase_ == ResultOneofCase.IndividualResults)
		{
			num ^= IndividualResults.GetHashCode();
		}
		if (resultCase_ == ResultOneofCase.TeamResults)
		{
			num ^= TeamResults.GetHashCode();
		}
		num ^= (int)resultCase_;
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (resultCase_ == ResultOneofCase.IndividualResults)
		{
			output.WriteRawTag(18);
			output.WriteMessage(IndividualResults);
		}
		if (resultCase_ == ResultOneofCase.TeamResults)
		{
			output.WriteRawTag(26);
			output.WriteMessage(TeamResults);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (resultCase_ == ResultOneofCase.IndividualResults)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(IndividualResults);
		}
		if (resultCase_ == ResultOneofCase.TeamResults)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(TeamResults);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RatingUpdate other)
	{
		if (other == null)
		{
			return;
		}
		switch (other.ResultCase)
		{
		case ResultOneofCase.IndividualResults:
			if (IndividualResults == null)
			{
				IndividualResults = new SessionIndividualResult();
			}
			IndividualResults.MergeFrom(other.IndividualResults);
			break;
		case ResultOneofCase.TeamResults:
			if (TeamResults == null)
			{
				TeamResults = new SessionTeamResult();
			}
			TeamResults.MergeFrom(other.TeamResults);
			break;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 18u:
			{
				SessionIndividualResult sessionIndividualResult = new SessionIndividualResult();
				if (resultCase_ == ResultOneofCase.IndividualResults)
				{
					sessionIndividualResult.MergeFrom(IndividualResults);
				}
				input.ReadMessage(sessionIndividualResult);
				IndividualResults = sessionIndividualResult;
				break;
			}
			case 26u:
			{
				SessionTeamResult sessionTeamResult = new SessionTeamResult();
				if (resultCase_ == ResultOneofCase.TeamResults)
				{
					sessionTeamResult.MergeFrom(TeamResults);
				}
				input.ReadMessage(sessionTeamResult);
				TeamResults = sessionTeamResult;
				break;
			}
			}
		}
	}
}
