using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Auth;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.User;

public sealed class UsersPublicDataByProviderUserIdRequest : IMessage<UsersPublicDataByProviderUserIdRequest>, IMessage, IEquatable<UsersPublicDataByProviderUserIdRequest>, IDeepCloneable<UsersPublicDataByProviderUserIdRequest>, IBufferMessage
{
	private static readonly MessageParser<UsersPublicDataByProviderUserIdRequest> _parser = new MessageParser<UsersPublicDataByProviderUserIdRequest>(() => new UsersPublicDataByProviderUserIdRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int ProviderIdFieldNumber = 2;

	private Provider providerId_ = Provider.Steam;

	public const int ProviderUserIdListFieldNumber = 3;

	private static readonly FieldCodec<string> _repeated_providerUserIdList_codec = FieldCodec.ForString(26u);

	private readonly RepeatedField<string> providerUserIdList_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<UsersPublicDataByProviderUserIdRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => UserContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public Provider ProviderId
	{
		get
		{
			return providerId_;
		}
		set
		{
			providerId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<string> ProviderUserIdList => providerUserIdList_;

	[DebuggerNonUserCode]
	public UsersPublicDataByProviderUserIdRequest()
	{
	}

	[DebuggerNonUserCode]
	public UsersPublicDataByProviderUserIdRequest(UsersPublicDataByProviderUserIdRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		providerId_ = other.providerId_;
		providerUserIdList_ = other.providerUserIdList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UsersPublicDataByProviderUserIdRequest Clone()
	{
		return new UsersPublicDataByProviderUserIdRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UsersPublicDataByProviderUserIdRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(UsersPublicDataByProviderUserIdRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		if (ProviderId != other.ProviderId)
		{
			return false;
		}
		if (!providerUserIdList_.Equals(other.providerUserIdList_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		if (ProviderId != Provider.Steam)
		{
			num ^= ProviderId.GetHashCode();
		}
		num ^= providerUserIdList_.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		if (ProviderId != Provider.Steam)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)ProviderId);
		}
		providerUserIdList_.WriteTo(ref output, _repeated_providerUserIdList_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (ProviderId != Provider.Steam)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ProviderId);
		}
		num += providerUserIdList_.CalculateSize(_repeated_providerUserIdList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UsersPublicDataByProviderUserIdRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
		}
		if (other.ProviderId != Provider.Steam)
		{
			ProviderId = other.ProviderId;
		}
		providerUserIdList_.Add(other.providerUserIdList_);
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 16u:
				ProviderId = (Provider)input.ReadEnum();
				break;
			case 26u:
				providerUserIdList_.AddEntriesFrom(ref input, _repeated_providerUserIdList_codec);
				break;
			}
		}
	}
}
