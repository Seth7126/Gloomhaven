using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Presence;

public sealed class PartyInviteAcceptRequest : IMessage<PartyInviteAcceptRequest>, IMessage, IEquatable<PartyInviteAcceptRequest>, IDeepCloneable<PartyInviteAcceptRequest>, IBufferMessage
{
	private static readonly MessageParser<PartyInviteAcceptRequest> _parser = new MessageParser<PartyInviteAcceptRequest>(() => new PartyInviteAcceptRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int InviteDataFieldNumber = 2;

	private InviteData inviteData_;

	[DebuggerNonUserCode]
	public static MessageParser<PartyInviteAcceptRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceServiceContractsReflection.Descriptor.MessageTypes[18];

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
	public InviteData InviteData
	{
		get
		{
			return inviteData_;
		}
		set
		{
			inviteData_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PartyInviteAcceptRequest()
	{
	}

	[DebuggerNonUserCode]
	public PartyInviteAcceptRequest(PartyInviteAcceptRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		inviteData_ = ((other.inviteData_ != null) ? other.inviteData_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PartyInviteAcceptRequest Clone()
	{
		return new PartyInviteAcceptRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PartyInviteAcceptRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(PartyInviteAcceptRequest other)
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
		if (!object.Equals(InviteData, other.InviteData))
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
		if (inviteData_ != null)
		{
			num ^= InviteData.GetHashCode();
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
		if (inviteData_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(InviteData);
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
		if (inviteData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(InviteData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PartyInviteAcceptRequest other)
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
		if (other.inviteData_ != null)
		{
			if (inviteData_ == null)
			{
				InviteData = new InviteData();
			}
			InviteData.MergeFrom(other.InviteData);
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
				if (inviteData_ == null)
				{
					InviteData = new InviteData();
				}
				input.ReadMessage(InviteData);
				break;
			}
		}
	}
}
