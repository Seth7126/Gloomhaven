using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Leaderboards;

public sealed class LeaderboardEntry : IMessage<LeaderboardEntry>, IMessage, IEquatable<LeaderboardEntry>, IDeepCloneable<LeaderboardEntry>, IBufferMessage
{
	private static readonly MessageParser<LeaderboardEntry> _parser = new MessageParser<LeaderboardEntry>(() => new LeaderboardEntry());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int RatingFieldNumber = 2;

	private double rating_;

	public const int PositionFieldNumber = 3;

	private int position_;

	public const int LastUpdatedFieldNumber = 4;

	private Timestamp lastUpdated_;

	public const int CustomDataFieldNumber = 5;

	private string customData_ = "";

	public const int CountFieldNumber = 6;

	private int count_;

	[DebuggerNonUserCode]
	public static MessageParser<LeaderboardEntry> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => LeaderboardsContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string UserId
	{
		get
		{
			return userId_;
		}
		set
		{
			userId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public double Rating
	{
		get
		{
			return rating_;
		}
		set
		{
			rating_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Position
	{
		get
		{
			return position_;
		}
		set
		{
			position_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Timestamp LastUpdated
	{
		get
		{
			return lastUpdated_;
		}
		set
		{
			lastUpdated_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string CustomData
	{
		get
		{
			return customData_;
		}
		set
		{
			customData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int Count
	{
		get
		{
			return count_;
		}
		set
		{
			count_ = value;
		}
	}

	[DebuggerNonUserCode]
	public LeaderboardEntry()
	{
	}

	[DebuggerNonUserCode]
	public LeaderboardEntry(LeaderboardEntry other)
		: this()
	{
		userId_ = other.userId_;
		rating_ = other.rating_;
		position_ = other.position_;
		lastUpdated_ = ((other.lastUpdated_ != null) ? other.lastUpdated_.Clone() : null);
		customData_ = other.customData_;
		count_ = other.count_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public LeaderboardEntry Clone()
	{
		return new LeaderboardEntry(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as LeaderboardEntry);
	}

	[DebuggerNonUserCode]
	public bool Equals(LeaderboardEntry other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UserId != other.UserId)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Rating, other.Rating))
		{
			return false;
		}
		if (Position != other.Position)
		{
			return false;
		}
		if (!object.Equals(LastUpdated, other.LastUpdated))
		{
			return false;
		}
		if (CustomData != other.CustomData)
		{
			return false;
		}
		if (Count != other.Count)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UserId.Length != 0)
		{
			num ^= UserId.GetHashCode();
		}
		if (Rating != 0.0)
		{
			num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Rating);
		}
		if (Position != 0)
		{
			num ^= Position.GetHashCode();
		}
		if (lastUpdated_ != null)
		{
			num ^= LastUpdated.GetHashCode();
		}
		if (CustomData.Length != 0)
		{
			num ^= CustomData.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
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
		if (UserId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(UserId);
		}
		if (Rating != 0.0)
		{
			output.WriteRawTag(17);
			output.WriteDouble(Rating);
		}
		if (Position != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(Position);
		}
		if (lastUpdated_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(LastUpdated);
		}
		if (CustomData.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(CustomData);
		}
		if (Count != 0)
		{
			output.WriteRawTag(48);
			output.WriteInt32(Count);
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
		if (UserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserId);
		}
		if (Rating != 0.0)
		{
			num += 9;
		}
		if (Position != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Position);
		}
		if (lastUpdated_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(LastUpdated);
		}
		if (CustomData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CustomData);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(LeaderboardEntry other)
	{
		if (other == null)
		{
			return;
		}
		if (other.UserId.Length != 0)
		{
			UserId = other.UserId;
		}
		if (other.Rating != 0.0)
		{
			Rating = other.Rating;
		}
		if (other.Position != 0)
		{
			Position = other.Position;
		}
		if (other.lastUpdated_ != null)
		{
			if (lastUpdated_ == null)
			{
				LastUpdated = new Timestamp();
			}
			LastUpdated.MergeFrom(other.LastUpdated);
		}
		if (other.CustomData.Length != 0)
		{
			CustomData = other.CustomData;
		}
		if (other.Count != 0)
		{
			Count = other.Count;
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
				UserId = input.ReadString();
				break;
			case 17u:
				Rating = input.ReadDouble();
				break;
			case 24u:
				Position = input.ReadInt32();
				break;
			case 34u:
				if (lastUpdated_ == null)
				{
					LastUpdated = new Timestamp();
				}
				input.ReadMessage(LastUpdated);
				break;
			case 42u:
				CustomData = input.ReadString();
				break;
			case 48u:
				Count = input.ReadInt32();
				break;
			}
		}
	}
}
