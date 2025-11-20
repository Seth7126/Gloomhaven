using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Nullable;
using Hydra.Api.Presence;

namespace Hydra.Api.Push.Presence;

public sealed class PresencePartyUpdate : IMessage<PresencePartyUpdate>, IMessage, IEquatable<PresencePartyUpdate>, IDeepCloneable<PresencePartyUpdate>, IBufferMessage
{
	private static readonly MessageParser<PresencePartyUpdate> _parser = new MessageParser<PresencePartyUpdate>(() => new PresencePartyUpdate());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 2;

	private PartyId id_;

	public const int DataFieldNumber = 3;

	private NullableString data_;

	public const int SettingsFieldNumber = 4;

	private PartySettings settings_;

	public const int MembersFieldNumber = 5;

	private static readonly FieldCodec<PresencePartyMemberUpdate> _repeated_members_codec = FieldCodec.ForMessage(42u, PresencePartyMemberUpdate.Parser);

	private readonly RepeatedField<PresencePartyMemberUpdate> members_ = new RepeatedField<PresencePartyMemberUpdate>();

	public const int JoinCodeFieldNumber = 6;

	private NullableString joinCode_;

	[DebuggerNonUserCode]
	public static MessageParser<PresencePartyUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[7];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PartyId Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableString Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PartySettings Settings
	{
		get
		{
			return settings_;
		}
		set
		{
			settings_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<PresencePartyMemberUpdate> Members => members_;

	[DebuggerNonUserCode]
	public NullableString JoinCode
	{
		get
		{
			return joinCode_;
		}
		set
		{
			joinCode_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PresencePartyUpdate()
	{
	}

	[DebuggerNonUserCode]
	public PresencePartyUpdate(PresencePartyUpdate other)
		: this()
	{
		id_ = ((other.id_ != null) ? other.id_.Clone() : null);
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		settings_ = ((other.settings_ != null) ? other.settings_.Clone() : null);
		members_ = other.members_.Clone();
		joinCode_ = ((other.joinCode_ != null) ? other.joinCode_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PresencePartyUpdate Clone()
	{
		return new PresencePartyUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PresencePartyUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(PresencePartyUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Id, other.Id))
		{
			return false;
		}
		if (!object.Equals(Data, other.Data))
		{
			return false;
		}
		if (!object.Equals(Settings, other.Settings))
		{
			return false;
		}
		if (!members_.Equals(other.members_))
		{
			return false;
		}
		if (!object.Equals(JoinCode, other.JoinCode))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (id_ != null)
		{
			num ^= Id.GetHashCode();
		}
		if (data_ != null)
		{
			num ^= Data.GetHashCode();
		}
		if (settings_ != null)
		{
			num ^= Settings.GetHashCode();
		}
		num ^= members_.GetHashCode();
		if (joinCode_ != null)
		{
			num ^= JoinCode.GetHashCode();
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
		if (id_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Id);
		}
		if (data_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Data);
		}
		if (settings_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Settings);
		}
		members_.WriteTo(ref output, _repeated_members_codec);
		if (joinCode_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(JoinCode);
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
		if (id_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Id);
		}
		if (data_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Data);
		}
		if (settings_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Settings);
		}
		num += members_.CalculateSize(_repeated_members_codec);
		if (joinCode_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(JoinCode);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PresencePartyUpdate other)
	{
		if (other == null)
		{
			return;
		}
		if (other.id_ != null)
		{
			if (id_ == null)
			{
				Id = new PartyId();
			}
			Id.MergeFrom(other.Id);
		}
		if (other.data_ != null)
		{
			if (data_ == null)
			{
				Data = new NullableString();
			}
			Data.MergeFrom(other.Data);
		}
		if (other.settings_ != null)
		{
			if (settings_ == null)
			{
				Settings = new PartySettings();
			}
			Settings.MergeFrom(other.Settings);
		}
		members_.Add(other.members_);
		if (other.joinCode_ != null)
		{
			if (joinCode_ == null)
			{
				JoinCode = new NullableString();
			}
			JoinCode.MergeFrom(other.JoinCode);
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
			case 18u:
				if (id_ == null)
				{
					Id = new PartyId();
				}
				input.ReadMessage(Id);
				break;
			case 26u:
				if (data_ == null)
				{
					Data = new NullableString();
				}
				input.ReadMessage(Data);
				break;
			case 34u:
				if (settings_ == null)
				{
					Settings = new PartySettings();
				}
				input.ReadMessage(Settings);
				break;
			case 42u:
				members_.AddEntriesFrom(ref input, _repeated_members_codec);
				break;
			case 50u:
				if (joinCode_ == null)
				{
					JoinCode = new NullableString();
				}
				input.ReadMessage(JoinCode);
				break;
			}
		}
	}
}
