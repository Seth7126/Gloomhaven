using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Banner;

public sealed class GetBannersResponse : IMessage<GetBannersResponse>, IMessage, IEquatable<GetBannersResponse>, IDeepCloneable<GetBannersResponse>, IBufferMessage
{
	private static readonly MessageParser<GetBannersResponse> _parser = new MessageParser<GetBannersResponse>(() => new GetBannersResponse());

	private UnknownFieldSet _unknownFields;

	public const int ConfigFieldNumber = 1;

	private BannerConfig config_;

	public const int BannersFieldNumber = 2;

	private static readonly FieldCodec<BannerInfo> _repeated_banners_codec = FieldCodec.ForMessage(18u, BannerInfo.Parser);

	private readonly RepeatedField<BannerInfo> banners_ = new RepeatedField<BannerInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GetBannersResponse> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => BannerContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerConfig Config
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
	public RepeatedField<BannerInfo> Banners => banners_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetBannersResponse()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetBannersResponse(GetBannersResponse other)
		: this()
	{
		config_ = ((other.config_ != null) ? other.config_.Clone() : null);
		banners_ = other.banners_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetBannersResponse Clone()
	{
		return new GetBannersResponse(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GetBannersResponse);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GetBannersResponse other)
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
		if (!banners_.Equals(other.banners_))
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
		num ^= banners_.GetHashCode();
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
		if (config_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Config);
		}
		banners_.WriteTo(ref output, _repeated_banners_codec);
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
		num += banners_.CalculateSize(_repeated_banners_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GetBannersResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.config_ != null)
		{
			if (config_ == null)
			{
				Config = new BannerConfig();
			}
			Config.MergeFrom(other.Config);
		}
		banners_.Add(other.banners_);
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
				if (config_ == null)
				{
					Config = new BannerConfig();
				}
				input.ReadMessage(Config);
				break;
			case 18u:
				banners_.AddEntriesFrom(ref input, _repeated_banners_codec);
				break;
			}
		}
	}
}
