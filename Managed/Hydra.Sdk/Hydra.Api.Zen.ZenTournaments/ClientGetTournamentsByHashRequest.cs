using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientGetTournamentsByHashRequest : IMessage<ClientGetTournamentsByHashRequest>, IMessage, IEquatable<ClientGetTournamentsByHashRequest>, IDeepCloneable<ClientGetTournamentsByHashRequest>, IBufferMessage
{
	private static readonly MessageParser<ClientGetTournamentsByHashRequest> _parser = new MessageParser<ClientGetTournamentsByHashRequest>(() => new ClientGetTournamentsByHashRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int TournamentHashFieldNumber = 2;

	private string tournamentHash_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ClientGetTournamentsByHashRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[23];

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
	public string TournamentHash
	{
		get
		{
			return tournamentHash_;
		}
		set
		{
			tournamentHash_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ClientGetTournamentsByHashRequest()
	{
	}

	[DebuggerNonUserCode]
	public ClientGetTournamentsByHashRequest(ClientGetTournamentsByHashRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		tournamentHash_ = other.tournamentHash_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientGetTournamentsByHashRequest Clone()
	{
		return new ClientGetTournamentsByHashRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientGetTournamentsByHashRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientGetTournamentsByHashRequest other)
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
		if (TournamentHash != other.TournamentHash)
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
		if (TournamentHash.Length != 0)
		{
			num ^= TournamentHash.GetHashCode();
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
		if (TournamentHash.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(TournamentHash);
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
		if (TournamentHash.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TournamentHash);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ClientGetTournamentsByHashRequest other)
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
		if (other.TournamentHash.Length != 0)
		{
			TournamentHash = other.TournamentHash;
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
				TournamentHash = input.ReadString();
				break;
			}
		}
	}
}
