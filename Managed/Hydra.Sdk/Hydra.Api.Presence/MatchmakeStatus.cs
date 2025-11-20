using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class MatchmakeStatus : IMessage<MatchmakeStatus>, IMessage, IEquatable<MatchmakeStatus>, IDeepCloneable<MatchmakeStatus>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeStatus> _parser = new MessageParser<MatchmakeStatus>(() => new MatchmakeStatus());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private GameSessionId id_;

	public const int StateFieldNumber = 2;

	private MatchmakeState state_ = MatchmakeState.None;

	public const int SessionFieldNumber = 3;

	private GameSessionData session_;

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeStatus> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MatchmakeStatusReflection.Descriptor.MessageTypes[10];

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
	public MatchmakeStatus()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeStatus(MatchmakeStatus other)
		: this()
	{
		id_ = ((other.id_ != null) ? other.id_.Clone() : null);
		state_ = other.state_;
		session_ = ((other.session_ != null) ? other.session_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeStatus Clone()
	{
		return new MatchmakeStatus(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeStatus);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeStatus other)
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
		if (!object.Equals(Session, other.Session))
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
		if (session_ != null)
		{
			num ^= Session.GetHashCode();
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
		if (session_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Session);
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
		if (session_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Session);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeStatus other)
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
		if (other.session_ != null)
		{
			if (session_ == null)
			{
				Session = new GameSessionData();
			}
			Session.MergeFrom(other.Session);
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
				if (session_ == null)
				{
					Session = new GameSessionData();
				}
				input.ReadMessage(Session);
				break;
			}
		}
	}
}
