using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class GameSessionData : IMessage<GameSessionData>, IMessage, IEquatable<GameSessionData>, IDeepCloneable<GameSessionData>, IBufferMessage
{
	private static readonly MessageParser<GameSessionData> _parser = new MessageParser<GameSessionData>(() => new GameSessionData());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private string id_ = "";

	public const int SettingsFieldNumber = 2;

	private MatchmakeSessionSettings settings_;

	public const int VariantsFieldNumber = 3;

	private static readonly FieldCodec<GameVariant> _repeated_variants_codec = FieldCodec.ForMessage(26u, GameVariant.Parser);

	private readonly RepeatedField<GameVariant> variants_ = new RepeatedField<GameVariant>();

	public const int DataFieldNumber = 4;

	private string data_ = "";

	public const int SessionMembersFieldNumber = 5;

	private static readonly FieldCodec<SessionMember> _repeated_sessionMembers_codec = FieldCodec.ForMessage(42u, SessionMember.Parser);

	private readonly RepeatedField<SessionMember> sessionMembers_ = new RepeatedField<SessionMember>();

	[DebuggerNonUserCode]
	public static MessageParser<GameSessionData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MatchmakeStatusReflection.Descriptor.MessageTypes[8];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public string Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<SessionMember> SessionMembers => sessionMembers_;

	[DebuggerNonUserCode]
	public GameSessionData()
	{
	}

	[DebuggerNonUserCode]
	public GameSessionData(GameSessionData other)
		: this()
	{
		id_ = other.id_;
		settings_ = ((other.settings_ != null) ? other.settings_.Clone() : null);
		variants_ = other.variants_.Clone();
		data_ = other.data_;
		sessionMembers_ = other.sessionMembers_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GameSessionData Clone()
	{
		return new GameSessionData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GameSessionData);
	}

	[DebuggerNonUserCode]
	public bool Equals(GameSessionData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Id != other.Id)
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
		if (Data != other.Data)
		{
			return false;
		}
		if (!sessionMembers_.Equals(other.sessionMembers_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Id.Length != 0)
		{
			num ^= Id.GetHashCode();
		}
		if (settings_ != null)
		{
			num ^= Settings.GetHashCode();
		}
		num ^= variants_.GetHashCode();
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
		}
		num ^= sessionMembers_.GetHashCode();
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
		if (Id.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Id);
		}
		if (settings_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Settings);
		}
		variants_.WriteTo(ref output, _repeated_variants_codec);
		if (Data.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(Data);
		}
		sessionMembers_.WriteTo(ref output, _repeated_sessionMembers_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (Id.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Id);
		}
		if (settings_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Settings);
		}
		num += variants_.CalculateSize(_repeated_variants_codec);
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Data);
		}
		num += sessionMembers_.CalculateSize(_repeated_sessionMembers_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GameSessionData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id.Length != 0)
		{
			Id = other.Id;
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
		if (other.Data.Length != 0)
		{
			Data = other.Data;
		}
		sessionMembers_.Add(other.sessionMembers_);
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
				Id = input.ReadString();
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
			case 34u:
				Data = input.ReadString();
				break;
			case 42u:
				sessionMembers_.AddEntriesFrom(ref input, _repeated_sessionMembers_codec);
				break;
			}
		}
	}
}
