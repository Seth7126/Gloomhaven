using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Leaderboards;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class TournamentPublicData : IMessage<TournamentPublicData>, IMessage, IEquatable<TournamentPublicData>, IDeepCloneable<TournamentPublicData>, IBufferMessage
{
	private static readonly MessageParser<TournamentPublicData> _parser = new MessageParser<TournamentPublicData>(() => new TournamentPublicData());

	private UnknownFieldSet _unknownFields;

	public const int TotalCountFieldNumber = 1;

	private int totalCount_;

	public const int Top1FieldNumber = 2;

	private LeaderboardEntry top1_;

	[DebuggerNonUserCode]
	public static MessageParser<TournamentPublicData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[14];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int TotalCount
	{
		get
		{
			return totalCount_;
		}
		set
		{
			totalCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	public LeaderboardEntry Top1
	{
		get
		{
			return top1_;
		}
		set
		{
			top1_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentPublicData()
	{
	}

	[DebuggerNonUserCode]
	public TournamentPublicData(TournamentPublicData other)
		: this()
	{
		totalCount_ = other.totalCount_;
		top1_ = ((other.top1_ != null) ? other.top1_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TournamentPublicData Clone()
	{
		return new TournamentPublicData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TournamentPublicData);
	}

	[DebuggerNonUserCode]
	public bool Equals(TournamentPublicData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TotalCount != other.TotalCount)
		{
			return false;
		}
		if (!object.Equals(Top1, other.Top1))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TotalCount != 0)
		{
			num ^= TotalCount.GetHashCode();
		}
		if (top1_ != null)
		{
			num ^= Top1.GetHashCode();
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
		if (TotalCount != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(TotalCount);
		}
		if (top1_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Top1);
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
		if (TotalCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalCount);
		}
		if (top1_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Top1);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TournamentPublicData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.TotalCount != 0)
		{
			TotalCount = other.TotalCount;
		}
		if (other.top1_ != null)
		{
			if (top1_ == null)
			{
				Top1 = new LeaderboardEntry();
			}
			Top1.MergeFrom(other.Top1);
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
			case 8u:
				TotalCount = input.ReadInt32();
				break;
			case 18u:
				if (top1_ == null)
				{
					Top1 = new LeaderboardEntry();
				}
				input.ReadMessage(Top1);
				break;
			}
		}
	}
}
