using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelUser : IMessage<ChannelUser>, IMessage, IEquatable<ChannelUser>, IDeepCloneable<ChannelUser>, IBufferMessage
{
	private static readonly MessageParser<ChannelUser> _parser = new MessageParser<ChannelUser>(() => new ChannelUser());

	private UnknownFieldSet _unknownFields;

	public const int UserFieldNumber = 1;

	private string user_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ChannelUser> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[26];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string User
	{
		get
		{
			return user_;
		}
		set
		{
			user_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ChannelUser()
	{
	}

	[DebuggerNonUserCode]
	public ChannelUser(ChannelUser other)
		: this()
	{
		user_ = other.user_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelUser Clone()
	{
		return new ChannelUser(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelUser);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelUser other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (User != other.User)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (User.Length != 0)
		{
			num ^= User.GetHashCode();
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
		if (User.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(User);
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
		if (User.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(User);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelUser other)
	{
		if (other != null)
		{
			if (other.User.Length != 0)
			{
				User = other.User;
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				User = input.ReadString();
			}
		}
	}
}
