using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace RedLynx.Api.Banner;

public sealed class GetBannersRequest : IMessage<GetBannersRequest>, IMessage, IEquatable<GetBannersRequest>, IDeepCloneable<GetBannersRequest>, IBufferMessage
{
	private static readonly MessageParser<GetBannersRequest> _parser = new MessageParser<GetBannersRequest>(() => new GetBannersRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int EntitlementsFieldNumber = 2;

	private static readonly FieldCodec<string> _repeated_entitlements_codec = FieldCodec.ForString(18u);

	private readonly RepeatedField<string> entitlements_ = new RepeatedField<string>();

	public const int ReferenceIdFieldNumber = 3;

	private string referenceId_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GetBannersRequest> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => BannerContractsReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<string> Entitlements => entitlements_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ReferenceId
	{
		get
		{
			return referenceId_;
		}
		set
		{
			referenceId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetBannersRequest()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetBannersRequest(GetBannersRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		entitlements_ = other.entitlements_.Clone();
		referenceId_ = other.referenceId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetBannersRequest Clone()
	{
		return new GetBannersRequest(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GetBannersRequest);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GetBannersRequest other)
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
		if (!entitlements_.Equals(other.entitlements_))
		{
			return false;
		}
		if (ReferenceId != other.ReferenceId)
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
		num ^= entitlements_.GetHashCode();
		if (ReferenceId.Length != 0)
		{
			num ^= ReferenceId.GetHashCode();
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
		entitlements_.WriteTo(ref output, _repeated_entitlements_codec);
		if (ReferenceId.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(ReferenceId);
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
		num += entitlements_.CalculateSize(_repeated_entitlements_codec);
		if (ReferenceId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ReferenceId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GetBannersRequest other)
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
		entitlements_.Add(other.entitlements_);
		if (other.ReferenceId.Length != 0)
		{
			ReferenceId = other.ReferenceId;
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
				entitlements_.AddEntriesFrom(ref input, _repeated_entitlements_codec);
				break;
			case 26u:
				ReferenceId = input.ReadString();
				break;
			}
		}
	}
}
