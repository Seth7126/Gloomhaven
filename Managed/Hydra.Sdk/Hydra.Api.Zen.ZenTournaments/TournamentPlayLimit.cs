using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class TournamentPlayLimit : IMessage<TournamentPlayLimit>, IMessage, IEquatable<TournamentPlayLimit>, IDeepCloneable<TournamentPlayLimit>, IBufferMessage
{
	private static readonly MessageParser<TournamentPlayLimit> _parser = new MessageParser<TournamentPlayLimit>(() => new TournamentPlayLimit());

	private UnknownFieldSet _unknownFields;

	public const int LimitTypeFieldNumber = 1;

	private TournamentPlayLimitType limitType_ = TournamentPlayLimitType.None;

	public const int LimitValueFieldNumber = 2;

	private int limitValue_;

	[DebuggerNonUserCode]
	public static MessageParser<TournamentPlayLimit> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public TournamentPlayLimitType LimitType
	{
		get
		{
			return limitType_;
		}
		set
		{
			limitType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int LimitValue
	{
		get
		{
			return limitValue_;
		}
		set
		{
			limitValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentPlayLimit()
	{
	}

	[DebuggerNonUserCode]
	public TournamentPlayLimit(TournamentPlayLimit other)
		: this()
	{
		limitType_ = other.limitType_;
		limitValue_ = other.limitValue_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TournamentPlayLimit Clone()
	{
		return new TournamentPlayLimit(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TournamentPlayLimit);
	}

	[DebuggerNonUserCode]
	public bool Equals(TournamentPlayLimit other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LimitType != other.LimitType)
		{
			return false;
		}
		if (LimitValue != other.LimitValue)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (LimitType != TournamentPlayLimitType.None)
		{
			num ^= LimitType.GetHashCode();
		}
		if (LimitValue != 0)
		{
			num ^= LimitValue.GetHashCode();
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
		if (LimitType != TournamentPlayLimitType.None)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)LimitType);
		}
		if (LimitValue != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(LimitValue);
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
		if (LimitType != TournamentPlayLimitType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)LimitType);
		}
		if (LimitValue != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitValue);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TournamentPlayLimit other)
	{
		if (other != null)
		{
			if (other.LimitType != TournamentPlayLimitType.None)
			{
				LimitType = other.LimitType;
			}
			if (other.LimitValue != 0)
			{
				LimitValue = other.LimitValue;
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
			case 8u:
				LimitType = (TournamentPlayLimitType)input.ReadEnum();
				break;
			case 16u:
				LimitValue = input.ReadInt32();
				break;
			}
		}
	}
}
