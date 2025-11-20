using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Auth;

namespace Hydra.Api.User;

public sealed class UserBaseData : IMessage<UserBaseData>, IMessage, IEquatable<UserBaseData>, IDeepCloneable<UserBaseData>, IBufferMessage
{
	private static readonly MessageParser<UserBaseData> _parser = new MessageParser<UserBaseData>(() => new UserBaseData());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int ProviderIdFieldNumber = 2;

	private Provider providerId_ = Provider.Steam;

	public const int ProviderUserIdFieldNumber = 3;

	private string providerUserId_ = "";

	public const int PlatformFieldNumber = 4;

	private string platform_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<UserBaseData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => UserContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string UserId
	{
		get
		{
			return userId_;
		}
		set
		{
			userId_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public string ProviderUserId
	{
		get
		{
			return providerUserId_;
		}
		set
		{
			providerUserId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Platform
	{
		get
		{
			return platform_;
		}
		set
		{
			platform_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public UserBaseData()
	{
	}

	[DebuggerNonUserCode]
	public UserBaseData(UserBaseData other)
		: this()
	{
		userId_ = other.userId_;
		providerId_ = other.providerId_;
		providerUserId_ = other.providerUserId_;
		platform_ = other.platform_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserBaseData Clone()
	{
		return new UserBaseData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserBaseData);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserBaseData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UserId != other.UserId)
		{
			return false;
		}
		if (ProviderId != other.ProviderId)
		{
			return false;
		}
		if (ProviderUserId != other.ProviderUserId)
		{
			return false;
		}
		if (Platform != other.Platform)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UserId.Length != 0)
		{
			num ^= UserId.GetHashCode();
		}
		if (ProviderId != Provider.Steam)
		{
			num ^= ProviderId.GetHashCode();
		}
		if (ProviderUserId.Length != 0)
		{
			num ^= ProviderUserId.GetHashCode();
		}
		if (Platform.Length != 0)
		{
			num ^= Platform.GetHashCode();
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
		if (UserId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(UserId);
		}
		if (ProviderId != Provider.Steam)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)ProviderId);
		}
		if (ProviderUserId.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(ProviderUserId);
		}
		if (Platform.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(Platform);
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
		if (UserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserId);
		}
		if (ProviderId != Provider.Steam)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ProviderId);
		}
		if (ProviderUserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ProviderUserId);
		}
		if (Platform.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Platform);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserBaseData other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			if (other.ProviderId != Provider.Steam)
			{
				ProviderId = other.ProviderId;
			}
			if (other.ProviderUserId.Length != 0)
			{
				ProviderUserId = other.ProviderUserId;
			}
			if (other.Platform.Length != 0)
			{
				Platform = other.Platform;
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
				UserId = input.ReadString();
				break;
			case 16u:
				ProviderId = (Provider)input.ReadEnum();
				break;
			case 26u:
				ProviderUserId = input.ReadString();
				break;
			case 34u:
				Platform = input.ReadString();
				break;
			}
		}
	}
}
