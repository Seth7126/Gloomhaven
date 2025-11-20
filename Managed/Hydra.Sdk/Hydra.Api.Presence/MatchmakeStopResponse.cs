using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class MatchmakeStopResponse : IMessage<MatchmakeStopResponse>, IMessage, IEquatable<MatchmakeStopResponse>, IDeepCloneable<MatchmakeStopResponse>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeStopResponse> _parser = new MessageParser<MatchmakeStopResponse>(() => new MatchmakeStopResponse());

	private UnknownFieldSet _unknownFields;

	public const int CorrelationIdFieldNumber = 1;

	private string correlationId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeStopResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceServiceContractsReflection.Descriptor.MessageTypes[41];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string CorrelationId
	{
		get
		{
			return correlationId_;
		}
		set
		{
			correlationId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public MatchmakeStopResponse()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeStopResponse(MatchmakeStopResponse other)
		: this()
	{
		correlationId_ = other.correlationId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeStopResponse Clone()
	{
		return new MatchmakeStopResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeStopResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeStopResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CorrelationId != other.CorrelationId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (CorrelationId.Length != 0)
		{
			num ^= CorrelationId.GetHashCode();
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
		if (CorrelationId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(CorrelationId);
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
		if (CorrelationId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CorrelationId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeStopResponse other)
	{
		if (other != null)
		{
			if (other.CorrelationId.Length != 0)
			{
				CorrelationId = other.CorrelationId;
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
				CorrelationId = input.ReadString();
			}
		}
	}
}
