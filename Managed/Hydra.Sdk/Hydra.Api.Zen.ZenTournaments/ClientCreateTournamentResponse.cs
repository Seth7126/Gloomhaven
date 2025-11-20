using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientCreateTournamentResponse : IMessage<ClientCreateTournamentResponse>, IMessage, IEquatable<ClientCreateTournamentResponse>, IDeepCloneable<ClientCreateTournamentResponse>, IBufferMessage
{
	private static readonly MessageParser<ClientCreateTournamentResponse> _parser = new MessageParser<ClientCreateTournamentResponse>(() => new ClientCreateTournamentResponse());

	private UnknownFieldSet _unknownFields;

	public const int TournamentFieldNumber = 1;

	private ClientTournamentData tournament_;

	[DebuggerNonUserCode]
	public static MessageParser<ClientCreateTournamentResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[28];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ClientTournamentData Tournament
	{
		get
		{
			return tournament_;
		}
		set
		{
			tournament_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ClientCreateTournamentResponse()
	{
	}

	[DebuggerNonUserCode]
	public ClientCreateTournamentResponse(ClientCreateTournamentResponse other)
		: this()
	{
		tournament_ = ((other.tournament_ != null) ? other.tournament_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientCreateTournamentResponse Clone()
	{
		return new ClientCreateTournamentResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientCreateTournamentResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientCreateTournamentResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Tournament, other.Tournament))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (tournament_ != null)
		{
			num ^= Tournament.GetHashCode();
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
		if (tournament_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Tournament);
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
		if (tournament_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Tournament);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ClientCreateTournamentResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.tournament_ != null)
		{
			if (tournament_ == null)
			{
				Tournament = new ClientTournamentData();
			}
			Tournament.MergeFrom(other.Tournament);
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				continue;
			}
			if (tournament_ == null)
			{
				Tournament = new ClientTournamentData();
			}
			input.ReadMessage(Tournament);
		}
	}
}
