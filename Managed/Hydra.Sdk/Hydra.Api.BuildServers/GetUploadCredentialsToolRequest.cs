using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.BuildServers;

public sealed class GetUploadCredentialsToolRequest : IMessage<GetUploadCredentialsToolRequest>, IMessage, IEquatable<GetUploadCredentialsToolRequest>, IDeepCloneable<GetUploadCredentialsToolRequest>, IBufferMessage
{
	private static readonly MessageParser<GetUploadCredentialsToolRequest> _parser = new MessageParser<GetUploadCredentialsToolRequest>(() => new GetUploadCredentialsToolRequest());

	private UnknownFieldSet _unknownFields;

	public const int ToolContextFieldNumber = 1;

	private ToolContext toolContext_;

	[DebuggerNonUserCode]
	public static MessageParser<GetUploadCredentialsToolRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => BuildServersContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ToolContext ToolContext
	{
		get
		{
			return toolContext_;
		}
		set
		{
			toolContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetUploadCredentialsToolRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetUploadCredentialsToolRequest(GetUploadCredentialsToolRequest other)
		: this()
	{
		toolContext_ = ((other.toolContext_ != null) ? other.toolContext_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetUploadCredentialsToolRequest Clone()
	{
		return new GetUploadCredentialsToolRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetUploadCredentialsToolRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetUploadCredentialsToolRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ToolContext, other.ToolContext))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (toolContext_ != null)
		{
			num ^= ToolContext.GetHashCode();
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
		if (toolContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ToolContext);
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
		if (toolContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ToolContext);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetUploadCredentialsToolRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.toolContext_ != null)
		{
			if (toolContext_ == null)
			{
				ToolContext = new ToolContext();
			}
			ToolContext.MergeFrom(other.ToolContext);
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
			if (toolContext_ == null)
			{
				ToolContext = new ToolContext();
			}
			input.ReadMessage(ToolContext);
		}
	}
}
