using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class ManagedGetServerInfoResponse : IMessage<ManagedGetServerInfoResponse>, IMessage, IEquatable<ManagedGetServerInfoResponse>, IDeepCloneable<ManagedGetServerInfoResponse>, IBufferMessage
{
	private static readonly MessageParser<ManagedGetServerInfoResponse> _parser = new MessageParser<ManagedGetServerInfoResponse>(() => new ManagedGetServerInfoResponse());

	private UnknownFieldSet _unknownFields;

	public const int AcceptClientResultFieldNumber = 1;

	private AcceptClientResult acceptClientResult_;

	[DebuggerNonUserCode]
	public static MessageParser<ManagedGetServerInfoResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public AcceptClientResult AcceptClientResult
	{
		get
		{
			return acceptClientResult_;
		}
		set
		{
			acceptClientResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ManagedGetServerInfoResponse()
	{
	}

	[DebuggerNonUserCode]
	public ManagedGetServerInfoResponse(ManagedGetServerInfoResponse other)
		: this()
	{
		acceptClientResult_ = ((other.acceptClientResult_ != null) ? other.acceptClientResult_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ManagedGetServerInfoResponse Clone()
	{
		return new ManagedGetServerInfoResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ManagedGetServerInfoResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(ManagedGetServerInfoResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(AcceptClientResult, other.AcceptClientResult))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (acceptClientResult_ != null)
		{
			num ^= AcceptClientResult.GetHashCode();
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
		if (acceptClientResult_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(AcceptClientResult);
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
		if (acceptClientResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(AcceptClientResult);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ManagedGetServerInfoResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.acceptClientResult_ != null)
		{
			if (acceptClientResult_ == null)
			{
				AcceptClientResult = new AcceptClientResult();
			}
			AcceptClientResult.MergeFrom(other.AcceptClientResult);
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				continue;
			}
			if (acceptClientResult_ == null)
			{
				AcceptClientResult = new AcceptClientResult();
			}
			input.ReadMessage(AcceptClientResult);
		}
	}
}
