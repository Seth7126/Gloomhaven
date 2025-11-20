using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Telemetry;

public sealed class TelemetryEventBaseEntry : IMessage<TelemetryEventBaseEntry>, IMessage, IEquatable<TelemetryEventBaseEntry>, IDeepCloneable<TelemetryEventBaseEntry>, IBufferMessage
{
	private static readonly MessageParser<TelemetryEventBaseEntry> _parser = new MessageParser<TelemetryEventBaseEntry>(() => new TelemetryEventBaseEntry());

	private UnknownFieldSet _unknownFields;

	public const int EventTypeFieldNumber = 1;

	private string eventType_ = "";

	public const int EventUidFieldNumber = 2;

	private string eventUid_ = "";

	public const int VersionFieldNumber = 3;

	private int version_;

	public const int JsonParamsFieldNumber = 4;

	private string jsonParams_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<TelemetryEventBaseEntry> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => TelemetryContractsReflection.Descriptor.MessageTypes[8];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string EventType
	{
		get
		{
			return eventType_;
		}
		set
		{
			eventType_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string EventUid
	{
		get
		{
			return eventUid_;
		}
		set
		{
			eventUid_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string JsonParams
	{
		get
		{
			return jsonParams_;
		}
		set
		{
			jsonParams_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TelemetryEventBaseEntry()
	{
	}

	[DebuggerNonUserCode]
	public TelemetryEventBaseEntry(TelemetryEventBaseEntry other)
		: this()
	{
		eventType_ = other.eventType_;
		eventUid_ = other.eventUid_;
		version_ = other.version_;
		jsonParams_ = other.jsonParams_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TelemetryEventBaseEntry Clone()
	{
		return new TelemetryEventBaseEntry(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TelemetryEventBaseEntry);
	}

	[DebuggerNonUserCode]
	public bool Equals(TelemetryEventBaseEntry other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (EventType != other.EventType)
		{
			return false;
		}
		if (EventUid != other.EventUid)
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		if (JsonParams != other.JsonParams)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (EventType.Length != 0)
		{
			num ^= EventType.GetHashCode();
		}
		if (EventUid.Length != 0)
		{
			num ^= EventUid.GetHashCode();
		}
		if (Version != 0)
		{
			num ^= Version.GetHashCode();
		}
		if (JsonParams.Length != 0)
		{
			num ^= JsonParams.GetHashCode();
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
		if (EventType.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(EventType);
		}
		if (EventUid.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(EventUid);
		}
		if (Version != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(Version);
		}
		if (JsonParams.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(JsonParams);
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
		if (EventType.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(EventType);
		}
		if (EventUid.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(EventUid);
		}
		if (Version != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Version);
		}
		if (JsonParams.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(JsonParams);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TelemetryEventBaseEntry other)
	{
		if (other != null)
		{
			if (other.EventType.Length != 0)
			{
				EventType = other.EventType;
			}
			if (other.EventUid.Length != 0)
			{
				EventUid = other.EventUid;
			}
			if (other.Version != 0)
			{
				Version = other.Version;
			}
			if (other.JsonParams.Length != 0)
			{
				JsonParams = other.JsonParams;
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
				EventType = input.ReadString();
				break;
			case 18u:
				EventUid = input.ReadString();
				break;
			case 24u:
				Version = input.ReadInt32();
				break;
			case 34u:
				JsonParams = input.ReadString();
				break;
			}
		}
	}
}
