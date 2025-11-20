using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Leaderboards;

public sealed class UpdateLeaderboardUserRequest : IMessage<UpdateLeaderboardUserRequest>, IMessage, IEquatable<UpdateLeaderboardUserRequest>, IDeepCloneable<UpdateLeaderboardUserRequest>, IBufferMessage
{
	private static readonly MessageParser<UpdateLeaderboardUserRequest> _parser = new MessageParser<UpdateLeaderboardUserRequest>(() => new UpdateLeaderboardUserRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int LeaderboardIdFieldNumber = 2;

	private string leaderboardId_ = "";

	public const int EntriesFieldNumber = 3;

	private static readonly FieldCodec<UpdateEntry> _repeated_entries_codec = FieldCodec.ForMessage(26u, UpdateEntry.Parser);

	private readonly RepeatedField<UpdateEntry> entries_ = new RepeatedField<UpdateEntry>();

	[DebuggerNonUserCode]
	public static MessageParser<UpdateLeaderboardUserRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => LeaderboardsContractsReflection.Descriptor.MessageTypes[8];

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
	public string LeaderboardId
	{
		get
		{
			return leaderboardId_;
		}
		set
		{
			leaderboardId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<UpdateEntry> Entries => entries_;

	[DebuggerNonUserCode]
	public UpdateLeaderboardUserRequest()
	{
	}

	[DebuggerNonUserCode]
	public UpdateLeaderboardUserRequest(UpdateLeaderboardUserRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		leaderboardId_ = other.leaderboardId_;
		entries_ = other.entries_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UpdateLeaderboardUserRequest Clone()
	{
		return new UpdateLeaderboardUserRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UpdateLeaderboardUserRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(UpdateLeaderboardUserRequest other)
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
		if (LeaderboardId != other.LeaderboardId)
		{
			return false;
		}
		if (!entries_.Equals(other.entries_))
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
		if (LeaderboardId.Length != 0)
		{
			num ^= LeaderboardId.GetHashCode();
		}
		num ^= entries_.GetHashCode();
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
		if (LeaderboardId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(LeaderboardId);
		}
		entries_.WriteTo(ref output, _repeated_entries_codec);
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
		if (LeaderboardId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LeaderboardId);
		}
		num += entries_.CalculateSize(_repeated_entries_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UpdateLeaderboardUserRequest other)
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
		if (other.LeaderboardId.Length != 0)
		{
			LeaderboardId = other.LeaderboardId;
		}
		entries_.Add(other.entries_);
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
				LeaderboardId = input.ReadString();
				break;
			case 26u:
				entries_.AddEntriesFrom(ref input, _repeated_entries_codec);
				break;
			}
		}
	}
}
