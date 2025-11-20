using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class TournamentReward : IMessage<TournamentReward>, IMessage, IEquatable<TournamentReward>, IDeepCloneable<TournamentReward>, IBufferMessage
{
	private static readonly MessageParser<TournamentReward> _parser = new MessageParser<TournamentReward>(() => new TournamentReward());

	private UnknownFieldSet _unknownFields;

	public const int PercentsBoundFieldNumber = 1;

	private double percentsBound_;

	public const int OfferIdFieldNumber = 2;

	private string offerId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<TournamentReward> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public double PercentsBound
	{
		get
		{
			return percentsBound_;
		}
		set
		{
			percentsBound_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string OfferId
	{
		get
		{
			return offerId_;
		}
		set
		{
			offerId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TournamentReward()
	{
	}

	[DebuggerNonUserCode]
	public TournamentReward(TournamentReward other)
		: this()
	{
		percentsBound_ = other.percentsBound_;
		offerId_ = other.offerId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TournamentReward Clone()
	{
		return new TournamentReward(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TournamentReward);
	}

	[DebuggerNonUserCode]
	public bool Equals(TournamentReward other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(PercentsBound, other.PercentsBound))
		{
			return false;
		}
		if (OfferId != other.OfferId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PercentsBound != 0.0)
		{
			num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(PercentsBound);
		}
		if (OfferId.Length != 0)
		{
			num ^= OfferId.GetHashCode();
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
		if (PercentsBound != 0.0)
		{
			output.WriteRawTag(9);
			output.WriteDouble(PercentsBound);
		}
		if (OfferId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(OfferId);
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
		if (PercentsBound != 0.0)
		{
			num += 9;
		}
		if (OfferId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(OfferId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TournamentReward other)
	{
		if (other != null)
		{
			if (other.PercentsBound != 0.0)
			{
				PercentsBound = other.PercentsBound;
			}
			if (other.OfferId.Length != 0)
			{
				OfferId = other.OfferId;
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
			case 9u:
				PercentsBound = input.ReadDouble();
				break;
			case 18u:
				OfferId = input.ReadString();
				break;
			}
		}
	}
}
