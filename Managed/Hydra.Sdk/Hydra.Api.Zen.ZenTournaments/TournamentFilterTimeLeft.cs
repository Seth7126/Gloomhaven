using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class TournamentFilterTimeLeft : IMessage<TournamentFilterTimeLeft>, IMessage, IEquatable<TournamentFilterTimeLeft>, IDeepCloneable<TournamentFilterTimeLeft>, IBufferMessage
{
	private static readonly MessageParser<TournamentFilterTimeLeft> _parser = new MessageParser<TournamentFilterTimeLeft>(() => new TournamentFilterTimeLeft());

	private UnknownFieldSet _unknownFields;

	public const int FromFieldNumber = 1;

	private Duration from_;

	public const int ToFieldNumber = 2;

	private Duration to_;

	[DebuggerNonUserCode]
	public static MessageParser<TournamentFilterTimeLeft> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[20];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public Duration From
	{
		get
		{
			return from_;
		}
		set
		{
			from_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Duration To
	{
		get
		{
			return to_;
		}
		set
		{
			to_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentFilterTimeLeft()
	{
	}

	[DebuggerNonUserCode]
	public TournamentFilterTimeLeft(TournamentFilterTimeLeft other)
		: this()
	{
		from_ = ((other.from_ != null) ? other.from_.Clone() : null);
		to_ = ((other.to_ != null) ? other.to_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TournamentFilterTimeLeft Clone()
	{
		return new TournamentFilterTimeLeft(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TournamentFilterTimeLeft);
	}

	[DebuggerNonUserCode]
	public bool Equals(TournamentFilterTimeLeft other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(From, other.From))
		{
			return false;
		}
		if (!object.Equals(To, other.To))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (from_ != null)
		{
			num ^= From.GetHashCode();
		}
		if (to_ != null)
		{
			num ^= To.GetHashCode();
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
		if (from_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(From);
		}
		if (to_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(To);
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
		if (from_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(From);
		}
		if (to_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(To);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TournamentFilterTimeLeft other)
	{
		if (other == null)
		{
			return;
		}
		if (other.from_ != null)
		{
			if (from_ == null)
			{
				From = new Duration();
			}
			From.MergeFrom(other.From);
		}
		if (other.to_ != null)
		{
			if (to_ == null)
			{
				To = new Duration();
			}
			To.MergeFrom(other.To);
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
				if (from_ == null)
				{
					From = new Duration();
				}
				input.ReadMessage(From);
				break;
			case 18u:
				if (to_ == null)
				{
					To = new Duration();
				}
				input.ReadMessage(To);
				break;
			}
		}
	}
}
