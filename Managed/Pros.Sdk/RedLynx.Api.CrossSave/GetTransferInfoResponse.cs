using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Auth;

namespace RedLynx.Api.CrossSave;

public sealed class GetTransferInfoResponse : IMessage<GetTransferInfoResponse>, IMessage, IEquatable<GetTransferInfoResponse>, IDeepCloneable<GetTransferInfoResponse>, IBufferMessage
{
	private static readonly MessageParser<GetTransferInfoResponse> _parser = new MessageParser<GetTransferInfoResponse>(() => new GetTransferInfoResponse());

	private UnknownFieldSet _unknownFields;

	public const int TransferIdFieldNumber = 1;

	private string transferId_ = "";

	public const int ContentTypeFieldNumber = 2;

	private string contentType_ = "";

	public const int DataDescriptionJsonFieldNumber = 3;

	private string dataDescriptionJson_ = "";

	public const int PlatformFieldNumber = 4;

	private Platform platform_;

	public const int ProviderFieldNumber = 5;

	private Provider provider_;

	public const int SaveCreatedAtFieldNumber = 6;

	private Timestamp saveCreatedAt_;

	public const int TransferCreatedAtFieldNumber = 7;

	private Timestamp transferCreatedAt_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GetTransferInfoResponse> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => CrossSaveContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string TransferId
	{
		get
		{
			return transferId_;
		}
		set
		{
			transferId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ContentType
	{
		get
		{
			return contentType_;
		}
		set
		{
			contentType_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string DataDescriptionJson
	{
		get
		{
			return dataDescriptionJson_;
		}
		set
		{
			dataDescriptionJson_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Platform Platform
	{
		get
		{
			return platform_;
		}
		set
		{
			platform_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Provider Provider
	{
		get
		{
			return provider_;
		}
		set
		{
			provider_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Timestamp SaveCreatedAt
	{
		get
		{
			return saveCreatedAt_;
		}
		set
		{
			saveCreatedAt_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Timestamp TransferCreatedAt
	{
		get
		{
			return transferCreatedAt_;
		}
		set
		{
			transferCreatedAt_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetTransferInfoResponse()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetTransferInfoResponse(GetTransferInfoResponse other)
		: this()
	{
		transferId_ = other.transferId_;
		contentType_ = other.contentType_;
		dataDescriptionJson_ = other.dataDescriptionJson_;
		platform_ = other.platform_;
		provider_ = other.provider_;
		saveCreatedAt_ = ((other.saveCreatedAt_ != null) ? other.saveCreatedAt_.Clone() : null);
		transferCreatedAt_ = ((other.transferCreatedAt_ != null) ? other.transferCreatedAt_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetTransferInfoResponse Clone()
	{
		return new GetTransferInfoResponse(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GetTransferInfoResponse);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GetTransferInfoResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TransferId != other.TransferId)
		{
			return false;
		}
		if (ContentType != other.ContentType)
		{
			return false;
		}
		if (DataDescriptionJson != other.DataDescriptionJson)
		{
			return false;
		}
		if (Platform != other.Platform)
		{
			return false;
		}
		if (Provider != other.Provider)
		{
			return false;
		}
		if (!object.Equals(SaveCreatedAt, other.SaveCreatedAt))
		{
			return false;
		}
		if (!object.Equals(TransferCreatedAt, other.TransferCreatedAt))
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
		if (TransferId.Length != 0)
		{
			num ^= TransferId.GetHashCode();
		}
		if (ContentType.Length != 0)
		{
			num ^= ContentType.GetHashCode();
		}
		if (DataDescriptionJson.Length != 0)
		{
			num ^= DataDescriptionJson.GetHashCode();
		}
		if (Platform != Platform.Unknown)
		{
			num ^= Platform.GetHashCode();
		}
		if (Provider != Provider.Steam)
		{
			num ^= Provider.GetHashCode();
		}
		if (saveCreatedAt_ != null)
		{
			num ^= SaveCreatedAt.GetHashCode();
		}
		if (transferCreatedAt_ != null)
		{
			num ^= TransferCreatedAt.GetHashCode();
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
		if (TransferId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TransferId);
		}
		if (ContentType.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ContentType);
		}
		if (DataDescriptionJson.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(DataDescriptionJson);
		}
		if (Platform != Platform.Unknown)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)Platform);
		}
		if (Provider != Provider.Steam)
		{
			output.WriteRawTag(40);
			output.WriteEnum((int)Provider);
		}
		if (saveCreatedAt_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(SaveCreatedAt);
		}
		if (transferCreatedAt_ != null)
		{
			output.WriteRawTag(58);
			output.WriteMessage(TransferCreatedAt);
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
		if (TransferId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TransferId);
		}
		if (ContentType.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ContentType);
		}
		if (DataDescriptionJson.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DataDescriptionJson);
		}
		if (Platform != Platform.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Platform);
		}
		if (Provider != Provider.Steam)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Provider);
		}
		if (saveCreatedAt_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(SaveCreatedAt);
		}
		if (transferCreatedAt_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(TransferCreatedAt);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GetTransferInfoResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.TransferId.Length != 0)
		{
			TransferId = other.TransferId;
		}
		if (other.ContentType.Length != 0)
		{
			ContentType = other.ContentType;
		}
		if (other.DataDescriptionJson.Length != 0)
		{
			DataDescriptionJson = other.DataDescriptionJson;
		}
		if (other.Platform != Platform.Unknown)
		{
			Platform = other.Platform;
		}
		if (other.Provider != Provider.Steam)
		{
			Provider = other.Provider;
		}
		if (other.saveCreatedAt_ != null)
		{
			if (saveCreatedAt_ == null)
			{
				SaveCreatedAt = new Timestamp();
			}
			SaveCreatedAt.MergeFrom(other.SaveCreatedAt);
		}
		if (other.transferCreatedAt_ != null)
		{
			if (transferCreatedAt_ == null)
			{
				TransferCreatedAt = new Timestamp();
			}
			TransferCreatedAt.MergeFrom(other.TransferCreatedAt);
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
				TransferId = input.ReadString();
				break;
			case 18u:
				ContentType = input.ReadString();
				break;
			case 26u:
				DataDescriptionJson = input.ReadString();
				break;
			case 32u:
				Platform = (Platform)input.ReadEnum();
				break;
			case 40u:
				Provider = (Provider)input.ReadEnum();
				break;
			case 50u:
				if (saveCreatedAt_ == null)
				{
					SaveCreatedAt = new Timestamp();
				}
				input.ReadMessage(SaveCreatedAt);
				break;
			case 58u:
				if (transferCreatedAt_ == null)
				{
					TransferCreatedAt = new Timestamp();
				}
				input.ReadMessage(TransferCreatedAt);
				break;
			}
		}
	}
}
