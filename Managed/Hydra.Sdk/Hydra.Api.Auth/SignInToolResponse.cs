using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Auth;

public sealed class SignInToolResponse : IMessage<SignInToolResponse>, IMessage, IEquatable<SignInToolResponse>, IDeepCloneable<SignInToolResponse>, IBufferMessage
{
	private static readonly MessageParser<SignInToolResponse> _parser = new MessageParser<SignInToolResponse>(() => new SignInToolResponse());

	private UnknownFieldSet _unknownFields;

	public const int RequestContextFieldNumber = 1;

	private HydraRequestContext requestContext_;

	public const int ToolContextFieldNumber = 2;

	private ToolContext toolContext_;

	public const int EndpointsFieldNumber = 3;

	private static readonly FieldCodec<EndpointInfo> _repeated_endpoints_codec = FieldCodec.ForMessage(26u, EndpointInfo.Parser);

	private readonly RepeatedField<EndpointInfo> endpoints_ = new RepeatedField<EndpointInfo>();

	public const int DateFieldNumber = 4;

	private long date_;

	public const int ExpiresAtFieldNumber = 6;

	private long expiresAt_;

	[DebuggerNonUserCode]
	public static MessageParser<SignInToolResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[38];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public HydraRequestContext RequestContext
	{
		get
		{
			return requestContext_;
		}
		set
		{
			requestContext_ = value;
		}
	}

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
	public RepeatedField<EndpointInfo> Endpoints => endpoints_;

	[DebuggerNonUserCode]
	public long Date
	{
		get
		{
			return date_;
		}
		set
		{
			date_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long ExpiresAt
	{
		get
		{
			return expiresAt_;
		}
		set
		{
			expiresAt_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SignInToolResponse()
	{
	}

	[DebuggerNonUserCode]
	public SignInToolResponse(SignInToolResponse other)
		: this()
	{
		requestContext_ = ((other.requestContext_ != null) ? other.requestContext_.Clone() : null);
		toolContext_ = ((other.toolContext_ != null) ? other.toolContext_.Clone() : null);
		endpoints_ = other.endpoints_.Clone();
		date_ = other.date_;
		expiresAt_ = other.expiresAt_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInToolResponse Clone()
	{
		return new SignInToolResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInToolResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInToolResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RequestContext, other.RequestContext))
		{
			return false;
		}
		if (!object.Equals(ToolContext, other.ToolContext))
		{
			return false;
		}
		if (!endpoints_.Equals(other.endpoints_))
		{
			return false;
		}
		if (Date != other.Date)
		{
			return false;
		}
		if (ExpiresAt != other.ExpiresAt)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (requestContext_ != null)
		{
			num ^= RequestContext.GetHashCode();
		}
		if (toolContext_ != null)
		{
			num ^= ToolContext.GetHashCode();
		}
		num ^= endpoints_.GetHashCode();
		if (Date != 0)
		{
			num ^= Date.GetHashCode();
		}
		if (ExpiresAt != 0)
		{
			num ^= ExpiresAt.GetHashCode();
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
		if (requestContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(RequestContext);
		}
		if (toolContext_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ToolContext);
		}
		endpoints_.WriteTo(ref output, _repeated_endpoints_codec);
		if (Date != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt64(Date);
		}
		if (ExpiresAt != 0)
		{
			output.WriteRawTag(48);
			output.WriteInt64(ExpiresAt);
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
		if (requestContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(RequestContext);
		}
		if (toolContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ToolContext);
		}
		num += endpoints_.CalculateSize(_repeated_endpoints_codec);
		if (Date != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Date);
		}
		if (ExpiresAt != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExpiresAt);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInToolResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.requestContext_ != null)
		{
			if (requestContext_ == null)
			{
				RequestContext = new HydraRequestContext();
			}
			RequestContext.MergeFrom(other.RequestContext);
		}
		if (other.toolContext_ != null)
		{
			if (toolContext_ == null)
			{
				ToolContext = new ToolContext();
			}
			ToolContext.MergeFrom(other.ToolContext);
		}
		endpoints_.Add(other.endpoints_);
		if (other.Date != 0)
		{
			Date = other.Date;
		}
		if (other.ExpiresAt != 0)
		{
			ExpiresAt = other.ExpiresAt;
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
				if (requestContext_ == null)
				{
					RequestContext = new HydraRequestContext();
				}
				input.ReadMessage(RequestContext);
				break;
			case 18u:
				if (toolContext_ == null)
				{
					ToolContext = new ToolContext();
				}
				input.ReadMessage(ToolContext);
				break;
			case 26u:
				endpoints_.AddEntriesFrom(ref input, _repeated_endpoints_codec);
				break;
			case 32u:
				Date = input.ReadInt64();
				break;
			case 48u:
				ExpiresAt = input.ReadInt64();
				break;
			}
		}
	}
}
