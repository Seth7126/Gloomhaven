using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Nullable;
using Hydra.Api.Presence;

namespace Hydra.Api.Push.Presence;

public sealed class PresenceSessionGameData : IMessage<PresenceSessionGameData>, IMessage, IEquatable<PresenceSessionGameData>, IDeepCloneable<PresenceSessionGameData>, IBufferMessage
{
	private static readonly MessageParser<PresenceSessionGameData> _parser = new MessageParser<PresenceSessionGameData>(() => new PresenceSessionGameData());

	private UnknownFieldSet _unknownFields;

	public const int PlayListIdFieldNumber = 1;

	private NullableString playListId_;

	public const int VariantsFieldNumber = 2;

	private static readonly FieldCodec<GameVariant> _repeated_variants_codec = FieldCodec.ForMessage(18u, GameVariant.Parser);

	private readonly RepeatedField<GameVariant> variants_ = new RepeatedField<GameVariant>();

	public const int SettingsFieldNumber = 3;

	private MatchmakeSessionSettings settings_;

	public const int DataFieldNumber = 4;

	private NullableString data_;

	public const int MembersUpdateFieldNumber = 5;

	private static readonly FieldCodec<PresenceSessionMemberUpdate> _repeated_membersUpdate_codec = FieldCodec.ForMessage(42u, PresenceSessionMemberUpdate.Parser);

	private readonly RepeatedField<PresenceSessionMemberUpdate> membersUpdate_ = new RepeatedField<PresenceSessionMemberUpdate>();

	public const int SCManagedContextFieldNumber = 6;

	private string sCManagedContext_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<PresenceSessionGameData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public NullableString PlayListId
	{
		get
		{
			return playListId_;
		}
		set
		{
			playListId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<GameVariant> Variants => variants_;

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
	public NullableString Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<PresenceSessionMemberUpdate> MembersUpdate => membersUpdate_;

	[DebuggerNonUserCode]
	public string SCManagedContext
	{
		get
		{
			return sCManagedContext_;
		}
		set
		{
			sCManagedContext_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PresenceSessionGameData()
	{
	}

	[DebuggerNonUserCode]
	public PresenceSessionGameData(PresenceSessionGameData other)
		: this()
	{
		playListId_ = ((other.playListId_ != null) ? other.playListId_.Clone() : null);
		variants_ = other.variants_.Clone();
		settings_ = ((other.settings_ != null) ? other.settings_.Clone() : null);
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		membersUpdate_ = other.membersUpdate_.Clone();
		sCManagedContext_ = other.sCManagedContext_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PresenceSessionGameData Clone()
	{
		return new PresenceSessionGameData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PresenceSessionGameData);
	}

	[DebuggerNonUserCode]
	public bool Equals(PresenceSessionGameData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(PlayListId, other.PlayListId))
		{
			return false;
		}
		if (!variants_.Equals(other.variants_))
		{
			return false;
		}
		if (!object.Equals(Settings, other.Settings))
		{
			return false;
		}
		if (!object.Equals(Data, other.Data))
		{
			return false;
		}
		if (!membersUpdate_.Equals(other.membersUpdate_))
		{
			return false;
		}
		if (SCManagedContext != other.SCManagedContext)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (playListId_ != null)
		{
			num ^= PlayListId.GetHashCode();
		}
		num ^= variants_.GetHashCode();
		if (settings_ != null)
		{
			num ^= Settings.GetHashCode();
		}
		if (data_ != null)
		{
			num ^= Data.GetHashCode();
		}
		num ^= membersUpdate_.GetHashCode();
		if (SCManagedContext.Length != 0)
		{
			num ^= SCManagedContext.GetHashCode();
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
		if (playListId_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(PlayListId);
		}
		variants_.WriteTo(ref output, _repeated_variants_codec);
		if (settings_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Settings);
		}
		if (data_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Data);
		}
		membersUpdate_.WriteTo(ref output, _repeated_membersUpdate_codec);
		if (SCManagedContext.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(SCManagedContext);
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
		if (playListId_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(PlayListId);
		}
		num += variants_.CalculateSize(_repeated_variants_codec);
		if (settings_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Settings);
		}
		if (data_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Data);
		}
		num += membersUpdate_.CalculateSize(_repeated_membersUpdate_codec);
		if (SCManagedContext.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SCManagedContext);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PresenceSessionGameData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.playListId_ != null)
		{
			if (playListId_ == null)
			{
				PlayListId = new NullableString();
			}
			PlayListId.MergeFrom(other.PlayListId);
		}
		variants_.Add(other.variants_);
		if (other.settings_ != null)
		{
			if (settings_ == null)
			{
				Settings = new MatchmakeSessionSettings();
			}
			Settings.MergeFrom(other.Settings);
		}
		if (other.data_ != null)
		{
			if (data_ == null)
			{
				Data = new NullableString();
			}
			Data.MergeFrom(other.Data);
		}
		membersUpdate_.Add(other.membersUpdate_);
		if (other.SCManagedContext.Length != 0)
		{
			SCManagedContext = other.SCManagedContext;
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
				if (playListId_ == null)
				{
					PlayListId = new NullableString();
				}
				input.ReadMessage(PlayListId);
				break;
			case 18u:
				variants_.AddEntriesFrom(ref input, _repeated_variants_codec);
				break;
			case 26u:
				if (settings_ == null)
				{
					Settings = new MatchmakeSessionSettings();
				}
				input.ReadMessage(Settings);
				break;
			case 34u:
				if (data_ == null)
				{
					Data = new NullableString();
				}
				input.ReadMessage(Data);
				break;
			case 42u:
				membersUpdate_.AddEntriesFrom(ref input, _repeated_membersUpdate_codec);
				break;
			case 50u:
				SCManagedContext = input.ReadString();
				break;
			}
		}
	}
}
