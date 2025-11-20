using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientCreateTournamentRequest : IMessage<ClientCreateTournamentRequest>, IMessage, IEquatable<ClientCreateTournamentRequest>, IDeepCloneable<ClientCreateTournamentRequest>, IBufferMessage
{
	private static readonly MessageParser<ClientCreateTournamentRequest> _parser = new MessageParser<ClientCreateTournamentRequest>(() => new ClientCreateTournamentRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int DurationFieldNumber = 2;

	private Duration duration_;

	public const int PropertiesFieldNumber = 3;

	private string properties_ = "";

	public const int PlayLimitFieldNumber = 4;

	private TournamentPlayLimit playLimit_;

	public const int ParticipantsLimitFieldNumber = 5;

	private int participantsLimit_;

	public const int PasswordFieldNumber = 6;

	private string password_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ClientCreateTournamentRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[27];

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
	public string Password
	{
		get
		{
			return password_;
		}
		set
		{
			password_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ClientCreateTournamentRequest()
	{
	}

	[DebuggerNonUserCode]
	public ClientCreateTournamentRequest(ClientCreateTournamentRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		duration_ = ((other.duration_ != null) ? other.duration_.Clone() : null);
		properties_ = other.properties_;
		playLimit_ = ((other.playLimit_ != null) ? other.playLimit_.Clone() : null);
		participantsLimit_ = other.participantsLimit_;
		password_ = other.password_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientCreateTournamentRequest Clone()
	{
		return new ClientCreateTournamentRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientCreateTournamentRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientCreateTournamentRequest other)
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
		if (Password != other.Password)
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
		if (Password.Length != 0)
		{
			num ^= Password.GetHashCode();
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
		if (context_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Context);
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
		if (Password.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(Password);
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
		if (context_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Context);
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
		if (Password.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Password);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ClientCreateTournamentRequest other)
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
		if (other.Password.Length != 0)
		{
			Password = other.Password;
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
				if (context_ == null)
				{
					Context = new UserContext();
				}
				input.ReadMessage(Context);
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
				Password = input.ReadString();
				break;
			}
		}
	}
}
