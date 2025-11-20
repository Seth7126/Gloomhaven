using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Presence;

namespace Hydra.Api.Push.Presence;

public sealed class PresenceSessionUpdate : IMessage<PresenceSessionUpdate>, IMessage, IEquatable<PresenceSessionUpdate>, IDeepCloneable<PresenceSessionUpdate>, IBufferMessage
{
	private static readonly MessageParser<PresenceSessionUpdate> _parser = new MessageParser<PresenceSessionUpdate>(() => new PresenceSessionUpdate());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private GameSessionId id_;

	public const int StateFieldNumber = 2;

	private MatchmakeState state_ = MatchmakeState.None;

	public const int QueueDataFieldNumber = 3;

	private PresenceSessionQueueData queueData_;

	public const int GameDataFieldNumber = 4;

	private PresenceSessionGameData gameData_;

	[DebuggerNonUserCode]
	public static MessageParser<PresenceSessionUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public GameSessionId Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = value;
		}
	}

	[DebuggerNonUserCode]
	public MatchmakeState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PresenceSessionQueueData QueueData
	{
		get
		{
			return queueData_;
		}
		set
		{
			queueData_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PresenceSessionGameData GameData
	{
		get
		{
			return gameData_;
		}
		set
		{
			gameData_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PresenceSessionUpdate()
	{
	}

	[DebuggerNonUserCode]
	public PresenceSessionUpdate(PresenceSessionUpdate other)
		: this()
	{
		id_ = ((other.id_ != null) ? other.id_.Clone() : null);
		state_ = other.state_;
		queueData_ = ((other.queueData_ != null) ? other.queueData_.Clone() : null);
		gameData_ = ((other.gameData_ != null) ? other.gameData_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PresenceSessionUpdate Clone()
	{
		return new PresenceSessionUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PresenceSessionUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(PresenceSessionUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Id, other.Id))
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (!object.Equals(QueueData, other.QueueData))
		{
			return false;
		}
		if (!object.Equals(GameData, other.GameData))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (id_ != null)
		{
			num ^= Id.GetHashCode();
		}
		if (State != MatchmakeState.None)
		{
			num ^= State.GetHashCode();
		}
		if (queueData_ != null)
		{
			num ^= QueueData.GetHashCode();
		}
		if (gameData_ != null)
		{
			num ^= GameData.GetHashCode();
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
		if (id_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Id);
		}
		if (State != MatchmakeState.None)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)State);
		}
		if (queueData_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(QueueData);
		}
		if (gameData_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(GameData);
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
		if (id_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Id);
		}
		if (State != MatchmakeState.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (queueData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(QueueData);
		}
		if (gameData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(GameData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PresenceSessionUpdate other)
	{
		if (other == null)
		{
			return;
		}
		if (other.id_ != null)
		{
			if (id_ == null)
			{
				Id = new GameSessionId();
			}
			Id.MergeFrom(other.Id);
		}
		if (other.State != MatchmakeState.None)
		{
			State = other.State;
		}
		if (other.queueData_ != null)
		{
			if (queueData_ == null)
			{
				QueueData = new PresenceSessionQueueData();
			}
			QueueData.MergeFrom(other.QueueData);
		}
		if (other.gameData_ != null)
		{
			if (gameData_ == null)
			{
				GameData = new PresenceSessionGameData();
			}
			GameData.MergeFrom(other.GameData);
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
				if (id_ == null)
				{
					Id = new GameSessionId();
				}
				input.ReadMessage(Id);
				break;
			case 16u:
				State = (MatchmakeState)input.ReadEnum();
				break;
			case 26u:
				if (queueData_ == null)
				{
					QueueData = new PresenceSessionQueueData();
				}
				input.ReadMessage(QueueData);
				break;
			case 34u:
				if (gameData_ == null)
				{
					GameData = new PresenceSessionGameData();
				}
				input.ReadMessage(GameData);
				break;
			}
		}
	}
}
