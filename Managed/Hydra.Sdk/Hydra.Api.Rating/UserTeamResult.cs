using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public sealed class UserTeamResult : IMessage<UserTeamResult>, IMessage, IEquatable<UserTeamResult>, IDeepCloneable<UserTeamResult>, IBufferMessage
{
	private static readonly MessageParser<UserTeamResult> _parser = new MessageParser<UserTeamResult>(() => new UserTeamResult());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int UserPlaceInTeamFieldNumber = 2;

	private int userPlaceInTeam_;

	public const int TeamPlaceInGameFieldNumber = 3;

	private int teamPlaceInGame_;

	public const int CustomDataFieldNumber = 4;

	private string customData_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<UserTeamResult> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[4];

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
	public int UserPlaceInTeam
	{
		get
		{
			return userPlaceInTeam_;
		}
		set
		{
			userPlaceInTeam_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int TeamPlaceInGame
	{
		get
		{
			return teamPlaceInGame_;
		}
		set
		{
			teamPlaceInGame_ = value;
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
	public UserTeamResult()
	{
	}

	[DebuggerNonUserCode]
	public UserTeamResult(UserTeamResult other)
		: this()
	{
		userId_ = other.userId_;
		userPlaceInTeam_ = other.userPlaceInTeam_;
		teamPlaceInGame_ = other.teamPlaceInGame_;
		customData_ = other.customData_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserTeamResult Clone()
	{
		return new UserTeamResult(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserTeamResult);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserTeamResult other)
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
		if (UserPlaceInTeam != other.UserPlaceInTeam)
		{
			return false;
		}
		if (TeamPlaceInGame != other.TeamPlaceInGame)
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
		if (UserPlaceInTeam != 0)
		{
			num ^= UserPlaceInTeam.GetHashCode();
		}
		if (TeamPlaceInGame != 0)
		{
			num ^= TeamPlaceInGame.GetHashCode();
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
		if (UserPlaceInTeam != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(UserPlaceInTeam);
		}
		if (TeamPlaceInGame != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(TeamPlaceInGame);
		}
		if (CustomData.Length != 0)
		{
			output.WriteRawTag(34);
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
		if (UserPlaceInTeam != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(UserPlaceInTeam);
		}
		if (TeamPlaceInGame != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TeamPlaceInGame);
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
	public void MergeFrom(UserTeamResult other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			if (other.UserPlaceInTeam != 0)
			{
				UserPlaceInTeam = other.UserPlaceInTeam;
			}
			if (other.TeamPlaceInGame != 0)
			{
				TeamPlaceInGame = other.TeamPlaceInGame;
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
				UserPlaceInTeam = input.ReadInt32();
				break;
			case 24u:
				TeamPlaceInGame = input.ReadInt32();
				break;
			case 34u:
				CustomData = input.ReadString();
				break;
			}
		}
	}
}
