using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace RedLynx.Api.Entitlement;

public sealed class ConsumeEntitlementsRequest : IMessage<ConsumeEntitlementsRequest>, IMessage, IEquatable<ConsumeEntitlementsRequest>, IDeepCloneable<ConsumeEntitlementsRequest>, IBufferMessage
{
	private static readonly MessageParser<ConsumeEntitlementsRequest> _parser = new MessageParser<ConsumeEntitlementsRequest>(() => new ConsumeEntitlementsRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int EntitlementIdsFieldNumber = 2;

	private static readonly FieldCodec<string> _repeated_entitlementIds_codec = FieldCodec.ForString(18u);

	private readonly RepeatedField<string> entitlementIds_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ConsumeEntitlementsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => EntitlementContractsReflection.Descriptor.MessageTypes[2];

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
	public RepeatedField<string> EntitlementIds => entitlementIds_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ConsumeEntitlementsRequest()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ConsumeEntitlementsRequest(ConsumeEntitlementsRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		entitlementIds_ = other.entitlementIds_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ConsumeEntitlementsRequest Clone()
	{
		return new ConsumeEntitlementsRequest(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ConsumeEntitlementsRequest);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ConsumeEntitlementsRequest other)
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
		if (!entitlementIds_.Equals(other.entitlementIds_))
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
		num ^= entitlementIds_.GetHashCode();
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
		entitlementIds_.WriteTo(ref output, _repeated_entitlementIds_codec);
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
		num += entitlementIds_.CalculateSize(_repeated_entitlementIds_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ConsumeEntitlementsRequest other)
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
		entitlementIds_.Add(other.entitlementIds_);
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
				entitlementIds_.AddEntriesFrom(ref input, _repeated_entitlementIds_codec);
				break;
			}
		}
	}
}
