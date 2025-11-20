using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Nullable;

namespace Hydra.Api.SessionControl;

public sealed class ServerBrowsingSessionData : IMessage<ServerBrowsingSessionData>, IMessage, IEquatable<ServerBrowsingSessionData>, IDeepCloneable<ServerBrowsingSessionData>, IBufferMessage
{
	private static readonly MessageParser<ServerBrowsingSessionData> _parser = new MessageParser<ServerBrowsingSessionData>(() => new ServerBrowsingSessionData());

	private UnknownFieldSet _unknownFields;

	public const int GameModeFieldNumber = 1;

	private NullableString gameMode_;

	public const int GameMapFieldNumber = 2;

	private NullableString gameMap_;

	public const int ServerNameFieldNumber = 3;

	private NullableString serverName_;

	public const int PasswordProtectedFieldNumber = 4;

	private NullableBool passwordProtected_;

	public const int MaxPlayerCountFieldNumber = 5;

	private NullableInt maxPlayerCount_;

	public const int TagsFieldNumber = 6;

	private HeartbeatServerTags tags_;

	public const int KeyValuesFieldNumber = 7;

	private HeartbeatServerKeyValue keyValues_;

	public const int MembersFieldNumber = 8;

	private HeartbeatServerMembers members_;

	[DebuggerNonUserCode]
	public static MessageParser<ServerBrowsingSessionData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[23];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public NullableString GameMode
	{
		get
		{
			return gameMode_;
		}
		set
		{
			gameMode_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableString GameMap
	{
		get
		{
			return gameMap_;
		}
		set
		{
			gameMap_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableString ServerName
	{
		get
		{
			return serverName_;
		}
		set
		{
			serverName_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableBool PasswordProtected
	{
		get
		{
			return passwordProtected_;
		}
		set
		{
			passwordProtected_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableInt MaxPlayerCount
	{
		get
		{
			return maxPlayerCount_;
		}
		set
		{
			maxPlayerCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	public HeartbeatServerTags Tags
	{
		get
		{
			return tags_;
		}
		set
		{
			tags_ = value;
		}
	}

	[DebuggerNonUserCode]
	public HeartbeatServerKeyValue KeyValues
	{
		get
		{
			return keyValues_;
		}
		set
		{
			keyValues_ = value;
		}
	}

	[DebuggerNonUserCode]
	public HeartbeatServerMembers Members
	{
		get
		{
			return members_;
		}
		set
		{
			members_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ServerBrowsingSessionData()
	{
	}

	[DebuggerNonUserCode]
	public ServerBrowsingSessionData(ServerBrowsingSessionData other)
		: this()
	{
		gameMode_ = ((other.gameMode_ != null) ? other.gameMode_.Clone() : null);
		gameMap_ = ((other.gameMap_ != null) ? other.gameMap_.Clone() : null);
		serverName_ = ((other.serverName_ != null) ? other.serverName_.Clone() : null);
		passwordProtected_ = ((other.passwordProtected_ != null) ? other.passwordProtected_.Clone() : null);
		maxPlayerCount_ = ((other.maxPlayerCount_ != null) ? other.maxPlayerCount_.Clone() : null);
		tags_ = ((other.tags_ != null) ? other.tags_.Clone() : null);
		keyValues_ = ((other.keyValues_ != null) ? other.keyValues_.Clone() : null);
		members_ = ((other.members_ != null) ? other.members_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServerBrowsingSessionData Clone()
	{
		return new ServerBrowsingSessionData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServerBrowsingSessionData);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServerBrowsingSessionData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(GameMode, other.GameMode))
		{
			return false;
		}
		if (!object.Equals(GameMap, other.GameMap))
		{
			return false;
		}
		if (!object.Equals(ServerName, other.ServerName))
		{
			return false;
		}
		if (!object.Equals(PasswordProtected, other.PasswordProtected))
		{
			return false;
		}
		if (!object.Equals(MaxPlayerCount, other.MaxPlayerCount))
		{
			return false;
		}
		if (!object.Equals(Tags, other.Tags))
		{
			return false;
		}
		if (!object.Equals(KeyValues, other.KeyValues))
		{
			return false;
		}
		if (!object.Equals(Members, other.Members))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (gameMode_ != null)
		{
			num ^= GameMode.GetHashCode();
		}
		if (gameMap_ != null)
		{
			num ^= GameMap.GetHashCode();
		}
		if (serverName_ != null)
		{
			num ^= ServerName.GetHashCode();
		}
		if (passwordProtected_ != null)
		{
			num ^= PasswordProtected.GetHashCode();
		}
		if (maxPlayerCount_ != null)
		{
			num ^= MaxPlayerCount.GetHashCode();
		}
		if (tags_ != null)
		{
			num ^= Tags.GetHashCode();
		}
		if (keyValues_ != null)
		{
			num ^= KeyValues.GetHashCode();
		}
		if (members_ != null)
		{
			num ^= Members.GetHashCode();
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
		if (gameMode_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(GameMode);
		}
		if (gameMap_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(GameMap);
		}
		if (serverName_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(ServerName);
		}
		if (passwordProtected_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(PasswordProtected);
		}
		if (maxPlayerCount_ != null)
		{
			output.WriteRawTag(42);
			output.WriteMessage(MaxPlayerCount);
		}
		if (tags_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(Tags);
		}
		if (keyValues_ != null)
		{
			output.WriteRawTag(58);
			output.WriteMessage(KeyValues);
		}
		if (members_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(Members);
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
		if (gameMode_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(GameMode);
		}
		if (gameMap_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(GameMap);
		}
		if (serverName_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ServerName);
		}
		if (passwordProtected_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(PasswordProtected);
		}
		if (maxPlayerCount_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(MaxPlayerCount);
		}
		if (tags_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Tags);
		}
		if (keyValues_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(KeyValues);
		}
		if (members_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Members);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ServerBrowsingSessionData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.gameMode_ != null)
		{
			if (gameMode_ == null)
			{
				GameMode = new NullableString();
			}
			GameMode.MergeFrom(other.GameMode);
		}
		if (other.gameMap_ != null)
		{
			if (gameMap_ == null)
			{
				GameMap = new NullableString();
			}
			GameMap.MergeFrom(other.GameMap);
		}
		if (other.serverName_ != null)
		{
			if (serverName_ == null)
			{
				ServerName = new NullableString();
			}
			ServerName.MergeFrom(other.ServerName);
		}
		if (other.passwordProtected_ != null)
		{
			if (passwordProtected_ == null)
			{
				PasswordProtected = new NullableBool();
			}
			PasswordProtected.MergeFrom(other.PasswordProtected);
		}
		if (other.maxPlayerCount_ != null)
		{
			if (maxPlayerCount_ == null)
			{
				MaxPlayerCount = new NullableInt();
			}
			MaxPlayerCount.MergeFrom(other.MaxPlayerCount);
		}
		if (other.tags_ != null)
		{
			if (tags_ == null)
			{
				Tags = new HeartbeatServerTags();
			}
			Tags.MergeFrom(other.Tags);
		}
		if (other.keyValues_ != null)
		{
			if (keyValues_ == null)
			{
				KeyValues = new HeartbeatServerKeyValue();
			}
			KeyValues.MergeFrom(other.KeyValues);
		}
		if (other.members_ != null)
		{
			if (members_ == null)
			{
				Members = new HeartbeatServerMembers();
			}
			Members.MergeFrom(other.Members);
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
				if (gameMode_ == null)
				{
					GameMode = new NullableString();
				}
				input.ReadMessage(GameMode);
				break;
			case 18u:
				if (gameMap_ == null)
				{
					GameMap = new NullableString();
				}
				input.ReadMessage(GameMap);
				break;
			case 26u:
				if (serverName_ == null)
				{
					ServerName = new NullableString();
				}
				input.ReadMessage(ServerName);
				break;
			case 34u:
				if (passwordProtected_ == null)
				{
					PasswordProtected = new NullableBool();
				}
				input.ReadMessage(PasswordProtected);
				break;
			case 42u:
				if (maxPlayerCount_ == null)
				{
					MaxPlayerCount = new NullableInt();
				}
				input.ReadMessage(MaxPlayerCount);
				break;
			case 50u:
				if (tags_ == null)
				{
					Tags = new HeartbeatServerTags();
				}
				input.ReadMessage(Tags);
				break;
			case 58u:
				if (keyValues_ == null)
				{
					KeyValues = new HeartbeatServerKeyValue();
				}
				input.ReadMessage(KeyValues);
				break;
			case 66u:
				if (members_ == null)
				{
					Members = new HeartbeatServerMembers();
				}
				input.ReadMessage(Members);
				break;
			}
		}
	}
}
