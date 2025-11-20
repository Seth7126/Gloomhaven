using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User.Zen;

public sealed class PlaytimeEvent : IMessage<PlaytimeEvent>, IMessage, IEquatable<PlaytimeEvent>, IDeepCloneable<PlaytimeEvent>, IBufferMessage
{
	private static readonly MessageParser<PlaytimeEvent> _parser = new MessageParser<PlaytimeEvent>(() => new PlaytimeEvent());

	private UnknownFieldSet _unknownFields;

	public const int PitIdFieldNumber = 1;

	private string pitId_ = "";

	public const int ActiveSecondsFieldNumber = 2;

	private int activeSeconds_;

	[DebuggerNonUserCode]
	public static MessageParser<PlaytimeEvent> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenEconomyApiContractsReflection.Descriptor.MessageTypes[7];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string PitId
	{
		get
		{
			return pitId_;
		}
		set
		{
			pitId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int ActiveSeconds
	{
		get
		{
			return activeSeconds_;
		}
		set
		{
			activeSeconds_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PlaytimeEvent()
	{
	}

	[DebuggerNonUserCode]
	public PlaytimeEvent(PlaytimeEvent other)
		: this()
	{
		pitId_ = other.pitId_;
		activeSeconds_ = other.activeSeconds_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PlaytimeEvent Clone()
	{
		return new PlaytimeEvent(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PlaytimeEvent);
	}

	[DebuggerNonUserCode]
	public bool Equals(PlaytimeEvent other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PitId != other.PitId)
		{
			return false;
		}
		if (ActiveSeconds != other.ActiveSeconds)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PitId.Length != 0)
		{
			num ^= PitId.GetHashCode();
		}
		if (ActiveSeconds != 0)
		{
			num ^= ActiveSeconds.GetHashCode();
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
		if (PitId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(PitId);
		}
		if (ActiveSeconds != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(ActiveSeconds);
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
		if (PitId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PitId);
		}
		if (ActiveSeconds != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ActiveSeconds);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PlaytimeEvent other)
	{
		if (other != null)
		{
			if (other.PitId.Length != 0)
			{
				PitId = other.PitId;
			}
			if (other.ActiveSeconds != 0)
			{
				ActiveSeconds = other.ActiveSeconds;
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
			case 10u:
				PitId = input.ReadString();
				break;
			case 16u:
				ActiveSeconds = input.ReadInt32();
				break;
			}
		}
	}
}
