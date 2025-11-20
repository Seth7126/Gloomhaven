using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Rating;

public sealed class UserRatingHistory : IMessage<UserRatingHistory>, IMessage, IEquatable<UserRatingHistory>, IDeepCloneable<UserRatingHistory>, IBufferMessage
{
	private static readonly MessageParser<UserRatingHistory> _parser = new MessageParser<UserRatingHistory>(() => new UserRatingHistory());

	private UnknownFieldSet _unknownFields;

	public const int GamesCountFieldNumber = 1;

	private int gamesCount_;

	public const int RatingFieldNumber = 2;

	private double rating_;

	public const int SessionIdFieldNumber = 3;

	private string sessionId_ = "";

	public const int TimeFieldNumber = 4;

	private Timestamp time_;

	public const int CustomDataFieldNumber = 5;

	private string customData_ = "";

	public const int ResultFieldNumber = 6;

	private MatchResult result_ = MatchResult.None;

	[DebuggerNonUserCode]
	public static MessageParser<UserRatingHistory> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int GamesCount
	{
		get
		{
			return gamesCount_;
		}
		set
		{
			gamesCount_ = value;
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
	public string SessionId
	{
		get
		{
			return sessionId_;
		}
		set
		{
			sessionId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public Timestamp Time
	{
		get
		{
			return time_;
		}
		set
		{
			time_ = value;
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
	public MatchResult Result
	{
		get
		{
			return result_;
		}
		set
		{
			result_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UserRatingHistory()
	{
	}

	[DebuggerNonUserCode]
	public UserRatingHistory(UserRatingHistory other)
		: this()
	{
		gamesCount_ = other.gamesCount_;
		rating_ = other.rating_;
		sessionId_ = other.sessionId_;
		time_ = ((other.time_ != null) ? other.time_.Clone() : null);
		customData_ = other.customData_;
		result_ = other.result_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserRatingHistory Clone()
	{
		return new UserRatingHistory(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserRatingHistory);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserRatingHistory other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GamesCount != other.GamesCount)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Rating, other.Rating))
		{
			return false;
		}
		if (SessionId != other.SessionId)
		{
			return false;
		}
		if (!object.Equals(Time, other.Time))
		{
			return false;
		}
		if (CustomData != other.CustomData)
		{
			return false;
		}
		if (Result != other.Result)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (GamesCount != 0)
		{
			num ^= GamesCount.GetHashCode();
		}
		if (Rating != 0.0)
		{
			num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Rating);
		}
		if (SessionId.Length != 0)
		{
			num ^= SessionId.GetHashCode();
		}
		if (time_ != null)
		{
			num ^= Time.GetHashCode();
		}
		if (CustomData.Length != 0)
		{
			num ^= CustomData.GetHashCode();
		}
		if (Result != MatchResult.None)
		{
			num ^= Result.GetHashCode();
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
		if (GamesCount != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(GamesCount);
		}
		if (Rating != 0.0)
		{
			output.WriteRawTag(17);
			output.WriteDouble(Rating);
		}
		if (SessionId.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(SessionId);
		}
		if (time_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Time);
		}
		if (CustomData.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(CustomData);
		}
		if (Result != MatchResult.None)
		{
			output.WriteRawTag(48);
			output.WriteEnum((int)Result);
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
		if (GamesCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GamesCount);
		}
		if (Rating != 0.0)
		{
			num += 9;
		}
		if (SessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SessionId);
		}
		if (time_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Time);
		}
		if (CustomData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CustomData);
		}
		if (Result != MatchResult.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Result);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserRatingHistory other)
	{
		if (other == null)
		{
			return;
		}
		if (other.GamesCount != 0)
		{
			GamesCount = other.GamesCount;
		}
		if (other.Rating != 0.0)
		{
			Rating = other.Rating;
		}
		if (other.SessionId.Length != 0)
		{
			SessionId = other.SessionId;
		}
		if (other.time_ != null)
		{
			if (time_ == null)
			{
				Time = new Timestamp();
			}
			Time.MergeFrom(other.Time);
		}
		if (other.CustomData.Length != 0)
		{
			CustomData = other.CustomData;
		}
		if (other.Result != MatchResult.None)
		{
			Result = other.Result;
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
				GamesCount = input.ReadInt32();
				break;
			case 17u:
				Rating = input.ReadDouble();
				break;
			case 26u:
				SessionId = input.ReadString();
				break;
			case 34u:
				if (time_ == null)
				{
					Time = new Timestamp();
				}
				input.ReadMessage(Time);
				break;
			case 42u:
				CustomData = input.ReadString();
				break;
			case 48u:
				Result = (MatchResult)input.ReadEnum();
				break;
			}
		}
	}
}
