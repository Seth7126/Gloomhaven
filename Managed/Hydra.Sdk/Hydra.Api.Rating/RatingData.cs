using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public sealed class RatingData : IMessage<RatingData>, IMessage, IEquatable<RatingData>, IDeepCloneable<RatingData>, IBufferMessage
{
	private static readonly MessageParser<RatingData> _parser = new MessageParser<RatingData>(() => new RatingData());

	private UnknownFieldSet _unknownFields;

	public const int RatingFieldNumber = 1;

	private double rating_;

	public const int GamesCountFieldNumber = 2;

	private int gamesCount_;

	public const int HistoryFieldNumber = 4;

	private static readonly FieldCodec<UserRatingHistory> _repeated_history_codec = FieldCodec.ForMessage(34u, UserRatingHistory.Parser);

	private readonly RepeatedField<UserRatingHistory> history_ = new RepeatedField<UserRatingHistory>();

	[DebuggerNonUserCode]
	public static MessageParser<RatingData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public RepeatedField<UserRatingHistory> History => history_;

	[DebuggerNonUserCode]
	public RatingData()
	{
	}

	[DebuggerNonUserCode]
	public RatingData(RatingData other)
		: this()
	{
		rating_ = other.rating_;
		gamesCount_ = other.gamesCount_;
		history_ = other.history_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RatingData Clone()
	{
		return new RatingData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RatingData);
	}

	[DebuggerNonUserCode]
	public bool Equals(RatingData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Rating, other.Rating))
		{
			return false;
		}
		if (GamesCount != other.GamesCount)
		{
			return false;
		}
		if (!history_.Equals(other.history_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Rating != 0.0)
		{
			num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Rating);
		}
		if (GamesCount != 0)
		{
			num ^= GamesCount.GetHashCode();
		}
		num ^= history_.GetHashCode();
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
		if (Rating != 0.0)
		{
			output.WriteRawTag(9);
			output.WriteDouble(Rating);
		}
		if (GamesCount != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(GamesCount);
		}
		history_.WriteTo(ref output, _repeated_history_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (Rating != 0.0)
		{
			num += 9;
		}
		if (GamesCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GamesCount);
		}
		num += history_.CalculateSize(_repeated_history_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RatingData other)
	{
		if (other != null)
		{
			if (other.Rating != 0.0)
			{
				Rating = other.Rating;
			}
			if (other.GamesCount != 0)
			{
				GamesCount = other.GamesCount;
			}
			history_.Add(other.history_);
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
			case 9u:
				Rating = input.ReadDouble();
				break;
			case 16u:
				GamesCount = input.ReadInt32();
				break;
			case 34u:
				history_.AddEntriesFrom(ref input, _repeated_history_codec);
				break;
			}
		}
	}
}
