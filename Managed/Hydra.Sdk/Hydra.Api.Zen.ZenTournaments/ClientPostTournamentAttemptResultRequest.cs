using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class ClientPostTournamentAttemptResultRequest : IMessage<ClientPostTournamentAttemptResultRequest>, IMessage, IEquatable<ClientPostTournamentAttemptResultRequest>, IDeepCloneable<ClientPostTournamentAttemptResultRequest>, IBufferMessage
{
	private static readonly MessageParser<ClientPostTournamentAttemptResultRequest> _parser = new MessageParser<ClientPostTournamentAttemptResultRequest>(() => new ClientPostTournamentAttemptResultRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int AttemptKeyFieldNumber = 2;

	private string attemptKey_ = "";

	public const int ValueFieldNumber = 3;

	private double value_;

	[DebuggerNonUserCode]
	public static MessageParser<ClientPostTournamentAttemptResultRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[34];

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
	public ClientPostTournamentAttemptResultRequest()
	{
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentAttemptResultRequest(ClientPostTournamentAttemptResultRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		attemptKey_ = other.attemptKey_;
		value_ = other.value_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ClientPostTournamentAttemptResultRequest Clone()
	{
		return new ClientPostTournamentAttemptResultRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ClientPostTournamentAttemptResultRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ClientPostTournamentAttemptResultRequest other)
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
		if (AttemptKey != other.AttemptKey)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Value, other.Value))
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
		if (AttemptKey.Length != 0)
		{
			num ^= AttemptKey.GetHashCode();
		}
		if (Value != 0.0)
		{
			num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Value);
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
		if (AttemptKey.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(AttemptKey);
		}
		if (Value != 0.0)
		{
			output.WriteRawTag(25);
			output.WriteDouble(Value);
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
		if (AttemptKey.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AttemptKey);
		}
		if (Value != 0.0)
		{
			num += 9;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ClientPostTournamentAttemptResultRequest other)
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
		if (other.AttemptKey.Length != 0)
		{
			AttemptKey = other.AttemptKey;
		}
		if (other.Value != 0.0)
		{
			Value = other.Value;
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
				AttemptKey = input.ReadString();
				break;
			case 25u:
				Value = input.ReadDouble();
				break;
			}
		}
	}
}
