using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public sealed class UserContextData : IMessage<UserContextData>, IMessage, IEquatable<UserContextData>, IDeepCloneable<UserContextData>, IBufferMessage
{
	private static readonly MessageParser<UserContextData> _parser = new MessageParser<UserContextData>(() => new UserContextData());

	private UnknownFieldSet _unknownFields;

	public const int UserIdentityFieldNumber = 1;

	private string userIdentity_ = "";

	public const int TitleIdFieldNumber = 2;

	private string titleId_ = "";

	public const int PlatformFieldNumber = 3;

	private string platform_ = "";

	public const int KernelSessionIdFieldNumber = 4;

	private string kernelSessionId_ = "";

	public const int ProviderIdFieldNumber = 5;

	private string providerId_ = "";

	public const int UserIdentityTypeFieldNumber = 6;

	private UserIdentityType userIdentityType_ = UserIdentityType.Unknown;

	[DebuggerNonUserCode]
	public static MessageParser<UserContextData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => UserContextReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string UserIdentity
	{
		get
		{
			return userIdentity_;
		}
		set
		{
			userIdentity_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string TitleId
	{
		get
		{
			return titleId_;
		}
		set
		{
			titleId_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public string KernelSessionId
	{
		get
		{
			return kernelSessionId_;
		}
		set
		{
			kernelSessionId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ProviderId
	{
		get
		{
			return providerId_;
		}
		set
		{
			providerId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public UserIdentityType UserIdentityType
	{
		get
		{
			return userIdentityType_;
		}
		set
		{
			userIdentityType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UserContextData()
	{
	}

	[DebuggerNonUserCode]
	public UserContextData(UserContextData other)
		: this()
	{
		userIdentity_ = other.userIdentity_;
		titleId_ = other.titleId_;
		platform_ = other.platform_;
		kernelSessionId_ = other.kernelSessionId_;
		providerId_ = other.providerId_;
		userIdentityType_ = other.userIdentityType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserContextData Clone()
	{
		return new UserContextData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserContextData);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserContextData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UserIdentity != other.UserIdentity)
		{
			return false;
		}
		if (TitleId != other.TitleId)
		{
			return false;
		}
		if (Platform != other.Platform)
		{
			return false;
		}
		if (KernelSessionId != other.KernelSessionId)
		{
			return false;
		}
		if (ProviderId != other.ProviderId)
		{
			return false;
		}
		if (UserIdentityType != other.UserIdentityType)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UserIdentity.Length != 0)
		{
			num ^= UserIdentity.GetHashCode();
		}
		if (TitleId.Length != 0)
		{
			num ^= TitleId.GetHashCode();
		}
		if (Platform.Length != 0)
		{
			num ^= Platform.GetHashCode();
		}
		if (KernelSessionId.Length != 0)
		{
			num ^= KernelSessionId.GetHashCode();
		}
		if (ProviderId.Length != 0)
		{
			num ^= ProviderId.GetHashCode();
		}
		if (UserIdentityType != UserIdentityType.Unknown)
		{
			num ^= UserIdentityType.GetHashCode();
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
		if (UserIdentity.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(UserIdentity);
		}
		if (TitleId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(TitleId);
		}
		if (Platform.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(Platform);
		}
		if (KernelSessionId.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(KernelSessionId);
		}
		if (ProviderId.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(ProviderId);
		}
		if (UserIdentityType != UserIdentityType.Unknown)
		{
			output.WriteRawTag(48);
			output.WriteEnum((int)UserIdentityType);
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
		if (UserIdentity.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserIdentity);
		}
		if (TitleId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleId);
		}
		if (Platform.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Platform);
		}
		if (KernelSessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(KernelSessionId);
		}
		if (ProviderId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ProviderId);
		}
		if (UserIdentityType != UserIdentityType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)UserIdentityType);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserContextData other)
	{
		if (other != null)
		{
			if (other.UserIdentity.Length != 0)
			{
				UserIdentity = other.UserIdentity;
			}
			if (other.TitleId.Length != 0)
			{
				TitleId = other.TitleId;
			}
			if (other.Platform.Length != 0)
			{
				Platform = other.Platform;
			}
			if (other.KernelSessionId.Length != 0)
			{
				KernelSessionId = other.KernelSessionId;
			}
			if (other.ProviderId.Length != 0)
			{
				ProviderId = other.ProviderId;
			}
			if (other.UserIdentityType != UserIdentityType.Unknown)
			{
				UserIdentityType = other.UserIdentityType;
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
				UserIdentity = input.ReadString();
				break;
			case 18u:
				TitleId = input.ReadString();
				break;
			case 26u:
				Platform = input.ReadString();
				break;
			case 34u:
				KernelSessionId = input.ReadString();
				break;
			case 42u:
				ProviderId = input.ReadString();
				break;
			case 48u:
				UserIdentityType = (UserIdentityType)input.ReadEnum();
				break;
			}
		}
	}
}
