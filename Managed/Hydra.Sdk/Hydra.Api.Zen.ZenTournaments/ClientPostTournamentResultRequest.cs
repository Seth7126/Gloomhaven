using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientPostTournamentResultRequest : IMessage<ClientPostTournamentResultRequest>, IMessage, IEquatable<ClientPostTournamentResultRequest>, IDeepCloneable<ClientPostTournamentResultRequest>, IBufferMessage
{
	private static readonly MessageParser<ClientPostTournamentResultRequest> _parser = new MessageParser<ClientPostTournamentResultRequest>(() => new ClientPostTournamentResultRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int TournamentIdFieldNumber = 2;

	private string tournamentId_ = "";

	public const int ValueFieldNumber = 3;

	private double value_;

	public const int PasswordFieldNumber = 4;

	private string password_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ClientPostTournamentResultRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[31];

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
	public string TournamentId
	{
		get
		{
			return tournamentId_;
		}
		set
		{
			tournamentId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public double Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = value;
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
	public ClientPostTournamentResultRequest()
	{
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentResultRequest(ClientPostTournamentResultRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		tournamentId_ = other.tournamentId_;
		value_ = other.value_;
		password_ = other.password_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentResultRequest Clone()
	{
		return new ClientPostTournamentResultRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientPostTournamentResultRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientPostTournamentResultRequest other)
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
		if (TournamentId != other.TournamentId)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Value, other.Value))
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
		if (TournamentId.Length != 0)
		{
			num ^= TournamentId.GetHashCode();
		}
		if (Value != 0.0)
		{
			num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Value);
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
		if (TournamentId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(TournamentId);
		}
		if (Value != 0.0)
		{
			output.WriteRawTag(25);
			output.WriteDouble(Value);
		}
		if (Password.Length != 0)
		{
			output.WriteRawTag(34);
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
		if (TournamentId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TournamentId);
		}
		if (Value != 0.0)
		{
			num += 9;
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
	public void MergeFrom(ClientPostTournamentResultRequest other)
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
		if (other.TournamentId.Length != 0)
		{
			TournamentId = other.TournamentId;
		}
		if (other.Value != 0.0)
		{
			Value = other.Value;
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
				TournamentId = input.ReadString();
				break;
			case 25u:
				Value = input.ReadDouble();
				break;
			case 34u:
				Password = input.ReadString();
				break;
			}
		}
	}
}
