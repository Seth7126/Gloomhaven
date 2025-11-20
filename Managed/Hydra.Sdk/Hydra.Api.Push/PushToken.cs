using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public sealed class PushToken : IMessage<PushToken>, IMessage, IEquatable<PushToken>, IDeepCloneable<PushToken>, IBufferMessage
{
	private static readonly MessageParser<PushToken> _parser = new MessageParser<PushToken>(() => new PushToken());

	private UnknownFieldSet _unknownFields;

	public const int InstanceKeyFieldNumber = 1;

	private int instanceKey_;

	public const int UserKeyFieldNumber = 3;

	private string userKey_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<PushToken> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushTokenReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int InstanceKey
	{
		get
		{
			return instanceKey_;
		}
		set
		{
			instanceKey_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string UserKey
	{
		get
		{
			return userKey_;
		}
		set
		{
			userKey_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PushToken()
	{
	}

	[DebuggerNonUserCode]
	public PushToken(PushToken other)
		: this()
	{
		instanceKey_ = other.instanceKey_;
		userKey_ = other.userKey_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushToken Clone()
	{
		return new PushToken(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushToken);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushToken other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (InstanceKey != other.InstanceKey)
		{
			return false;
		}
		if (UserKey != other.UserKey)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (InstanceKey != 0)
		{
			num ^= InstanceKey.GetHashCode();
		}
		if (UserKey.Length != 0)
		{
			num ^= UserKey.GetHashCode();
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
		if (InstanceKey != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(InstanceKey);
		}
		if (UserKey.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(UserKey);
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
		if (InstanceKey != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(InstanceKey);
		}
		if (UserKey.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserKey);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushToken other)
	{
		if (other != null)
		{
			if (other.InstanceKey != 0)
			{
				InstanceKey = other.InstanceKey;
			}
			if (other.UserKey.Length != 0)
			{
				UserKey = other.UserKey;
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
				InstanceKey = input.ReadInt32();
				break;
			case 26u:
				UserKey = input.ReadString();
				break;
			}
		}
	}
}
