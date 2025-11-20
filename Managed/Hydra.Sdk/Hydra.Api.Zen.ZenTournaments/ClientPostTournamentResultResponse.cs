using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientPostTournamentResultResponse : IMessage<ClientPostTournamentResultResponse>, IMessage, IEquatable<ClientPostTournamentResultResponse>, IDeepCloneable<ClientPostTournamentResultResponse>, IBufferMessage
{
	private static readonly MessageParser<ClientPostTournamentResultResponse> _parser = new MessageParser<ClientPostTournamentResultResponse>(() => new ClientPostTournamentResultResponse());

	private UnknownFieldSet _unknownFields;

	public const int TournamentFieldNumber = 1;

	private ClientTournamentData tournament_;

	public const int ResultTypeFieldNumber = 2;

	private PostTournamentResultType resultType_ = PostTournamentResultType.None;

	[DebuggerNonUserCode]
	public static MessageParser<ClientPostTournamentResultResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[36];

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
	public PostTournamentResultType ResultType
	{
		get
		{
			return resultType_;
		}
		set
		{
			resultType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentResultResponse()
	{
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentResultResponse(ClientPostTournamentResultResponse other)
		: this()
	{
		tournament_ = ((other.tournament_ != null) ? other.tournament_.Clone() : null);
		resultType_ = other.resultType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentResultResponse Clone()
	{
		return new ClientPostTournamentResultResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientPostTournamentResultResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientPostTournamentResultResponse other)
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
		if (ResultType != other.ResultType)
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
		if (ResultType != PostTournamentResultType.None)
		{
			num ^= ResultType.GetHashCode();
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
		if (ResultType != PostTournamentResultType.None)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)ResultType);
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
		if (ResultType != PostTournamentResultType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ResultType);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ClientPostTournamentResultResponse other)
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
		if (other.ResultType != PostTournamentResultType.None)
		{
			ResultType = other.ResultType;
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
				if (tournament_ == null)
				{
					Tournament = new ClientTournamentData();
				}
				input.ReadMessage(Tournament);
				break;
			case 16u:
				ResultType = (PostTournamentResultType)input.ReadEnum();
				break;
			}
		}
	}
}
