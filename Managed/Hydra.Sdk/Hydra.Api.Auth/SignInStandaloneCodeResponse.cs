using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;

namespace Hydra.Api.Auth;

public sealed class SignInStandaloneCodeResponse : IMessage<SignInStandaloneCodeResponse>, IMessage, IEquatable<SignInStandaloneCodeResponse>, IDeepCloneable<SignInStandaloneCodeResponse>, IBufferMessage
{
	private static readonly MessageParser<SignInStandaloneCodeResponse> _parser = new MessageParser<SignInStandaloneCodeResponse>(() => new SignInStandaloneCodeResponse());

	private UnknownFieldSet _unknownFields;

	public const int RequestContextFieldNumber = 1;

	private HydraRequestContext requestContext_;

	public const int EndpointsFieldNumber = 2;

	private static readonly FieldCodec<EndpointInfo> _repeated_endpoints_codec = FieldCodec.ForMessage(18u, EndpointInfo.Parser);

	private readonly RepeatedField<EndpointInfo> endpoints_ = new RepeatedField<EndpointInfo>();

	public const int DateFieldNumber = 3;

	private long date_;

	public const int ServerTokenFieldNumber = 4;

	private string serverToken_ = "";

	public const int ExpiresAtFieldNumber = 5;

	private long expiresAt_;

	[DebuggerNonUserCode]
	public static MessageParser<SignInStandaloneCodeResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[34];

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
	public string ServerToken
	{
		get
		{
			return serverToken_;
		}
		set
		{
			serverToken_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public SignInStandaloneCodeResponse()
	{
	}

	[DebuggerNonUserCode]
	public SignInStandaloneCodeResponse(SignInStandaloneCodeResponse other)
		: this()
	{
		requestContext_ = ((other.requestContext_ != null) ? other.requestContext_.Clone() : null);
		endpoints_ = other.endpoints_.Clone();
		date_ = other.date_;
		serverToken_ = other.serverToken_;
		expiresAt_ = other.expiresAt_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInStandaloneCodeResponse Clone()
	{
		return new SignInStandaloneCodeResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInStandaloneCodeResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInStandaloneCodeResponse other)
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
		if (!endpoints_.Equals(other.endpoints_))
		{
			return false;
		}
		if (Date != other.Date)
		{
			return false;
		}
		if (ServerToken != other.ServerToken)
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
		num ^= endpoints_.GetHashCode();
		if (Date != 0)
		{
			num ^= Date.GetHashCode();
		}
		if (ServerToken.Length != 0)
		{
			num ^= ServerToken.GetHashCode();
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
		endpoints_.WriteTo(ref output, _repeated_endpoints_codec);
		if (Date != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt64(Date);
		}
		if (ServerToken.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(ServerToken);
		}
		if (ExpiresAt != 0)
		{
			output.WriteRawTag(40);
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
		num += endpoints_.CalculateSize(_repeated_endpoints_codec);
		if (Date != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Date);
		}
		if (ServerToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerToken);
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
	public void MergeFrom(SignInStandaloneCodeResponse other)
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
		endpoints_.Add(other.endpoints_);
		if (other.Date != 0)
		{
			Date = other.Date;
		}
		if (other.ServerToken.Length != 0)
		{
			ServerToken = other.ServerToken;
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
				endpoints_.AddEntriesFrom(ref input, _repeated_endpoints_codec);
				break;
			case 24u:
				Date = input.ReadInt64();
				break;
			case 34u:
				ServerToken = input.ReadString();
				break;
			case 40u:
				ExpiresAt = input.ReadInt64();
				break;
			}
		}
	}
}
