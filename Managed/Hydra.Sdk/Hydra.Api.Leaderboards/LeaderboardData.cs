using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Leaderboards;

public sealed class LeaderboardData : IMessage<LeaderboardData>, IMessage, IEquatable<LeaderboardData>, IDeepCloneable<LeaderboardData>, IBufferMessage
{
	private static readonly MessageParser<LeaderboardData> _parser = new MessageParser<LeaderboardData>(() => new LeaderboardData());

	private UnknownFieldSet _unknownFields;

	public const int LeaderboardIdFieldNumber = 1;

	private string leaderboardId_ = "";

	public const int EntriesFieldNumber = 2;

	private static readonly FieldCodec<LeaderboardEntry> _repeated_entries_codec = FieldCodec.ForMessage(18u, LeaderboardEntry.Parser);

	private readonly RepeatedField<LeaderboardEntry> entries_ = new RepeatedField<LeaderboardEntry>();

	public const int SelfDataFieldNumber = 3;

	private LeaderboardEntry selfData_;

	public const int TotalCountFieldNumber = 4;

	private int totalCount_;

	[DebuggerNonUserCode]
	public static MessageParser<LeaderboardData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => LeaderboardsContractsReflection.Descriptor.MessageTypes[1];

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
	public RepeatedField<LeaderboardEntry> Entries => entries_;

	[DebuggerNonUserCode]
	public LeaderboardEntry SelfData
	{
		get
		{
			return selfData_;
		}
		set
		{
			selfData_ = value;
		}
	}

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
	public LeaderboardData()
	{
	}

	[DebuggerNonUserCode]
	public LeaderboardData(LeaderboardData other)
		: this()
	{
		leaderboardId_ = other.leaderboardId_;
		entries_ = other.entries_.Clone();
		selfData_ = ((other.selfData_ != null) ? other.selfData_.Clone() : null);
		totalCount_ = other.totalCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public LeaderboardData Clone()
	{
		return new LeaderboardData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as LeaderboardData);
	}

	[DebuggerNonUserCode]
	public bool Equals(LeaderboardData other)
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
		if (!entries_.Equals(other.entries_))
		{
			return false;
		}
		if (!object.Equals(SelfData, other.SelfData))
		{
			return false;
		}
		if (TotalCount != other.TotalCount)
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
		num ^= entries_.GetHashCode();
		if (selfData_ != null)
		{
			num ^= SelfData.GetHashCode();
		}
		if (TotalCount != 0)
		{
			num ^= TotalCount.GetHashCode();
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
			output.WriteRawTag(10);
			output.WriteString(LeaderboardId);
		}
		entries_.WriteTo(ref output, _repeated_entries_codec);
		if (selfData_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(SelfData);
		}
		if (TotalCount != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(TotalCount);
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
		num += entries_.CalculateSize(_repeated_entries_codec);
		if (selfData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(SelfData);
		}
		if (TotalCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(LeaderboardData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.LeaderboardId.Length != 0)
		{
			LeaderboardId = other.LeaderboardId;
		}
		entries_.Add(other.entries_);
		if (other.selfData_ != null)
		{
			if (selfData_ == null)
			{
				SelfData = new LeaderboardEntry();
			}
			SelfData.MergeFrom(other.SelfData);
		}
		if (other.TotalCount != 0)
		{
			TotalCount = other.TotalCount;
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
			case 10u:
				LeaderboardId = input.ReadString();
				break;
			case 18u:
				entries_.AddEntriesFrom(ref input, _repeated_entries_codec);
				break;
			case 26u:
				if (selfData_ == null)
				{
					SelfData = new LeaderboardEntry();
				}
				input.ReadMessage(SelfData);
				break;
			case 32u:
				TotalCount = input.ReadInt32();
				break;
			}
		}
	}
}
