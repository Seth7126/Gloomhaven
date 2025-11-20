using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class KeyContainer : IMessage<KeyContainer>, IMessage, IEquatable<KeyContainer>, IDeepCloneable<KeyContainer>, IBufferMessage
{
	private static readonly MessageParser<KeyContainer> _parser = new MessageParser<KeyContainer>(() => new KeyContainer());

	private UnknownFieldSet _unknownFields;

	public const int KeyFieldNumber = 1;

	private ByteString key_ = ByteString.Empty;

	public const int HashKeyFieldNumber = 2;

	private ByteString hashKey_ = ByteString.Empty;

	public const int InitVectorValueFieldNumber = 3;

	private ByteString initVectorValue_ = ByteString.Empty;

	public const int NonceFieldNumber = 4;

	private ByteString nonce_ = ByteString.Empty;

	[DebuggerNonUserCode]
	public static MessageParser<KeyContainer> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => KeyContainerReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ByteString Key
	{
		get
		{
			return key_;
		}
		set
		{
			key_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ByteString HashKey
	{
		get
		{
			return hashKey_;
		}
		set
		{
			hashKey_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ByteString InitVectorValue
	{
		get
		{
			return initVectorValue_;
		}
		set
		{
			initVectorValue_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ByteString Nonce
	{
		get
		{
			return nonce_;
		}
		set
		{
			nonce_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public KeyContainer()
	{
	}

	[DebuggerNonUserCode]
	public KeyContainer(KeyContainer other)
		: this()
	{
		key_ = other.key_;
		hashKey_ = other.hashKey_;
		initVectorValue_ = other.initVectorValue_;
		nonce_ = other.nonce_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public KeyContainer Clone()
	{
		return new KeyContainer(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as KeyContainer);
	}

	[DebuggerNonUserCode]
	public bool Equals(KeyContainer other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Key != other.Key)
		{
			return false;
		}
		if (HashKey != other.HashKey)
		{
			return false;
		}
		if (InitVectorValue != other.InitVectorValue)
		{
			return false;
		}
		if (Nonce != other.Nonce)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Key.Length != 0)
		{
			num ^= Key.GetHashCode();
		}
		if (HashKey.Length != 0)
		{
			num ^= HashKey.GetHashCode();
		}
		if (InitVectorValue.Length != 0)
		{
			num ^= InitVectorValue.GetHashCode();
		}
		if (Nonce.Length != 0)
		{
			num ^= Nonce.GetHashCode();
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
		if (Key.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteBytes(Key);
		}
		if (HashKey.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteBytes(HashKey);
		}
		if (InitVectorValue.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteBytes(InitVectorValue);
		}
		if (Nonce.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteBytes(Nonce);
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
		if (Key.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Key);
		}
		if (HashKey.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(HashKey);
		}
		if (InitVectorValue.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(InitVectorValue);
		}
		if (Nonce.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Nonce);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(KeyContainer other)
	{
		if (other != null)
		{
			if (other.Key.Length != 0)
			{
				Key = other.Key;
			}
			if (other.HashKey.Length != 0)
			{
				HashKey = other.HashKey;
			}
			if (other.InitVectorValue.Length != 0)
			{
				InitVectorValue = other.InitVectorValue;
			}
			if (other.Nonce.Length != 0)
			{
				Nonce = other.Nonce;
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
				Key = input.ReadBytes();
				break;
			case 18u:
				HashKey = input.ReadBytes();
				break;
			case 26u:
				InitVectorValue = input.ReadBytes();
				break;
			case 34u:
				Nonce = input.ReadBytes();
				break;
			}
		}
	}
}
