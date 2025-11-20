using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Auth;

public sealed class SignInResponse : IMessage<SignInResponse>, IMessage, IEquatable<SignInResponse>, IDeepCloneable<SignInResponse>, IBufferMessage
{
	private static readonly MessageParser<SignInResponse> _parser = new MessageParser<SignInResponse>(() => new SignInResponse());

	private UnknownFieldSet _unknownFields;

	public const int RequestContextFieldNumber = 1;

	private HydraRequestContext requestContext_;

	public const int UserContextFieldNumber = 2;

	private UserContext userContext_;

	public const int EndpointsFieldNumber = 3;

	private static readonly FieldCodec<EndpointInfo> _repeated_endpoints_codec = FieldCodec.ForMessage(26u, EndpointInfo.Parser);

	private readonly RepeatedField<EndpointInfo> endpoints_ = new RepeatedField<EndpointInfo>();

	public const int DateFieldNumber = 4;

	private long date_;

	public const int RefreshAfterSecondsFieldNumber = 5;

	private int refreshAfterSeconds_;

	public const int ExternalIdentityTokenFieldNumber = 7;

	private string externalIdentityToken_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SignInResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[24];

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
	public UserContext UserContext
	{
		get
		{
			return userContext_;
		}
		set
		{
			userContext_ = value;
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
	public string ExternalIdentityToken
	{
		get
		{
			return externalIdentityToken_;
		}
		set
		{
			externalIdentityToken_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SignInResponse()
	{
	}

	[DebuggerNonUserCode]
	public SignInResponse(SignInResponse other)
		: this()
	{
		requestContext_ = ((other.requestContext_ != null) ? other.requestContext_.Clone() : null);
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		endpoints_ = other.endpoints_.Clone();
		date_ = other.date_;
		refreshAfterSeconds_ = other.refreshAfterSeconds_;
		externalIdentityToken_ = other.externalIdentityToken_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInResponse Clone()
	{
		return new SignInResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInResponse other)
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
		if (!object.Equals(UserContext, other.UserContext))
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
		if (RefreshAfterSeconds != other.RefreshAfterSeconds)
		{
			return false;
		}
		if (ExternalIdentityToken != other.ExternalIdentityToken)
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
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		num ^= endpoints_.GetHashCode();
		if (Date != 0)
		{
			num ^= Date.GetHashCode();
		}
		if (RefreshAfterSeconds != 0)
		{
			num ^= RefreshAfterSeconds.GetHashCode();
		}
		if (ExternalIdentityToken.Length != 0)
		{
			num ^= ExternalIdentityToken.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(UserContext);
		}
		endpoints_.WriteTo(ref output, _repeated_endpoints_codec);
		if (Date != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt64(Date);
		}
		if (RefreshAfterSeconds != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(RefreshAfterSeconds);
		}
		if (ExternalIdentityToken.Length != 0)
		{
			output.WriteRawTag(58);
			output.WriteString(ExternalIdentityToken);
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
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		num += endpoints_.CalculateSize(_repeated_endpoints_codec);
		if (Date != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Date);
		}
		if (RefreshAfterSeconds != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RefreshAfterSeconds);
		}
		if (ExternalIdentityToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ExternalIdentityToken);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInResponse other)
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
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
		}
		endpoints_.Add(other.endpoints_);
		if (other.Date != 0)
		{
			Date = other.Date;
		}
		if (other.RefreshAfterSeconds != 0)
		{
			RefreshAfterSeconds = other.RefreshAfterSeconds;
		}
		if (other.ExternalIdentityToken.Length != 0)
		{
			ExternalIdentityToken = other.ExternalIdentityToken;
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 26u:
				endpoints_.AddEntriesFrom(ref input, _repeated_endpoints_codec);
				break;
			case 32u:
				Date = input.ReadInt64();
				break;
			case 40u:
				RefreshAfterSeconds = input.ReadInt32();
				break;
			case 58u:
				ExternalIdentityToken = input.ReadString();
				break;
			}
		}
	}
}
