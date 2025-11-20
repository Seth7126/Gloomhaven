using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Leaderboards;

public sealed class LeaderboardRequest : IMessage<LeaderboardRequest>, IMessage, IEquatable<LeaderboardRequest>, IDeepCloneable<LeaderboardRequest>, IBufferMessage
{
	private static readonly MessageParser<LeaderboardRequest> _parser = new MessageParser<LeaderboardRequest>(() => new LeaderboardRequest());

	private UnknownFieldSet _unknownFields;

	public const int LeaderboardIdFieldNumber = 2;

	private string leaderboardId_ = "";

	public const int StartPositionFieldNumber = 3;

	private int startPosition_;

	public const int ResultsCountFieldNumber = 4;

	private int resultsCount_;

	[DebuggerNonUserCode]
	public static MessageParser<LeaderboardRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => LeaderboardsContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string LeaderboardId
	{
		get
		{
			return leaderboardId_;
		}
		set
		{
			leaderboardId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int StartPosition
	{
		get
		{
			return startPosition_;
		}
		set
		{
			startPosition_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int ResultsCount
	{
		get
		{
			return resultsCount_;
		}
		set
		{
			resultsCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	public LeaderboardRequest()
	{
	}

	[DebuggerNonUserCode]
	public LeaderboardRequest(LeaderboardRequest other)
		: this()
	{
		leaderboardId_ = other.leaderboardId_;
		startPosition_ = other.startPosition_;
		resultsCount_ = other.resultsCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public LeaderboardRequest Clone()
	{
		return new LeaderboardRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as LeaderboardRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(LeaderboardRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LeaderboardId != other.LeaderboardId)
		{
			return false;
		}
		if (StartPosition != other.StartPosition)
		{
			return false;
		}
		if (ResultsCount != other.ResultsCount)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (LeaderboardId.Length != 0)
		{
			num ^= LeaderboardId.GetHashCode();
		}
		if (StartPosition != 0)
		{
			num ^= StartPosition.GetHashCode();
		}
		if (ResultsCount != 0)
		{
			num ^= ResultsCount.GetHashCode();
		}
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
		if (LeaderboardId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(LeaderboardId);
		}
		if (StartPosition != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(StartPosition);
		}
		if (ResultsCount != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(ResultsCount);
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
		if (LeaderboardId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LeaderboardId);
		}
		if (StartPosition != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(StartPosition);
		}
		if (ResultsCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResultsCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(LeaderboardRequest other)
	{
		if (other != null)
		{
			if (other.LeaderboardId.Length != 0)
			{
				LeaderboardId = other.LeaderboardId;
			}
			if (other.StartPosition != 0)
			{
				StartPosition = other.StartPosition;
			}
			if (other.ResultsCount != 0)
			{
				ResultsCount = other.ResultsCount;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				LeaderboardId = input.ReadString();
				break;
			case 24u:
				StartPosition = input.ReadInt32();
				break;
			case 32u:
				ResultsCount = input.ReadInt32();
				break;
			}
		}
	}
}
