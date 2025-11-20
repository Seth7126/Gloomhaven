using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class MatchmakeSessionSettings : IMessage<MatchmakeSessionSettings>, IMessage, IEquatable<MatchmakeSessionSettings>, IDeepCloneable<MatchmakeSessionSettings>, IBufferMessage
{
	private static readonly MessageParser<MatchmakeSessionSettings> _parser = new MessageParser<MatchmakeSessionSettings>(() => new MatchmakeSessionSettings());

	private UnknownFieldSet _unknownFields;

	public const int JipFieldNumber = 1;

	private MatchmakeJIPState jip_ = MatchmakeJIPState.Disabled;

	public const int MaxPlayersFieldNumber = 2;

	private int maxPlayers_;

	public const int JoinDelegationFieldNumber = 3;

	private MatchmakeJoinDelegation joinDelegation_ = MatchmakeJoinDelegation.Disabled;

	public const int AllowedUserIdsFieldNumber = 4;

	private static readonly FieldCodec<string> _repeated_allowedUserIds_codec = FieldCodec.ForString(34u);

	private readonly RepeatedField<string> allowedUserIds_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<MatchmakeSessionSettings> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MatchmakeStatusReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public MatchmakeJIPState Jip
	{
		get
		{
			return jip_;
		}
		set
		{
			jip_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int MaxPlayers
	{
		get
		{
			return maxPlayers_;
		}
		set
		{
			maxPlayers_ = value;
		}
	}

	[DebuggerNonUserCode]
	public MatchmakeJoinDelegation JoinDelegation
	{
		get
		{
			return joinDelegation_;
		}
		set
		{
			joinDelegation_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<string> AllowedUserIds => allowedUserIds_;

	[DebuggerNonUserCode]
	public MatchmakeSessionSettings()
	{
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionSettings(MatchmakeSessionSettings other)
		: this()
	{
		jip_ = other.jip_;
		maxPlayers_ = other.maxPlayers_;
		joinDelegation_ = other.joinDelegation_;
		allowedUserIds_ = other.allowedUserIds_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MatchmakeSessionSettings Clone()
	{
		return new MatchmakeSessionSettings(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MatchmakeSessionSettings);
	}

	[DebuggerNonUserCode]
	public bool Equals(MatchmakeSessionSettings other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Jip != other.Jip)
		{
			return false;
		}
		if (MaxPlayers != other.MaxPlayers)
		{
			return false;
		}
		if (JoinDelegation != other.JoinDelegation)
		{
			return false;
		}
		if (!allowedUserIds_.Equals(other.allowedUserIds_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Jip != MatchmakeJIPState.Disabled)
		{
			num ^= Jip.GetHashCode();
		}
		if (MaxPlayers != 0)
		{
			num ^= MaxPlayers.GetHashCode();
		}
		if (JoinDelegation != MatchmakeJoinDelegation.Disabled)
		{
			num ^= JoinDelegation.GetHashCode();
		}
		num ^= allowedUserIds_.GetHashCode();
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
		if (Jip != MatchmakeJIPState.Disabled)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)Jip);
		}
		if (MaxPlayers != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(MaxPlayers);
		}
		if (JoinDelegation != MatchmakeJoinDelegation.Disabled)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)JoinDelegation);
		}
		allowedUserIds_.WriteTo(ref output, _repeated_allowedUserIds_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (Jip != MatchmakeJIPState.Disabled)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Jip);
		}
		if (MaxPlayers != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxPlayers);
		}
		if (JoinDelegation != MatchmakeJoinDelegation.Disabled)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)JoinDelegation);
		}
		num += allowedUserIds_.CalculateSize(_repeated_allowedUserIds_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MatchmakeSessionSettings other)
	{
		if (other != null)
		{
			if (other.Jip != MatchmakeJIPState.Disabled)
			{
				Jip = other.Jip;
			}
			if (other.MaxPlayers != 0)
			{
				MaxPlayers = other.MaxPlayers;
			}
			if (other.JoinDelegation != MatchmakeJoinDelegation.Disabled)
			{
				JoinDelegation = other.JoinDelegation;
			}
			allowedUserIds_.Add(other.allowedUserIds_);
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
			case 8u:
				Jip = (MatchmakeJIPState)input.ReadEnum();
				break;
			case 16u:
				MaxPlayers = input.ReadInt32();
				break;
			case 24u:
				JoinDelegation = (MatchmakeJoinDelegation)input.ReadEnum();
				break;
			case 34u:
				allowedUserIds_.AddEntriesFrom(ref input, _repeated_allowedUserIds_codec);
				break;
			}
		}
	}
}
