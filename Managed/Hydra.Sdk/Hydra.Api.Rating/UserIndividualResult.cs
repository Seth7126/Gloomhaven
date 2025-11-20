using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public sealed class UserIndividualResult : IMessage<UserIndividualResult>, IMessage, IEquatable<UserIndividualResult>, IDeepCloneable<UserIndividualResult>, IBufferMessage
{
	private static readonly MessageParser<UserIndividualResult> _parser = new MessageParser<UserIndividualResult>(() => new UserIndividualResult());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int UserPlaceInGameFieldNumber = 2;

	private int userPlaceInGame_;

	public const int CustomDataFieldNumber = 3;

	private string customData_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<UserIndividualResult> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[3];

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
	public int UserPlaceInGame
	{
		get
		{
			return userPlaceInGame_;
		}
		set
		{
			userPlaceInGame_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string CustomData
	{
		get
		{
			return customData_;
		}
		set
		{
			customData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public UserIndividualResult()
	{
	}

	[DebuggerNonUserCode]
	public UserIndividualResult(UserIndividualResult other)
		: this()
	{
		userId_ = other.userId_;
		userPlaceInGame_ = other.userPlaceInGame_;
		customData_ = other.customData_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserIndividualResult Clone()
	{
		return new UserIndividualResult(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserIndividualResult);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserIndividualResult other)
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
		if (UserPlaceInGame != other.UserPlaceInGame)
		{
			return false;
		}
		if (CustomData != other.CustomData)
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
		if (UserPlaceInGame != 0)
		{
			num ^= UserPlaceInGame.GetHashCode();
		}
		if (CustomData.Length != 0)
		{
			num ^= CustomData.GetHashCode();
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
		if (UserPlaceInGame != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(UserPlaceInGame);
		}
		if (CustomData.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(CustomData);
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
		if (UserPlaceInGame != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(UserPlaceInGame);
		}
		if (CustomData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CustomData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserIndividualResult other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			if (other.UserPlaceInGame != 0)
			{
				UserPlaceInGame = other.UserPlaceInGame;
			}
			if (other.CustomData.Length != 0)
			{
				CustomData = other.CustomData;
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
				UserPlaceInGame = input.ReadInt32();
				break;
			case 26u:
				CustomData = input.ReadString();
				break;
			}
		}
	}
}
