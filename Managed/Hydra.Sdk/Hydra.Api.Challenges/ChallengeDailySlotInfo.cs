using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ChallengeDailySlotInfo : IMessage<ChallengeDailySlotInfo>, IMessage, IEquatable<ChallengeDailySlotInfo>, IDeepCloneable<ChallengeDailySlotInfo>, IBufferMessage
{
	private static readonly MessageParser<ChallengeDailySlotInfo> _parser = new MessageParser<ChallengeDailySlotInfo>(() => new ChallengeDailySlotInfo());

	private UnknownFieldSet _unknownFields;

	public const int AvailableRechargeCountFieldNumber = 1;

	private int availableRechargeCount_;

	public const int RemainingCooldownSecFieldNumber = 2;

	private int remainingCooldownSec_;

	[DebuggerNonUserCode]
	public static MessageParser<ChallengeDailySlotInfo> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[10];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int AvailableRechargeCount
	{
		get
		{
			return availableRechargeCount_;
		}
		set
		{
			availableRechargeCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int RemainingCooldownSec
	{
		get
		{
			return remainingCooldownSec_;
		}
		set
		{
			remainingCooldownSec_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChallengeDailySlotInfo()
	{
	}

	[DebuggerNonUserCode]
	public ChallengeDailySlotInfo(ChallengeDailySlotInfo other)
		: this()
	{
		availableRechargeCount_ = other.availableRechargeCount_;
		remainingCooldownSec_ = other.remainingCooldownSec_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChallengeDailySlotInfo Clone()
	{
		return new ChallengeDailySlotInfo(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChallengeDailySlotInfo);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChallengeDailySlotInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (AvailableRechargeCount != other.AvailableRechargeCount)
		{
			return false;
		}
		if (RemainingCooldownSec != other.RemainingCooldownSec)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (AvailableRechargeCount != 0)
		{
			num ^= AvailableRechargeCount.GetHashCode();
		}
		if (RemainingCooldownSec != 0)
		{
			num ^= RemainingCooldownSec.GetHashCode();
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
		if (AvailableRechargeCount != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(AvailableRechargeCount);
		}
		if (RemainingCooldownSec != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(RemainingCooldownSec);
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
		if (AvailableRechargeCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AvailableRechargeCount);
		}
		if (RemainingCooldownSec != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RemainingCooldownSec);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChallengeDailySlotInfo other)
	{
		if (other != null)
		{
			if (other.AvailableRechargeCount != 0)
			{
				AvailableRechargeCount = other.AvailableRechargeCount;
			}
			if (other.RemainingCooldownSec != 0)
			{
				RemainingCooldownSec = other.RemainingCooldownSec;
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
				AvailableRechargeCount = input.ReadInt32();
				break;
			case 16u:
				RemainingCooldownSec = input.ReadInt32();
				break;
			}
		}
	}
}
