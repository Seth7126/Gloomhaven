using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.SessionControl;

namespace Hydra.Api.ServerManagerScheduler;

public sealed class RejectedPendingSession : IMessage<RejectedPendingSession>, IMessage, IEquatable<RejectedPendingSession>, IDeepCloneable<RejectedPendingSession>, IBufferMessage
{
	private static readonly MessageParser<RejectedPendingSession> _parser = new MessageParser<RejectedPendingSession>(() => new RejectedPendingSession());

	private UnknownFieldSet _unknownFields;

	public const int DsmIdFieldNumber = 1;

	private string dsmId_ = "";

	public const int DataCenterIdFieldNumber = 2;

	private string dataCenterId_ = "";

	public const int SessionFieldNumber = 3;

	private PendingSession session_;

	public const int ReasonFieldNumber = 4;

	private SessionRejectReason reason_ = SessionRejectReason.Unknown;

	[DebuggerNonUserCode]
	public static MessageParser<RejectedPendingSession> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RejectedPendingSessionReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string DsmId
	{
		get
		{
			return dsmId_;
		}
		set
		{
			dsmId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string DataCenterId
	{
		get
		{
			return dataCenterId_;
		}
		set
		{
			dataCenterId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PendingSession Session
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
	public SessionRejectReason Reason
	{
		get
		{
			return reason_;
		}
		set
		{
			reason_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RejectedPendingSession()
	{
	}

	[DebuggerNonUserCode]
	public RejectedPendingSession(RejectedPendingSession other)
		: this()
	{
		dsmId_ = other.dsmId_;
		dataCenterId_ = other.dataCenterId_;
		session_ = ((other.session_ != null) ? other.session_.Clone() : null);
		reason_ = other.reason_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RejectedPendingSession Clone()
	{
		return new RejectedPendingSession(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RejectedPendingSession);
	}

	[DebuggerNonUserCode]
	public bool Equals(RejectedPendingSession other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DsmId != other.DsmId)
		{
			return false;
		}
		if (DataCenterId != other.DataCenterId)
		{
			return false;
		}
		if (!object.Equals(Session, other.Session))
		{
			return false;
		}
		if (Reason != other.Reason)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (DsmId.Length != 0)
		{
			num ^= DsmId.GetHashCode();
		}
		if (DataCenterId.Length != 0)
		{
			num ^= DataCenterId.GetHashCode();
		}
		if (session_ != null)
		{
			num ^= Session.GetHashCode();
		}
		if (Reason != SessionRejectReason.Unknown)
		{
			num ^= Reason.GetHashCode();
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
		if (DsmId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(DsmId);
		}
		if (DataCenterId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(DataCenterId);
		}
		if (session_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Session);
		}
		if (Reason != SessionRejectReason.Unknown)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)Reason);
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
		if (DsmId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DsmId);
		}
		if (DataCenterId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DataCenterId);
		}
		if (session_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Session);
		}
		if (Reason != SessionRejectReason.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Reason);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RejectedPendingSession other)
	{
		if (other == null)
		{
			return;
		}
		if (other.DsmId.Length != 0)
		{
			DsmId = other.DsmId;
		}
		if (other.DataCenterId.Length != 0)
		{
			DataCenterId = other.DataCenterId;
		}
		if (other.session_ != null)
		{
			if (session_ == null)
			{
				Session = new PendingSession();
			}
			Session.MergeFrom(other.Session);
		}
		if (other.Reason != SessionRejectReason.Unknown)
		{
			Reason = other.Reason;
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
				DsmId = input.ReadString();
				break;
			case 18u:
				DataCenterId = input.ReadString();
				break;
			case 26u:
				if (session_ == null)
				{
					Session = new PendingSession();
				}
				input.ReadMessage(Session);
				break;
			case 32u:
				Reason = (SessionRejectReason)input.ReadEnum();
				break;
			}
		}
	}
}
