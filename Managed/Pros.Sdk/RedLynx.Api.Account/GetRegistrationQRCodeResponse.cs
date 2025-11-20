using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Account;

public sealed class GetRegistrationQRCodeResponse : IMessage<GetRegistrationQRCodeResponse>, IMessage, IEquatable<GetRegistrationQRCodeResponse>, IDeepCloneable<GetRegistrationQRCodeResponse>, IBufferMessage
{
	private static readonly MessageParser<GetRegistrationQRCodeResponse> _parser = new MessageParser<GetRegistrationQRCodeResponse>(() => new GetRegistrationQRCodeResponse());

	private UnknownFieldSet _unknownFields;

	public const int ConfigFieldNumber = 7;

	private QRCodeConfig config_;

	public const int RegistrationLinkFieldNumber = 1;

	private string registrationLink_ = "";

	public const int QrCodePngFieldNumber = 2;

	private ByteString qrCodePng_ = ByteString.Empty;

	public const int CodeFieldNumber = 3;

	private string code_ = "";

	public const int LinkStatusFieldNumber = 4;

	private LinkStatus linkStatus_;

	public const int ExpirationInSecondsFieldNumber = 5;

	private int expirationInSeconds_;

	public const int RetrievalIntervalsFieldNumber = 6;

	private static readonly FieldCodec<RetrievalInterval> _repeated_retrievalIntervals_codec = FieldCodec.ForMessage(50u, RetrievalInterval.Parser);

	private readonly RepeatedField<RetrievalInterval> retrievalIntervals_ = new RepeatedField<RetrievalInterval>();

	public const int AccountInfoFieldNumber = 8;

	private AccountInfo accountInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GetRegistrationQRCodeResponse> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => AccountContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRCodeConfig Config
	{
		get
		{
			return config_;
		}
		set
		{
			config_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string RegistrationLink
	{
		get
		{
			return registrationLink_;
		}
		set
		{
			registrationLink_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ByteString QrCodePng
	{
		get
		{
			return qrCodePng_;
		}
		set
		{
			qrCodePng_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Code
	{
		get
		{
			return code_;
		}
		set
		{
			code_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LinkStatus LinkStatus
	{
		get
		{
			return linkStatus_;
		}
		set
		{
			linkStatus_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ExpirationInSeconds
	{
		get
		{
			return expirationInSeconds_;
		}
		set
		{
			expirationInSeconds_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RetrievalInterval> RetrievalIntervals => retrievalIntervals_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountInfo AccountInfo
	{
		get
		{
			return accountInfo_;
		}
		set
		{
			accountInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetRegistrationQRCodeResponse()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetRegistrationQRCodeResponse(GetRegistrationQRCodeResponse other)
		: this()
	{
		config_ = ((other.config_ != null) ? other.config_.Clone() : null);
		registrationLink_ = other.registrationLink_;
		qrCodePng_ = other.qrCodePng_;
		code_ = other.code_;
		linkStatus_ = other.linkStatus_;
		expirationInSeconds_ = other.expirationInSeconds_;
		retrievalIntervals_ = other.retrievalIntervals_.Clone();
		accountInfo_ = ((other.accountInfo_ != null) ? other.accountInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetRegistrationQRCodeResponse Clone()
	{
		return new GetRegistrationQRCodeResponse(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GetRegistrationQRCodeResponse);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GetRegistrationQRCodeResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Config, other.Config))
		{
			return false;
		}
		if (RegistrationLink != other.RegistrationLink)
		{
			return false;
		}
		if (QrCodePng != other.QrCodePng)
		{
			return false;
		}
		if (Code != other.Code)
		{
			return false;
		}
		if (LinkStatus != other.LinkStatus)
		{
			return false;
		}
		if (ExpirationInSeconds != other.ExpirationInSeconds)
		{
			return false;
		}
		if (!retrievalIntervals_.Equals(other.retrievalIntervals_))
		{
			return false;
		}
		if (!object.Equals(AccountInfo, other.AccountInfo))
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
		if (config_ != null)
		{
			num ^= Config.GetHashCode();
		}
		if (RegistrationLink.Length != 0)
		{
			num ^= RegistrationLink.GetHashCode();
		}
		if (QrCodePng.Length != 0)
		{
			num ^= QrCodePng.GetHashCode();
		}
		if (Code.Length != 0)
		{
			num ^= Code.GetHashCode();
		}
		if (LinkStatus != LinkStatus.Unknown)
		{
			num ^= LinkStatus.GetHashCode();
		}
		if (ExpirationInSeconds != 0)
		{
			num ^= ExpirationInSeconds.GetHashCode();
		}
		num ^= retrievalIntervals_.GetHashCode();
		if (accountInfo_ != null)
		{
			num ^= AccountInfo.GetHashCode();
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
		if (RegistrationLink.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(RegistrationLink);
		}
		if (QrCodePng.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteBytes(QrCodePng);
		}
		if (Code.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(Code);
		}
		if (LinkStatus != LinkStatus.Unknown)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)LinkStatus);
		}
		if (ExpirationInSeconds != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(ExpirationInSeconds);
		}
		retrievalIntervals_.WriteTo(ref output, _repeated_retrievalIntervals_codec);
		if (config_ != null)
		{
			output.WriteRawTag(58);
			output.WriteMessage(Config);
		}
		if (accountInfo_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(AccountInfo);
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
		if (config_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Config);
		}
		if (RegistrationLink.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(RegistrationLink);
		}
		if (QrCodePng.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(QrCodePng);
		}
		if (Code.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Code);
		}
		if (LinkStatus != LinkStatus.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)LinkStatus);
		}
		if (ExpirationInSeconds != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ExpirationInSeconds);
		}
		num += retrievalIntervals_.CalculateSize(_repeated_retrievalIntervals_codec);
		if (accountInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(AccountInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GetRegistrationQRCodeResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.config_ != null)
		{
			if (config_ == null)
			{
				Config = new QRCodeConfig();
			}
			Config.MergeFrom(other.Config);
		}
		if (other.RegistrationLink.Length != 0)
		{
			RegistrationLink = other.RegistrationLink;
		}
		if (other.QrCodePng.Length != 0)
		{
			QrCodePng = other.QrCodePng;
		}
		if (other.Code.Length != 0)
		{
			Code = other.Code;
		}
		if (other.LinkStatus != LinkStatus.Unknown)
		{
			LinkStatus = other.LinkStatus;
		}
		if (other.ExpirationInSeconds != 0)
		{
			ExpirationInSeconds = other.ExpirationInSeconds;
		}
		retrievalIntervals_.Add(other.retrievalIntervals_);
		if (other.accountInfo_ != null)
		{
			if (accountInfo_ == null)
			{
				AccountInfo = new AccountInfo();
			}
			AccountInfo.MergeFrom(other.AccountInfo);
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
				RegistrationLink = input.ReadString();
				break;
			case 18u:
				QrCodePng = input.ReadBytes();
				break;
			case 26u:
				Code = input.ReadString();
				break;
			case 32u:
				LinkStatus = (LinkStatus)input.ReadEnum();
				break;
			case 40u:
				ExpirationInSeconds = input.ReadInt32();
				break;
			case 50u:
				retrievalIntervals_.AddEntriesFrom(ref input, _repeated_retrievalIntervals_codec);
				break;
			case 58u:
				if (config_ == null)
				{
					Config = new QRCodeConfig();
				}
				input.ReadMessage(Config);
				break;
			case 66u:
				if (accountInfo_ == null)
				{
					AccountInfo = new AccountInfo();
				}
				input.ReadMessage(AccountInfo);
				break;
			}
		}
	}
}
