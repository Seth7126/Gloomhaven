using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientPostTournamentAttemptIncrementResponse : IMessage<ClientPostTournamentAttemptIncrementResponse>, IMessage, IEquatable<ClientPostTournamentAttemptIncrementResponse>, IDeepCloneable<ClientPostTournamentAttemptIncrementResponse>, IBufferMessage
{
	private static readonly MessageParser<ClientPostTournamentAttemptIncrementResponse> _parser = new MessageParser<ClientPostTournamentAttemptIncrementResponse>(() => new ClientPostTournamentAttemptIncrementResponse());

	private UnknownFieldSet _unknownFields;

	public const int TournamentFieldNumber = 1;

	private ClientTournamentData tournament_;

	public const int ResultTypeFieldNumber = 2;

	private PostTournamentResultType resultType_ = PostTournamentResultType.None;

	public const int AttemptKeyFieldNumber = 3;

	private string attemptKey_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ClientPostTournamentAttemptIncrementResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[33];

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
	public string AttemptKey
	{
		get
		{
			return attemptKey_;
		}
		set
		{
			attemptKey_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentAttemptIncrementResponse()
	{
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentAttemptIncrementResponse(ClientPostTournamentAttemptIncrementResponse other)
		: this()
	{
		tournament_ = ((other.tournament_ != null) ? other.tournament_.Clone() : null);
		resultType_ = other.resultType_;
		attemptKey_ = other.attemptKey_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentAttemptIncrementResponse Clone()
	{
		return new ClientPostTournamentAttemptIncrementResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientPostTournamentAttemptIncrementResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientPostTournamentAttemptIncrementResponse other)
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
		if (AttemptKey != other.AttemptKey)
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
		if (AttemptKey.Length != 0)
		{
			num ^= AttemptKey.GetHashCode();
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
		if (AttemptKey.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(AttemptKey);
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
		if (AttemptKey.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AttemptKey);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ClientPostTournamentAttemptIncrementResponse other)
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
		if (other.AttemptKey.Length != 0)
		{
			AttemptKey = other.AttemptKey;
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
			case 26u:
				AttemptKey = input.ReadString();
				break;
			}
		}
	}
}
