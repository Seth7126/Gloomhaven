using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace RedLynx.Api.Telemetry;

public sealed class SendTelemetryPackRequest : IMessage<SendTelemetryPackRequest>, IMessage, IEquatable<SendTelemetryPackRequest>, IDeepCloneable<SendTelemetryPackRequest>, IBufferMessage
{
	private static readonly MessageParser<SendTelemetryPackRequest> _parser = new MessageParser<SendTelemetryPackRequest>(() => new SendTelemetryPackRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int HeaderFieldNumber = 2;

	private TelemetryPackHeader header_;

	public const int EntriesFieldNumber = 3;

	private ByteString entries_ = ByteString.Empty;

	public const int DataFieldNumber = 4;

	private ByteString data_ = ByteString.Empty;

	public const int EventGenerationFieldNumber = 5;

	private int eventGeneration_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SendTelemetryPackRequest> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TelemetryContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserContext UserContext
	{
		get
		{
			return userContext_;
		}
		set
		{
			userContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TelemetryPackHeader Header
	{
		get
		{
			return header_;
		}
		set
		{
			header_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ByteString Entries
	{
		get
		{
			return entries_;
		}
		set
		{
			entries_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ByteString Data
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
	[GeneratedCode("protoc", null)]
	public int EventGeneration
	{
		get
		{
			return eventGeneration_;
		}
		set
		{
			eventGeneration_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SendTelemetryPackRequest()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SendTelemetryPackRequest(SendTelemetryPackRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		header_ = ((other.header_ != null) ? other.header_.Clone() : null);
		entries_ = other.entries_;
		data_ = other.data_;
		eventGeneration_ = other.eventGeneration_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SendTelemetryPackRequest Clone()
	{
		return new SendTelemetryPackRequest(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SendTelemetryPackRequest);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SendTelemetryPackRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		if (!object.Equals(Header, other.Header))
		{
			return false;
		}
		if (Entries != other.Entries)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (EventGeneration != other.EventGeneration)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		if (header_ != null)
		{
			num ^= Header.GetHashCode();
		}
		if (Entries.Length != 0)
		{
			num ^= Entries.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
		}
		if (EventGeneration != 0)
		{
			num ^= EventGeneration.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		if (header_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Header);
		}
		if (Entries.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteBytes(Entries);
		}
		if (Data.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteBytes(Data);
		}
		if (EventGeneration != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(EventGeneration);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (header_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Header);
		}
		if (Entries.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Entries);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Data);
		}
		if (EventGeneration != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EventGeneration);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SendTelemetryPackRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
		}
		if (other.header_ != null)
		{
			if (header_ == null)
			{
				Header = new TelemetryPackHeader();
			}
			Header.MergeFrom(other.Header);
		}
		if (other.Entries.Length != 0)
		{
			Entries = other.Entries;
		}
		if (other.Data.Length != 0)
		{
			Data = other.Data;
		}
		if (other.EventGeneration != 0)
		{
			EventGeneration = other.EventGeneration;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 18u:
				if (header_ == null)
				{
					Header = new TelemetryPackHeader();
				}
				input.ReadMessage(Header);
				break;
			case 26u:
				Entries = input.ReadBytes();
				break;
			case 34u:
				Data = input.ReadBytes();
				break;
			case 40u:
				EventGeneration = input.ReadInt32();
				break;
			}
		}
	}
}
