using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class SetTournamentVisibilityRequest : IMessage<SetTournamentVisibilityRequest>, IMessage, IEquatable<SetTournamentVisibilityRequest>, IDeepCloneable<SetTournamentVisibilityRequest>, IBufferMessage
{
	private static readonly MessageParser<SetTournamentVisibilityRequest> _parser = new MessageParser<SetTournamentVisibilityRequest>(() => new SetTournamentVisibilityRequest());

	private UnknownFieldSet _unknownFields;

	public const int TournamentIdFieldNumber = 1;

	private string tournamentId_ = "";

	public const int IsVisibleFieldNumber = 2;

	private bool isVisible_;

	[DebuggerNonUserCode]
	public static MessageParser<SetTournamentVisibilityRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[8];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public bool IsVisible
	{
		get
		{
			return isVisible_;
		}
		set
		{
			isVisible_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SetTournamentVisibilityRequest()
	{
	}

	[DebuggerNonUserCode]
	public SetTournamentVisibilityRequest(SetTournamentVisibilityRequest other)
		: this()
	{
		tournamentId_ = other.tournamentId_;
		isVisible_ = other.isVisible_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SetTournamentVisibilityRequest Clone()
	{
		return new SetTournamentVisibilityRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SetTournamentVisibilityRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SetTournamentVisibilityRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TournamentId != other.TournamentId)
		{
			return false;
		}
		if (IsVisible != other.IsVisible)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TournamentId.Length != 0)
		{
			num ^= TournamentId.GetHashCode();
		}
		if (IsVisible)
		{
			num ^= IsVisible.GetHashCode();
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
		if (TournamentId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TournamentId);
		}
		if (IsVisible)
		{
			output.WriteRawTag(16);
			output.WriteBool(IsVisible);
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
		if (TournamentId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TournamentId);
		}
		if (IsVisible)
		{
			num += 2;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SetTournamentVisibilityRequest other)
	{
		if (other != null)
		{
			if (other.TournamentId.Length != 0)
			{
				TournamentId = other.TournamentId;
			}
			if (other.IsVisible)
			{
				IsVisible = other.IsVisible;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				TournamentId = input.ReadString();
				break;
			case 16u:
				IsVisible = input.ReadBool();
				break;
			}
		}
	}
}
