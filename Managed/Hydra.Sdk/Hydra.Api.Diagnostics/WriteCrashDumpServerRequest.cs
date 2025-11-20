using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Diagnostics;

public sealed class WriteCrashDumpServerRequest : IMessage<WriteCrashDumpServerRequest>, IMessage, IEquatable<WriteCrashDumpServerRequest>, IDeepCloneable<WriteCrashDumpServerRequest>, IBufferMessage
{
	private static readonly MessageParser<WriteCrashDumpServerRequest> _parser = new MessageParser<WriteCrashDumpServerRequest>(() => new WriteCrashDumpServerRequest());

	private UnknownFieldSet _unknownFields;

	public const int ServerContextFieldNumber = 1;

	private ServerContext serverContext_;

	public const int PropertiesFieldNumber = 2;

	private static readonly FieldCodec<DiagnosticsProperty> _repeated_properties_codec = FieldCodec.ForMessage(18u, DiagnosticsProperty.Parser);

	private readonly RepeatedField<DiagnosticsProperty> properties_ = new RepeatedField<DiagnosticsProperty>();

	public const int DumpHashFieldNumber = 3;

	private string dumpHash_ = "";

	public const int DataFieldNumber = 4;

	private ByteString data_ = ByteString.Empty;

	public const int DataTypeFieldNumber = 5;

	private DiagnosticsDataType dataType_ = DiagnosticsDataType.Binary;

	public const int VersionFieldNumber = 6;

	private string version_ = "";

	public const int SdkVersionFieldNumber = 7;

	private string sdkVersion_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<WriteCrashDumpServerRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => DiagnosticsContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ServerContext ServerContext
	{
		get
		{
			return serverContext_;
		}
		set
		{
			serverContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<DiagnosticsProperty> Properties => properties_;

	[DebuggerNonUserCode]
	public string DumpHash
	{
		get
		{
			return dumpHash_;
		}
		set
		{
			dumpHash_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
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
	public DiagnosticsDataType DataType
	{
		get
		{
			return dataType_;
		}
		set
		{
			dataType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string SdkVersion
	{
		get
		{
			return sdkVersion_;
		}
		set
		{
			sdkVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public WriteCrashDumpServerRequest()
	{
	}

	[DebuggerNonUserCode]
	public WriteCrashDumpServerRequest(WriteCrashDumpServerRequest other)
		: this()
	{
		serverContext_ = ((other.serverContext_ != null) ? other.serverContext_.Clone() : null);
		properties_ = other.properties_.Clone();
		dumpHash_ = other.dumpHash_;
		data_ = other.data_;
		dataType_ = other.dataType_;
		version_ = other.version_;
		sdkVersion_ = other.sdkVersion_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public WriteCrashDumpServerRequest Clone()
	{
		return new WriteCrashDumpServerRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as WriteCrashDumpServerRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(WriteCrashDumpServerRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ServerContext, other.ServerContext))
		{
			return false;
		}
		if (!properties_.Equals(other.properties_))
		{
			return false;
		}
		if (DumpHash != other.DumpHash)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (DataType != other.DataType)
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		if (SdkVersion != other.SdkVersion)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (serverContext_ != null)
		{
			num ^= ServerContext.GetHashCode();
		}
		num ^= properties_.GetHashCode();
		if (DumpHash.Length != 0)
		{
			num ^= DumpHash.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
		}
		if (DataType != DiagnosticsDataType.Binary)
		{
			num ^= DataType.GetHashCode();
		}
		if (Version.Length != 0)
		{
			num ^= Version.GetHashCode();
		}
		if (SdkVersion.Length != 0)
		{
			num ^= SdkVersion.GetHashCode();
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
		if (serverContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ServerContext);
		}
		properties_.WriteTo(ref output, _repeated_properties_codec);
		if (DumpHash.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(DumpHash);
		}
		if (Data.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteBytes(Data);
		}
		if (DataType != DiagnosticsDataType.Binary)
		{
			output.WriteRawTag(40);
			output.WriteEnum((int)DataType);
		}
		if (Version.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(Version);
		}
		if (SdkVersion.Length != 0)
		{
			output.WriteRawTag(58);
			output.WriteString(SdkVersion);
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
		if (serverContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ServerContext);
		}
		num += properties_.CalculateSize(_repeated_properties_codec);
		if (DumpHash.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DumpHash);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Data);
		}
		if (DataType != DiagnosticsDataType.Binary)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DataType);
		}
		if (Version.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Version);
		}
		if (SdkVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SdkVersion);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(WriteCrashDumpServerRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.serverContext_ != null)
		{
			if (serverContext_ == null)
			{
				ServerContext = new ServerContext();
			}
			ServerContext.MergeFrom(other.ServerContext);
		}
		properties_.Add(other.properties_);
		if (other.DumpHash.Length != 0)
		{
			DumpHash = other.DumpHash;
		}
		if (other.Data.Length != 0)
		{
			Data = other.Data;
		}
		if (other.DataType != DiagnosticsDataType.Binary)
		{
			DataType = other.DataType;
		}
		if (other.Version.Length != 0)
		{
			Version = other.Version;
		}
		if (other.SdkVersion.Length != 0)
		{
			SdkVersion = other.SdkVersion;
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
				if (serverContext_ == null)
				{
					ServerContext = new ServerContext();
				}
				input.ReadMessage(ServerContext);
				break;
			case 18u:
				properties_.AddEntriesFrom(ref input, _repeated_properties_codec);
				break;
			case 26u:
				DumpHash = input.ReadString();
				break;
			case 34u:
				Data = input.ReadBytes();
				break;
			case 40u:
				DataType = (DiagnosticsDataType)input.ReadEnum();
				break;
			case 50u:
				Version = input.ReadString();
				break;
			case 58u:
				SdkVersion = input.ReadString();
				break;
			}
		}
	}
}
