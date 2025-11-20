using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public sealed class PushMessageHeader : IMessage<PushMessageHeader>, IMessage, IEquatable<PushMessageHeader>, IDeepCloneable<PushMessageHeader>, IBufferMessage
{
	private static readonly MessageParser<PushMessageHeader> _parser = new MessageParser<PushMessageHeader>(() => new PushMessageHeader());

	private UnknownFieldSet _unknownFields;

	public const int TokenFieldNumber = 1;

	private PushToken token_;

	public const int MessageTypeFieldNumber = 2;

	private PushMessageType messageType_ = PushMessageType.Undefined;

	public const int VersionFieldNumber = 3;

	private int version_;

	[DebuggerNonUserCode]
	public static MessageParser<PushMessageHeader> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushMessageHeaderReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PushToken Token
	{
		get
		{
			return token_;
		}
		set
		{
			token_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PushMessageType MessageType
	{
		get
		{
			return messageType_;
		}
		set
		{
			messageType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PushMessageHeader()
	{
	}

	[DebuggerNonUserCode]
	public PushMessageHeader(PushMessageHeader other)
		: this()
	{
		token_ = ((other.token_ != null) ? other.token_.Clone() : null);
		messageType_ = other.messageType_;
		version_ = other.version_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushMessageHeader Clone()
	{
		return new PushMessageHeader(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushMessageHeader);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushMessageHeader other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Token, other.Token))
		{
			return false;
		}
		if (MessageType != other.MessageType)
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (token_ != null)
		{
			num ^= Token.GetHashCode();
		}
		if (MessageType != PushMessageType.Undefined)
		{
			num ^= MessageType.GetHashCode();
		}
		if (Version != 0)
		{
			num ^= Version.GetHashCode();
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
		if (token_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Token);
		}
		if (MessageType != PushMessageType.Undefined)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)MessageType);
		}
		if (Version != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(Version);
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
		if (token_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Token);
		}
		if (MessageType != PushMessageType.Undefined)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)MessageType);
		}
		if (Version != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Version);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushMessageHeader other)
	{
		if (other == null)
		{
			return;
		}
		if (other.token_ != null)
		{
			if (token_ == null)
			{
				Token = new PushToken();
			}
			Token.MergeFrom(other.Token);
		}
		if (other.MessageType != PushMessageType.Undefined)
		{
			MessageType = other.MessageType;
		}
		if (other.Version != 0)
		{
			Version = other.Version;
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
				if (token_ == null)
				{
					Token = new PushToken();
				}
				input.ReadMessage(Token);
				break;
			case 16u:
				MessageType = (PushMessageType)input.ReadEnum();
				break;
			case 24u:
				Version = input.ReadInt32();
				break;
			}
		}
	}
}
