using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Presence;

public sealed class PresenceUserUpdate : IMessage<PresenceUserUpdate>, IMessage, IEquatable<PresenceUserUpdate>, IDeepCloneable<PresenceUserUpdate>, IBufferMessage
{
	private static readonly MessageParser<PresenceUserUpdate> _parser = new MessageParser<PresenceUserUpdate>(() => new PresenceUserUpdate());

	private UnknownFieldSet _unknownFields;

	public const int InviteEventsFieldNumber = 1;

	private static readonly FieldCodec<InviteEvent> _repeated_inviteEvents_codec = FieldCodec.ForMessage(10u, InviteEvent.Parser);

	private readonly RepeatedField<InviteEvent> inviteEvents_ = new RepeatedField<InviteEvent>();

	public const int LongOperationResultFieldNumber = 2;

	private LongOperationResultUpdate longOperationResult_;

	[DebuggerNonUserCode]
	public static MessageParser<PresenceUserUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<InviteEvent> InviteEvents => inviteEvents_;

	[DebuggerNonUserCode]
	public LongOperationResultUpdate LongOperationResult
	{
		get
		{
			return longOperationResult_;
		}
		set
		{
			longOperationResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PresenceUserUpdate()
	{
	}

	[DebuggerNonUserCode]
	public PresenceUserUpdate(PresenceUserUpdate other)
		: this()
	{
		inviteEvents_ = other.inviteEvents_.Clone();
		longOperationResult_ = ((other.longOperationResult_ != null) ? other.longOperationResult_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PresenceUserUpdate Clone()
	{
		return new PresenceUserUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PresenceUserUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(PresenceUserUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!inviteEvents_.Equals(other.inviteEvents_))
		{
			return false;
		}
		if (!object.Equals(LongOperationResult, other.LongOperationResult))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= inviteEvents_.GetHashCode();
		if (longOperationResult_ != null)
		{
			num ^= LongOperationResult.GetHashCode();
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
		inviteEvents_.WriteTo(ref output, _repeated_inviteEvents_codec);
		if (longOperationResult_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(LongOperationResult);
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
		num += inviteEvents_.CalculateSize(_repeated_inviteEvents_codec);
		if (longOperationResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(LongOperationResult);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PresenceUserUpdate other)
	{
		if (other == null)
		{
			return;
		}
		inviteEvents_.Add(other.inviteEvents_);
		if (other.longOperationResult_ != null)
		{
			if (longOperationResult_ == null)
			{
				LongOperationResult = new LongOperationResultUpdate();
			}
			LongOperationResult.MergeFrom(other.LongOperationResult);
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
				inviteEvents_.AddEntriesFrom(ref input, _repeated_inviteEvents_codec);
				break;
			case 18u:
				if (longOperationResult_ == null)
				{
					LongOperationResult = new LongOperationResultUpdate();
				}
				input.ReadMessage(LongOperationResult);
				break;
			}
		}
	}
}
