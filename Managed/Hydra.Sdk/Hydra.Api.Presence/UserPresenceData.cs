using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class UserPresenceData : IMessage<UserPresenceData>, IMessage, IEquatable<UserPresenceData>, IDeepCloneable<UserPresenceData>, IBufferMessage
{
	private static readonly MessageParser<UserPresenceData> _parser = new MessageParser<UserPresenceData>(() => new UserPresenceData());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int SessionFieldNumber = 2;

	private GameSessionData session_;

	public const int IsOnlineFieldNumber = 3;

	private bool isOnline_;

	public const int IsInvitableFieldNumber = 4;

	private bool isInvitable_;

	public const int StaticDataFieldNumber = 5;

	private string staticData_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<UserPresenceData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MatchmakeStatusReflection.Descriptor.MessageTypes[11];

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
	public GameSessionData Session
	{
		get
		{
			return session_;
		}
		set
		{
			session_ = value;
		}
	}

	[DebuggerNonUserCode]
	public bool IsOnline
	{
		get
		{
			return isOnline_;
		}
		set
		{
			isOnline_ = value;
		}
	}

	[DebuggerNonUserCode]
	public bool IsInvitable
	{
		get
		{
			return isInvitable_;
		}
		set
		{
			isInvitable_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string StaticData
	{
		get
		{
			return staticData_;
		}
		set
		{
			staticData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public UserPresenceData()
	{
	}

	[DebuggerNonUserCode]
	public UserPresenceData(UserPresenceData other)
		: this()
	{
		userId_ = other.userId_;
		session_ = ((other.session_ != null) ? other.session_.Clone() : null);
		isOnline_ = other.isOnline_;
		isInvitable_ = other.isInvitable_;
		staticData_ = other.staticData_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserPresenceData Clone()
	{
		return new UserPresenceData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserPresenceData);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserPresenceData other)
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
		if (!object.Equals(Session, other.Session))
		{
			return false;
		}
		if (IsOnline != other.IsOnline)
		{
			return false;
		}
		if (IsInvitable != other.IsInvitable)
		{
			return false;
		}
		if (StaticData != other.StaticData)
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
		if (session_ != null)
		{
			num ^= Session.GetHashCode();
		}
		if (IsOnline)
		{
			num ^= IsOnline.GetHashCode();
		}
		if (IsInvitable)
		{
			num ^= IsInvitable.GetHashCode();
		}
		if (StaticData.Length != 0)
		{
			num ^= StaticData.GetHashCode();
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
		if (session_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Session);
		}
		if (IsOnline)
		{
			output.WriteRawTag(24);
			output.WriteBool(IsOnline);
		}
		if (IsInvitable)
		{
			output.WriteRawTag(32);
			output.WriteBool(IsInvitable);
		}
		if (StaticData.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(StaticData);
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
		if (session_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Session);
		}
		if (IsOnline)
		{
			num += 2;
		}
		if (IsInvitable)
		{
			num += 2;
		}
		if (StaticData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(StaticData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserPresenceData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.UserId.Length != 0)
		{
			UserId = other.UserId;
		}
		if (other.session_ != null)
		{
			if (session_ == null)
			{
				Session = new GameSessionData();
			}
			Session.MergeFrom(other.Session);
		}
		if (other.IsOnline)
		{
			IsOnline = other.IsOnline;
		}
		if (other.IsInvitable)
		{
			IsInvitable = other.IsInvitable;
		}
		if (other.StaticData.Length != 0)
		{
			StaticData = other.StaticData;
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
				UserId = input.ReadString();
				break;
			case 18u:
				if (session_ == null)
				{
					Session = new GameSessionData();
				}
				input.ReadMessage(Session);
				break;
			case 24u:
				IsOnline = input.ReadBool();
				break;
			case 32u:
				IsInvitable = input.ReadBool();
				break;
			case 42u:
				StaticData = input.ReadString();
				break;
			}
		}
	}
}
