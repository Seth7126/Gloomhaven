using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class TournamentSettings : IMessage<TournamentSettings>, IMessage, IEquatable<TournamentSettings>, IDeepCloneable<TournamentSettings>, IBufferMessage
{
	private static readonly MessageParser<TournamentSettings> _parser = new MessageParser<TournamentSettings>(() => new TournamentSettings());

	private UnknownFieldSet _unknownFields;

	public const int StartsOnFieldNumber = 1;

	private Timestamp startsOn_;

	public const int DurationFieldNumber = 2;

	private Duration duration_;

	public const int PropertiesFieldNumber = 3;

	private string properties_ = "";

	public const int PlayLimitFieldNumber = 4;

	private TournamentPlayLimit playLimit_;

	public const int ParticipantsLimitFieldNumber = 5;

	private int participantsLimit_;

	public const int CdnDataFieldNumber = 6;

	private string cdnData_ = "";

	public const int RewardsFieldNumber = 7;

	private static readonly FieldCodec<TournamentReward> _repeated_rewards_codec = FieldCodec.ForMessage(58u, TournamentReward.Parser);

	private readonly RepeatedField<TournamentReward> rewards_ = new RepeatedField<TournamentReward>();

	[DebuggerNonUserCode]
	public static MessageParser<TournamentSettings> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public Timestamp StartsOn
	{
		get
		{
			return startsOn_;
		}
		set
		{
			startsOn_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Duration Duration
	{
		get
		{
			return duration_;
		}
		set
		{
			duration_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Properties
	{
		get
		{
			return properties_;
		}
		set
		{
			properties_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TournamentPlayLimit PlayLimit
	{
		get
		{
			return playLimit_;
		}
		set
		{
			playLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int ParticipantsLimit
	{
		get
		{
			return participantsLimit_;
		}
		set
		{
			participantsLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string CdnData
	{
		get
		{
			return cdnData_;
		}
		set
		{
			cdnData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<TournamentReward> Rewards => rewards_;

	[DebuggerNonUserCode]
	public TournamentSettings()
	{
	}

	[DebuggerNonUserCode]
	public TournamentSettings(TournamentSettings other)
		: this()
	{
		startsOn_ = ((other.startsOn_ != null) ? other.startsOn_.Clone() : null);
		duration_ = ((other.duration_ != null) ? other.duration_.Clone() : null);
		properties_ = other.properties_;
		playLimit_ = ((other.playLimit_ != null) ? other.playLimit_.Clone() : null);
		participantsLimit_ = other.participantsLimit_;
		cdnData_ = other.cdnData_;
		rewards_ = other.rewards_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TournamentSettings Clone()
	{
		return new TournamentSettings(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TournamentSettings);
	}

	[DebuggerNonUserCode]
	public bool Equals(TournamentSettings other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(StartsOn, other.StartsOn))
		{
			return false;
		}
		if (!object.Equals(Duration, other.Duration))
		{
			return false;
		}
		if (Properties != other.Properties)
		{
			return false;
		}
		if (!object.Equals(PlayLimit, other.PlayLimit))
		{
			return false;
		}
		if (ParticipantsLimit != other.ParticipantsLimit)
		{
			return false;
		}
		if (CdnData != other.CdnData)
		{
			return false;
		}
		if (!rewards_.Equals(other.rewards_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (startsOn_ != null)
		{
			num ^= StartsOn.GetHashCode();
		}
		if (duration_ != null)
		{
			num ^= Duration.GetHashCode();
		}
		if (Properties.Length != 0)
		{
			num ^= Properties.GetHashCode();
		}
		if (playLimit_ != null)
		{
			num ^= PlayLimit.GetHashCode();
		}
		if (ParticipantsLimit != 0)
		{
			num ^= ParticipantsLimit.GetHashCode();
		}
		if (CdnData.Length != 0)
		{
			num ^= CdnData.GetHashCode();
		}
		num ^= rewards_.GetHashCode();
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
		if (startsOn_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(StartsOn);
		}
		if (duration_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Duration);
		}
		if (Properties.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(Properties);
		}
		if (playLimit_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(PlayLimit);
		}
		if (ParticipantsLimit != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(ParticipantsLimit);
		}
		if (CdnData.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(CdnData);
		}
		rewards_.WriteTo(ref output, _repeated_rewards_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (startsOn_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(StartsOn);
		}
		if (duration_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Duration);
		}
		if (Properties.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Properties);
		}
		if (playLimit_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(PlayLimit);
		}
		if (ParticipantsLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ParticipantsLimit);
		}
		if (CdnData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CdnData);
		}
		num += rewards_.CalculateSize(_repeated_rewards_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TournamentSettings other)
	{
		if (other == null)
		{
			return;
		}
		if (other.startsOn_ != null)
		{
			if (startsOn_ == null)
			{
				StartsOn = new Timestamp();
			}
			StartsOn.MergeFrom(other.StartsOn);
		}
		if (other.duration_ != null)
		{
			if (duration_ == null)
			{
				Duration = new Duration();
			}
			Duration.MergeFrom(other.Duration);
		}
		if (other.Properties.Length != 0)
		{
			Properties = other.Properties;
		}
		if (other.playLimit_ != null)
		{
			if (playLimit_ == null)
			{
				PlayLimit = new TournamentPlayLimit();
			}
			PlayLimit.MergeFrom(other.PlayLimit);
		}
		if (other.ParticipantsLimit != 0)
		{
			ParticipantsLimit = other.ParticipantsLimit;
		}
		if (other.CdnData.Length != 0)
		{
			CdnData = other.CdnData;
		}
		rewards_.Add(other.rewards_);
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
				if (startsOn_ == null)
				{
					StartsOn = new Timestamp();
				}
				input.ReadMessage(StartsOn);
				break;
			case 18u:
				if (duration_ == null)
				{
					Duration = new Duration();
				}
				input.ReadMessage(Duration);
				break;
			case 26u:
				Properties = input.ReadString();
				break;
			case 34u:
				if (playLimit_ == null)
				{
					PlayLimit = new TournamentPlayLimit();
				}
				input.ReadMessage(PlayLimit);
				break;
			case 40u:
				ParticipantsLimit = input.ReadInt32();
				break;
			case 50u:
				CdnData = input.ReadString();
				break;
			case 58u:
				rewards_.AddEntriesFrom(ref input, _repeated_rewards_codec);
				break;
			}
		}
	}
}
