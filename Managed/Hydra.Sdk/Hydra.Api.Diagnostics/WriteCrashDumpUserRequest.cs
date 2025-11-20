using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Diagnostics;

public sealed class WriteCrashDumpUserRequest : IMessage<WriteCrashDumpUserRequest>, IMessage, IEquatable<WriteCrashDumpUserRequest>, IDeepCloneable<WriteCrashDumpUserRequest>, IBufferMessage
{
	private static readonly MessageParser<WriteCrashDumpUserRequest> _parser = new MessageParser<WriteCrashDumpUserRequest>(() => new WriteCrashDumpUserRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int PropertiesFieldNumber = 2;

	private static readonly FieldCodec<DiagnosticsProperty> _repeated_properties_codec = FieldCodec.ForMessage(18u, DiagnosticsProperty.Parser);

	private readonly RepeatedField<DiagnosticsProperty> properties_ = new RepeatedField<DiagnosticsProperty>();

	public const int DumpHashFieldNumber = 3;

	private string dumpHash_ = "";

	public const int DataFieldNumber = 4;

	private ByteString data_ = ByteString.Empty;

	public const int DataTypeFieldNumber = 5;

	private DiagnosticsDataType dataType_ = DiagnosticsDataType.Binary;

	public const int ProviderFieldNumber = 6;

	private string provider_ = "";

	public const int ClientVersionFieldNumber = 7;

	private string clientVersion_ = "";

	public const int SdkVersionFieldNumber = 8;

	private string sdkVersion_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<WriteCrashDumpUserRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => DiagnosticsContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
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
	public string Provider
	{
		get
		{
			return provider_;
		}
		set
		{
			provider_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ClientVersion
	{
		get
		{
			return clientVersion_;
		}
		set
		{
			clientVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public WriteCrashDumpUserRequest()
	{
	}

	[DebuggerNonUserCode]
	public WriteCrashDumpUserRequest(WriteCrashDumpUserRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		properties_ = other.properties_.Clone();
		dumpHash_ = other.dumpHash_;
		data_ = other.data_;
		dataType_ = other.dataType_;
		provider_ = other.provider_;
		clientVersion_ = other.clientVersion_;
		sdkVersion_ = other.sdkVersion_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public WriteCrashDumpUserRequest Clone()
	{
		return new WriteCrashDumpUserRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as WriteCrashDumpUserRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(WriteCrashDumpUserRequest other)
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
		if (Provider != other.Provider)
		{
			return false;
		}
		if (ClientVersion != other.ClientVersion)
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
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
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
		if (Provider.Length != 0)
		{
			num ^= Provider.GetHashCode();
		}
		if (ClientVersion.Length != 0)
		{
			num ^= ClientVersion.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
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
		if (Provider.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(Provider);
		}
		if (ClientVersion.Length != 0)
		{
			output.WriteRawTag(58);
			output.WriteString(ClientVersion);
		}
		if (SdkVersion.Length != 0)
		{
			output.WriteRawTag(66);
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
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
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
		if (Provider.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Provider);
		}
		if (ClientVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ClientVersion);
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
	public void MergeFrom(WriteCrashDumpUserRequest other)
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
		if (other.Provider.Length != 0)
		{
			Provider = other.Provider;
		}
		if (other.ClientVersion.Length != 0)
		{
			ClientVersion = other.ClientVersion;
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
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
				Provider = input.ReadString();
				break;
			case 58u:
				ClientVersion = input.ReadString();
				break;
			case 66u:
				SdkVersion = input.ReadString();
				break;
			}
		}
	}
}
