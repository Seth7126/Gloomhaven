using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Presence;

public sealed class MatchmakeSessionSetVariantsRequest : IMessage<MatchmakeSessionSetVariantsRequest>, IMessage, IEquatable<MatchmakeSessionSetVariantsRequest>, IDeepCloneable<MatchmakeSessionSetVariantsRequest>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeSessionSetVariantsRequest> _parser = new MessageParser<MatchmakeSessionSetVariantsRequest>(() => new MatchmakeSessionSetVariantsRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int VariantsFieldNumber = 2;

	private static readonly FieldCodec<GameVariant> _repeated_variants_codec = FieldCodec.ForMessage(18u, GameVariant.Parser);

	private readonly RepeatedField<GameVariant> variants_ = new RepeatedField<GameVariant>();

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeSessionSetVariantsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceServiceContractsReflection.Descriptor.MessageTypes[46];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext Context
	{
		get
		{
			return context_;
		}
		set
		{
			context_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<GameVariant> Variants => variants_;

	[DebuggerNonUserCode]
	public MatchmakeSessionSetVariantsRequest()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionSetVariantsRequest(MatchmakeSessionSetVariantsRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		variants_ = other.variants_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionSetVariantsRequest Clone()
	{
		return new MatchmakeSessionSetVariantsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeSessionSetVariantsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeSessionSetVariantsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Context, other.Context))
		{
			return false;
		}
		if (!variants_.Equals(other.variants_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (context_ != null)
		{
			num ^= Context.GetHashCode();
		}
		num ^= variants_.GetHashCode();
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
		if (context_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Context);
		}
		variants_.WriteTo(ref output, _repeated_variants_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (context_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Context);
		}
		num += variants_.CalculateSize(_repeated_variants_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeSessionSetVariantsRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.context_ != null)
		{
			if (context_ == null)
			{
				Context = new UserContext();
			}
			Context.MergeFrom(other.Context);
		}
		variants_.Add(other.variants_);
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
				if (context_ == null)
				{
					Context = new UserContext();
				}
				input.ReadMessage(Context);
				break;
			case 18u:
				variants_.AddEntriesFrom(ref input, _repeated_variants_codec);
				break;
			}
		}
	}
}
