using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.EndpointDispatcher;

public sealed class TitleIdentity : IMessage<TitleIdentity>, IMessage, IEquatable<TitleIdentity>, IDeepCloneable<TitleIdentity>, IBufferMessage
{
	private static readonly MessageParser<TitleIdentity> _parser = new MessageParser<TitleIdentity>(() => new TitleIdentity());

	private UnknownFieldSet _unknownFields;

	public const int TitleIdFieldNumber = 1;

	private string titleId_ = "";

	public const int TitleSecretFieldNumber = 2;

	private string titleSecret_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<TitleIdentity> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EndpointDispatcherContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string TitleId
	{
		get
		{
			return titleId_;
		}
		set
		{
			titleId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string TitleSecret
	{
		get
		{
			return titleSecret_;
		}
		set
		{
			titleSecret_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TitleIdentity()
	{
	}

	[DebuggerNonUserCode]
	public TitleIdentity(TitleIdentity other)
		: this()
	{
		titleId_ = other.titleId_;
		titleSecret_ = other.titleSecret_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TitleIdentity Clone()
	{
		return new TitleIdentity(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TitleIdentity);
	}

	[DebuggerNonUserCode]
	public bool Equals(TitleIdentity other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TitleId != other.TitleId)
		{
			return false;
		}
		if (TitleSecret != other.TitleSecret)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TitleId.Length != 0)
		{
			num ^= TitleId.GetHashCode();
		}
		if (TitleSecret.Length != 0)
		{
			num ^= TitleSecret.GetHashCode();
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
		if (TitleId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TitleId);
		}
		if (TitleSecret.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(TitleSecret);
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
		if (TitleId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleId);
		}
		if (TitleSecret.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleSecret);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TitleIdentity other)
	{
		if (other != null)
		{
			if (other.TitleId.Length != 0)
			{
				TitleId = other.TitleId;
			}
			if (other.TitleSecret.Length != 0)
			{
				TitleSecret = other.TitleSecret;
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
				TitleId = input.ReadString();
				break;
			case 18u:
				TitleSecret = input.ReadString();
				break;
			}
		}
	}
}
