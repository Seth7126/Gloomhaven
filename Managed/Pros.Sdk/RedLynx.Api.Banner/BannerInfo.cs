using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Banner;

public sealed class BannerInfo : IMessage<BannerInfo>, IMessage, IEquatable<BannerInfo>, IDeepCloneable<BannerInfo>, IBufferMessage
{
	private static readonly MessageParser<BannerInfo> _parser = new MessageParser<BannerInfo>(() => new BannerInfo());

	private UnknownFieldSet _unknownFields;

	public const int CampaignIdFieldNumber = 1;

	private string campaignId_ = "";

	public const int ImageUrlFieldNumber = 2;

	private string imageUrl_ = "";

	public const int RedirectUrlFieldNumber = 3;

	private string redirectUrl_ = "";

	public const int UrlTypeFieldNumber = 7;

	private BannerUrlType urlType_;

	public const int SeverityFieldNumber = 4;

	private BannerSeverity severity_;

	public const int BannerTypeFieldNumber = 5;

	private BannerType bannerType_;

	public const int MessagesFieldNumber = 6;

	private static readonly FieldCodec<BannerMessage> _repeated_messages_codec = FieldCodec.ForMessage(50u, BannerMessage.Parser);

	private readonly RepeatedField<BannerMessage> messages_ = new RepeatedField<BannerMessage>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BannerInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => BannerContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string CampaignId
	{
		get
		{
			return campaignId_;
		}
		set
		{
			campaignId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ImageUrl
	{
		get
		{
			return imageUrl_;
		}
		set
		{
			imageUrl_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string RedirectUrl
	{
		get
		{
			return redirectUrl_;
		}
		set
		{
			redirectUrl_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerUrlType UrlType
	{
		get
		{
			return urlType_;
		}
		set
		{
			urlType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerSeverity Severity
	{
		get
		{
			return severity_;
		}
		set
		{
			severity_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerType BannerType
	{
		get
		{
			return bannerType_;
		}
		set
		{
			bannerType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BannerMessage> Messages => messages_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerInfo(BannerInfo other)
		: this()
	{
		campaignId_ = other.campaignId_;
		imageUrl_ = other.imageUrl_;
		redirectUrl_ = other.redirectUrl_;
		urlType_ = other.urlType_;
		severity_ = other.severity_;
		bannerType_ = other.bannerType_;
		messages_ = other.messages_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerInfo Clone()
	{
		return new BannerInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BannerInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BannerInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CampaignId != other.CampaignId)
		{
			return false;
		}
		if (ImageUrl != other.ImageUrl)
		{
			return false;
		}
		if (RedirectUrl != other.RedirectUrl)
		{
			return false;
		}
		if (UrlType != other.UrlType)
		{
			return false;
		}
		if (Severity != other.Severity)
		{
			return false;
		}
		if (BannerType != other.BannerType)
		{
			return false;
		}
		if (!messages_.Equals(other.messages_))
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
		if (CampaignId.Length != 0)
		{
			num ^= CampaignId.GetHashCode();
		}
		if (ImageUrl.Length != 0)
		{
			num ^= ImageUrl.GetHashCode();
		}
		if (RedirectUrl.Length != 0)
		{
			num ^= RedirectUrl.GetHashCode();
		}
		if (UrlType != BannerUrlType.ExternalUrl)
		{
			num ^= UrlType.GetHashCode();
		}
		if (Severity != BannerSeverity.None)
		{
			num ^= Severity.GetHashCode();
		}
		if (BannerType != BannerType.Tips)
		{
			num ^= BannerType.GetHashCode();
		}
		num ^= messages_.GetHashCode();
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
		if (CampaignId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(CampaignId);
		}
		if (ImageUrl.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ImageUrl);
		}
		if (RedirectUrl.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(RedirectUrl);
		}
		if (Severity != BannerSeverity.None)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)Severity);
		}
		if (BannerType != BannerType.Tips)
		{
			output.WriteRawTag(40);
			output.WriteEnum((int)BannerType);
		}
		messages_.WriteTo(ref output, _repeated_messages_codec);
		if (UrlType != BannerUrlType.ExternalUrl)
		{
			output.WriteRawTag(56);
			output.WriteEnum((int)UrlType);
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
		if (CampaignId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CampaignId);
		}
		if (ImageUrl.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ImageUrl);
		}
		if (RedirectUrl.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(RedirectUrl);
		}
		if (UrlType != BannerUrlType.ExternalUrl)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)UrlType);
		}
		if (Severity != BannerSeverity.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Severity);
		}
		if (BannerType != BannerType.Tips)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BannerType);
		}
		num += messages_.CalculateSize(_repeated_messages_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BannerInfo other)
	{
		if (other != null)
		{
			if (other.CampaignId.Length != 0)
			{
				CampaignId = other.CampaignId;
			}
			if (other.ImageUrl.Length != 0)
			{
				ImageUrl = other.ImageUrl;
			}
			if (other.RedirectUrl.Length != 0)
			{
				RedirectUrl = other.RedirectUrl;
			}
			if (other.UrlType != BannerUrlType.ExternalUrl)
			{
				UrlType = other.UrlType;
			}
			if (other.Severity != BannerSeverity.None)
			{
				Severity = other.Severity;
			}
			if (other.BannerType != BannerType.Tips)
			{
				BannerType = other.BannerType;
			}
			messages_.Add(other.messages_);
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				CampaignId = input.ReadString();
				break;
			case 18u:
				ImageUrl = input.ReadString();
				break;
			case 26u:
				RedirectUrl = input.ReadString();
				break;
			case 32u:
				Severity = (BannerSeverity)input.ReadEnum();
				break;
			case 40u:
				BannerType = (BannerType)input.ReadEnum();
				break;
			case 50u:
				messages_.AddEntriesFrom(ref input, _repeated_messages_codec);
				break;
			case 56u:
				UrlType = (BannerUrlType)input.ReadEnum();
				break;
			}
		}
	}
}
