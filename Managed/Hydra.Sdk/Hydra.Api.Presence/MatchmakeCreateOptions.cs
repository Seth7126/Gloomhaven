using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class MatchmakeCreateOptions : IMessage<MatchmakeCreateOptions>, IMessage, IEquatable<MatchmakeCreateOptions>, IDeepCloneable<MatchmakeCreateOptions>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeCreateOptions> _parser = new MessageParser<MatchmakeCreateOptions>(() => new MatchmakeCreateOptions());

	private UnknownFieldSet _unknownFields;

	public const int OptionsFieldNumber = 1;

	private MatchmakeSessionOptions options_;

	public const int SettingsFieldNumber = 2;

	private MatchmakeSessionSettings settings_;

	public const int VariantsFieldNumber = 3;

	private static readonly FieldCodec<GameVariant> _repeated_variants_codec = FieldCodec.ForMessage(26u, GameVariant.Parser);

	private readonly RepeatedField<GameVariant> variants_ = new RepeatedField<GameVariant>();

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeCreateOptions> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MatchmakeStatusReflection.Descriptor.MessageTypes[4];

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
	public MatchmakeSessionSettings Settings
	{
		get
		{
			return settings_;
		}
		set
		{
			settings_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<GameVariant> Variants => variants_;

	[DebuggerNonUserCode]
	public MatchmakeCreateOptions()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeCreateOptions(MatchmakeCreateOptions other)
		: this()
	{
		options_ = ((other.options_ != null) ? other.options_.Clone() : null);
		settings_ = ((other.settings_ != null) ? other.settings_.Clone() : null);
		variants_ = other.variants_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeCreateOptions Clone()
	{
		return new MatchmakeCreateOptions(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeCreateOptions);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeCreateOptions other)
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
		if (!object.Equals(Settings, other.Settings))
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
		if (settings_ != null)
		{
			num ^= Settings.GetHashCode();
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
		if (settings_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Settings);
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
		if (settings_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Settings);
		}
		num += variants_.CalculateSize(_repeated_variants_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeCreateOptions other)
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
		if (other.settings_ != null)
		{
			if (settings_ == null)
			{
				Settings = new MatchmakeSessionSettings();
			}
			Settings.MergeFrom(other.Settings);
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
				if (settings_ == null)
				{
					Settings = new MatchmakeSessionSettings();
				}
				input.ReadMessage(Settings);
				break;
			case 26u:
				variants_.AddEntriesFrom(ref input, _repeated_variants_codec);
				break;
			}
		}
	}
}
