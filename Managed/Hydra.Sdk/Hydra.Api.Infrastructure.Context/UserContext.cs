using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public sealed class UserContext : IMessage<UserContext>, IMessage, IEquatable<UserContext>, IDeepCloneable<UserContext>, IBufferMessage
{
	private static readonly MessageParser<UserContext> _parser = new MessageParser<UserContext>(() => new UserContext());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private UserContextData data_;

	public const int SignatureFieldNumber = 2;

	private ByteString signature_ = ByteString.Empty;

	[DebuggerNonUserCode]
	public static MessageParser<UserContext> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => UserContextReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContextData Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ByteString Signature
	{
		get
		{
			return signature_;
		}
		set
		{
			signature_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public UserContext()
	{
	}

	[DebuggerNonUserCode]
	public UserContext(UserContext other)
		: this()
	{
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		signature_ = other.signature_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserContext Clone()
	{
		return new UserContext(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserContext);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserContext other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Data, other.Data))
		{
			return false;
		}
		if (Signature != other.Signature)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (data_ != null)
		{
			num ^= Data.GetHashCode();
		}
		if (Signature.Length != 0)
		{
			num ^= Signature.GetHashCode();
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
		if (data_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Data);
		}
		if (Signature.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteBytes(Signature);
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
		if (data_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Data);
		}
		if (Signature.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Signature);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserContext other)
	{
		if (other == null)
		{
			return;
		}
		if (other.data_ != null)
		{
			if (data_ == null)
			{
				Data = new UserContextData();
			}
			Data.MergeFrom(other.Data);
		}
		if (other.Signature.Length != 0)
		{
			Signature = other.Signature;
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
				if (data_ == null)
				{
					Data = new UserContextData();
				}
				input.ReadMessage(Data);
				break;
			case 18u:
				Signature = input.ReadBytes();
				break;
			}
		}
	}
}
