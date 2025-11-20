using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class MatchmakeSearchOptions : IMessage<MatchmakeSearchOptions>, IMessage, IEquatable<MatchmakeSearchOptions>, IDeepCloneable<MatchmakeSearchOptions>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeSearchOptions> _parser = new MessageParser<MatchmakeSearchOptions>(() => new MatchmakeSearchOptions());

	private UnknownFieldSet _unknownFields;

	public const int OptionsFieldNumber = 1;

	private MatchmakeSessionOptions options_;

	public const int VariantsFieldNumber = 2;

	private static readonly FieldCodec<QueueVariants> _repeated_variants_codec = FieldCodec.ForMessage(18u, QueueVariants.Parser);

	private readonly RepeatedField<QueueVariants> variants_ = new RepeatedField<QueueVariants>();

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeSearchOptions> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MatchmakeStatusReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public MatchmakeSessionOptions Options
	{
		get
		{
			return options_;
		}
		set
		{
			options_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<QueueVariants> Variants => variants_;

	[DebuggerNonUserCode]
	public MatchmakeSearchOptions()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeSearchOptions(MatchmakeSearchOptions other)
		: this()
	{
		options_ = ((other.options_ != null) ? other.options_.Clone() : null);
		variants_ = other.variants_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeSearchOptions Clone()
	{
		return new MatchmakeSearchOptions(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeSearchOptions);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeSearchOptions other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Options, other.Options))
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
		if (options_ != null)
		{
			num ^= Options.GetHashCode();
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
		if (options_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Options);
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
		if (options_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Options);
		}
		num += variants_.CalculateSize(_repeated_variants_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeSearchOptions other)
	{
		if (other == null)
		{
			return;
		}
		if (other.options_ != null)
		{
			if (options_ == null)
			{
				Options = new MatchmakeSessionOptions();
			}
			Options.MergeFrom(other.Options);
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
				if (options_ == null)
				{
					Options = new MatchmakeSessionOptions();
				}
				input.ReadMessage(Options);
				break;
			case 18u:
				variants_.AddEntriesFrom(ref input, _repeated_variants_codec);
				break;
			}
		}
	}
}
