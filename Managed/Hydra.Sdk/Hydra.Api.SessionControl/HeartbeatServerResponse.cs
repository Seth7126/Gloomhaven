using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class HeartbeatServerResponse : IMessage<HeartbeatServerResponse>, IMessage, IEquatable<HeartbeatServerResponse>, IDeepCloneable<HeartbeatServerResponse>, IBufferMessage
{
	private static readonly MessageParser<HeartbeatServerResponse> _parser = new MessageParser<HeartbeatServerResponse>(() => new HeartbeatServerResponse());

	private UnknownFieldSet _unknownFields;

	public const int RefreshAfterSecondsFieldNumber = 1;

	private int refreshAfterSeconds_;

	[DebuggerNonUserCode]
	public static MessageParser<HeartbeatServerResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[30];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int RefreshAfterSeconds
	{
		get
		{
			return refreshAfterSeconds_;
		}
		set
		{
			refreshAfterSeconds_ = value;
		}
	}

	[DebuggerNonUserCode]
	public HeartbeatServerResponse()
	{
	}

	[DebuggerNonUserCode]
	public HeartbeatServerResponse(HeartbeatServerResponse other)
		: this()
	{
		refreshAfterSeconds_ = other.refreshAfterSeconds_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public HeartbeatServerResponse Clone()
	{
		return new HeartbeatServerResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as HeartbeatServerResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(HeartbeatServerResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (RefreshAfterSeconds != other.RefreshAfterSeconds)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (RefreshAfterSeconds != 0)
		{
			num ^= RefreshAfterSeconds.GetHashCode();
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
		if (RefreshAfterSeconds != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(RefreshAfterSeconds);
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
		if (RefreshAfterSeconds != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RefreshAfterSeconds);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(HeartbeatServerResponse other)
	{
		if (other != null)
		{
			if (other.RefreshAfterSeconds != 0)
			{
				RefreshAfterSeconds = other.RefreshAfterSeconds;
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
			if (num3 != 8)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				RefreshAfterSeconds = input.ReadInt32();
			}
		}
	}
}
