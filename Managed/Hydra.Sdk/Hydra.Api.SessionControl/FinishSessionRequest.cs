using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.SessionControl;

public sealed class FinishSessionRequest : IMessage<FinishSessionRequest>, IMessage, IEquatable<FinishSessionRequest>, IDeepCloneable<FinishSessionRequest>, IBufferMessage
{
	private static readonly MessageParser<FinishSessionRequest> _parser = new MessageParser<FinishSessionRequest>(() => new FinishSessionRequest());

	private UnknownFieldSet _unknownFields;

	public const int ServerContextFieldNumber = 1;

	private ServerContext serverContext_;

	[DebuggerNonUserCode]
	public static MessageParser<FinishSessionRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[12];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServerContext ServerContext
	{
		get
		{
			return serverContext_;
		}
		set
		{
			serverContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public FinishSessionRequest()
	{
	}

	[DebuggerNonUserCode]
	public FinishSessionRequest(FinishSessionRequest other)
		: this()
	{
		serverContext_ = ((other.serverContext_ != null) ? other.serverContext_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public FinishSessionRequest Clone()
	{
		return new FinishSessionRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as FinishSessionRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(FinishSessionRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ServerContext, other.ServerContext))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (serverContext_ != null)
		{
			num ^= ServerContext.GetHashCode();
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
		if (serverContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ServerContext);
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
		if (serverContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ServerContext);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(FinishSessionRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.serverContext_ != null)
		{
			if (serverContext_ == null)
			{
				ServerContext = new ServerContext();
			}
			ServerContext.MergeFrom(other.ServerContext);
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
			if (serverContext_ == null)
			{
				ServerContext = new ServerContext();
			}
			input.ReadMessage(ServerContext);
		}
	}
}
