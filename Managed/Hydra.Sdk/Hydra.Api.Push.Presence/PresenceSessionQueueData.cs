using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Presence;

namespace Hydra.Api.Push.Presence;

public sealed class PresenceSessionQueueData : IMessage<PresenceSessionQueueData>, IMessage, IEquatable<PresenceSessionQueueData>, IDeepCloneable<PresenceSessionQueueData>, IBufferMessage
{
	private static readonly MessageParser<PresenceSessionQueueData> _parser = new MessageParser<PresenceSessionQueueData>(() => new PresenceSessionQueueData());

	private UnknownFieldSet _unknownFields;

	public const int PlayListIdFieldNumber = 1;

	private string playListId_ = "";

	public const int QueueVariantsFieldNumber = 2;

	private static readonly FieldCodec<QueueVariants> _repeated_queueVariants_codec = FieldCodec.ForMessage(18u, Hydra.Api.Presence.QueueVariants.Parser);

	private readonly RepeatedField<QueueVariants> queueVariants_ = new RepeatedField<QueueVariants>();

	[DebuggerNonUserCode]
	public static MessageParser<PresenceSessionQueueData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string PlayListId
	{
		get
		{
			return playListId_;
		}
		set
		{
			playListId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<QueueVariants> QueueVariants => queueVariants_;

	[DebuggerNonUserCode]
	public PresenceSessionQueueData()
	{
	}

	[DebuggerNonUserCode]
	public PresenceSessionQueueData(PresenceSessionQueueData other)
		: this()
	{
		playListId_ = other.playListId_;
		queueVariants_ = other.queueVariants_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PresenceSessionQueueData Clone()
	{
		return new PresenceSessionQueueData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PresenceSessionQueueData);
	}

	[DebuggerNonUserCode]
	public bool Equals(PresenceSessionQueueData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PlayListId != other.PlayListId)
		{
			return false;
		}
		if (!queueVariants_.Equals(other.queueVariants_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PlayListId.Length != 0)
		{
			num ^= PlayListId.GetHashCode();
		}
		num ^= queueVariants_.GetHashCode();
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
		if (PlayListId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(PlayListId);
		}
		queueVariants_.WriteTo(ref output, _repeated_queueVariants_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (PlayListId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PlayListId);
		}
		num += queueVariants_.CalculateSize(_repeated_queueVariants_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PresenceSessionQueueData other)
	{
		if (other != null)
		{
			if (other.PlayListId.Length != 0)
			{
				PlayListId = other.PlayListId;
			}
			queueVariants_.Add(other.queueVariants_);
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
				PlayListId = input.ReadString();
				break;
			case 18u:
				queueVariants_.AddEntriesFrom(ref input, _repeated_queueVariants_codec);
				break;
			}
		}
	}
}
