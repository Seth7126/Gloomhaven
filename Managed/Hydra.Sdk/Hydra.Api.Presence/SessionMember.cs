using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class SessionMember : IMessage<SessionMember>, IMessage, IEquatable<SessionMember>, IDeepCloneable<SessionMember>, IBufferMessage
{
	private static readonly MessageParser<SessionMember> _parser = new MessageParser<SessionMember>(() => new SessionMember());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int IsOwnerFieldNumber = 2;

	private bool isOwner_;

	public const int DataFieldNumber = 3;

	private string data_ = "";

	public const int StaticDataFieldNumber = 4;

	private string staticData_ = "";

	public const int RatingFieldNumber = 5;

	private float rating_;

	public const int SortingIndexFieldNumber = 6;

	private int sortingIndex_;

	public const int GroupIdFieldNumber = 7;

	private string groupId_ = "";

	public const int TeamIdFieldNumber = 8;

	private int teamId_;

	[DebuggerNonUserCode]
	public static MessageParser<SessionMember> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MatchmakeStatusReflection.Descriptor.MessageTypes[7];

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
	public bool IsOwner
	{
		get
		{
			return isOwner_;
		}
		set
		{
			isOwner_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public float Rating
	{
		get
		{
			return rating_;
		}
		set
		{
			rating_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int SortingIndex
	{
		get
		{
			return sortingIndex_;
		}
		set
		{
			sortingIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string GroupId
	{
		get
		{
			return groupId_;
		}
		set
		{
			groupId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int TeamId
	{
		get
		{
			return teamId_;
		}
		set
		{
			teamId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SessionMember()
	{
	}

	[DebuggerNonUserCode]
	public SessionMember(SessionMember other)
		: this()
	{
		userId_ = other.userId_;
		isOwner_ = other.isOwner_;
		data_ = other.data_;
		staticData_ = other.staticData_;
		rating_ = other.rating_;
		sortingIndex_ = other.sortingIndex_;
		groupId_ = other.groupId_;
		teamId_ = other.teamId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SessionMember Clone()
	{
		return new SessionMember(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SessionMember);
	}

	[DebuggerNonUserCode]
	public bool Equals(SessionMember other)
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
		if (IsOwner != other.IsOwner)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (StaticData != other.StaticData)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Rating, other.Rating))
		{
			return false;
		}
		if (SortingIndex != other.SortingIndex)
		{
			return false;
		}
		if (GroupId != other.GroupId)
		{
			return false;
		}
		if (TeamId != other.TeamId)
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
		if (IsOwner)
		{
			num ^= IsOwner.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
		}
		if (StaticData.Length != 0)
		{
			num ^= StaticData.GetHashCode();
		}
		if (Rating != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Rating);
		}
		if (SortingIndex != 0)
		{
			num ^= SortingIndex.GetHashCode();
		}
		if (GroupId.Length != 0)
		{
			num ^= GroupId.GetHashCode();
		}
		if (TeamId != 0)
		{
			num ^= TeamId.GetHashCode();
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
		if (IsOwner)
		{
			output.WriteRawTag(16);
			output.WriteBool(IsOwner);
		}
		if (Data.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(Data);
		}
		if (StaticData.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(StaticData);
		}
		if (Rating != 0f)
		{
			output.WriteRawTag(45);
			output.WriteFloat(Rating);
		}
		if (SortingIndex != 0)
		{
			output.WriteRawTag(48);
			output.WriteInt32(SortingIndex);
		}
		if (GroupId.Length != 0)
		{
			output.WriteRawTag(58);
			output.WriteString(GroupId);
		}
		if (TeamId != 0)
		{
			output.WriteRawTag(64);
			output.WriteInt32(TeamId);
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
		if (IsOwner)
		{
			num += 2;
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Data);
		}
		if (StaticData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(StaticData);
		}
		if (Rating != 0f)
		{
			num += 5;
		}
		if (SortingIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SortingIndex);
		}
		if (GroupId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(GroupId);
		}
		if (TeamId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TeamId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SessionMember other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			if (other.IsOwner)
			{
				IsOwner = other.IsOwner;
			}
			if (other.Data.Length != 0)
			{
				Data = other.Data;
			}
			if (other.StaticData.Length != 0)
			{
				StaticData = other.StaticData;
			}
			if (other.Rating != 0f)
			{
				Rating = other.Rating;
			}
			if (other.SortingIndex != 0)
			{
				SortingIndex = other.SortingIndex;
			}
			if (other.GroupId.Length != 0)
			{
				GroupId = other.GroupId;
			}
			if (other.TeamId != 0)
			{
				TeamId = other.TeamId;
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
				IsOwner = input.ReadBool();
				break;
			case 26u:
				Data = input.ReadString();
				break;
			case 34u:
				StaticData = input.ReadString();
				break;
			case 45u:
				Rating = input.ReadFloat();
				break;
			case 48u:
				SortingIndex = input.ReadInt32();
				break;
			case 58u:
				GroupId = input.ReadString();
				break;
			case 64u:
				TeamId = input.ReadInt32();
				break;
			}
		}
	}
}
