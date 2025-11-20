using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class SessionMemberEventData : IMessage<SessionMemberEventData>, IMessage, IEquatable<SessionMemberEventData>, IDeepCloneable<SessionMemberEventData>, IBufferMessage
{
	private static readonly MessageParser<SessionMemberEventData> _parser = new MessageParser<SessionMemberEventData>(() => new SessionMemberEventData());

	private UnknownFieldSet _unknownFields;

	public const int ServerUserContextFieldNumber = 1;

	private ServerUserContext serverUserContext_;

	public const int BackendDataFieldNumber = 2;

	private SessionMemberBackendData backendData_;

	public const int EventTypeFieldNumber = 3;

	private SessionMemberEventType eventType_ = SessionMemberEventType.None;

	[DebuggerNonUserCode]
	public static MessageParser<SessionMemberEventData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MemberEventReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServerUserContext ServerUserContext
	{
		get
		{
			return serverUserContext_;
		}
		set
		{
			serverUserContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SessionMemberBackendData BackendData
	{
		get
		{
			return backendData_;
		}
		set
		{
			backendData_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SessionMemberEventType EventType
	{
		get
		{
			return eventType_;
		}
		set
		{
			eventType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SessionMemberEventData()
	{
	}

	[DebuggerNonUserCode]
	public SessionMemberEventData(SessionMemberEventData other)
		: this()
	{
		serverUserContext_ = ((other.serverUserContext_ != null) ? other.serverUserContext_.Clone() : null);
		backendData_ = ((other.backendData_ != null) ? other.backendData_.Clone() : null);
		eventType_ = other.eventType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SessionMemberEventData Clone()
	{
		return new SessionMemberEventData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SessionMemberEventData);
	}

	[DebuggerNonUserCode]
	public bool Equals(SessionMemberEventData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ServerUserContext, other.ServerUserContext))
		{
			return false;
		}
		if (!object.Equals(BackendData, other.BackendData))
		{
			return false;
		}
		if (EventType != other.EventType)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (serverUserContext_ != null)
		{
			num ^= ServerUserContext.GetHashCode();
		}
		if (backendData_ != null)
		{
			num ^= BackendData.GetHashCode();
		}
		if (EventType != SessionMemberEventType.None)
		{
			num ^= EventType.GetHashCode();
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
		if (serverUserContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ServerUserContext);
		}
		if (backendData_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(BackendData);
		}
		if (EventType != SessionMemberEventType.None)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)EventType);
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
		if (serverUserContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ServerUserContext);
		}
		if (backendData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(BackendData);
		}
		if (EventType != SessionMemberEventType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EventType);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SessionMemberEventData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.serverUserContext_ != null)
		{
			if (serverUserContext_ == null)
			{
				ServerUserContext = new ServerUserContext();
			}
			ServerUserContext.MergeFrom(other.ServerUserContext);
		}
		if (other.backendData_ != null)
		{
			if (backendData_ == null)
			{
				BackendData = new SessionMemberBackendData();
			}
			BackendData.MergeFrom(other.BackendData);
		}
		if (other.EventType != SessionMemberEventType.None)
		{
			EventType = other.EventType;
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
				if (serverUserContext_ == null)
				{
					ServerUserContext = new ServerUserContext();
				}
				input.ReadMessage(ServerUserContext);
				break;
			case 18u:
				if (backendData_ == null)
				{
					BackendData = new SessionMemberBackendData();
				}
				input.ReadMessage(BackendData);
				break;
			case 24u:
				EventType = (SessionMemberEventType)input.ReadEnum();
				break;
			}
		}
	}
}
