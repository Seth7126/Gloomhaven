using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Leaderboards;

public sealed class GetLeaderboardFilteredRequest : IMessage<GetLeaderboardFilteredRequest>, IMessage, IEquatable<GetLeaderboardFilteredRequest>, IDeepCloneable<GetLeaderboardFilteredRequest>, IBufferMessage
{
	private static readonly MessageParser<GetLeaderboardFilteredRequest> _parser = new MessageParser<GetLeaderboardFilteredRequest>(() => new GetLeaderboardFilteredRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int LeaderboardIdFieldNumber = 2;

	private string leaderboardId_ = "";

	public const int UserIdsFieldNumber = 3;

	private static readonly FieldCodec<string> _repeated_userIds_codec = FieldCodec.ForString(26u);

	private readonly RepeatedField<string> userIds_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<GetLeaderboardFilteredRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => LeaderboardsContractsReflection.Descriptor.MessageTypes[5];

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
	public RepeatedField<string> UserIds => userIds_;

	[DebuggerNonUserCode]
	public GetLeaderboardFilteredRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetLeaderboardFilteredRequest(GetLeaderboardFilteredRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		leaderboardId_ = other.leaderboardId_;
		userIds_ = other.userIds_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetLeaderboardFilteredRequest Clone()
	{
		return new GetLeaderboardFilteredRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetLeaderboardFilteredRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetLeaderboardFilteredRequest other)
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
		if (!userIds_.Equals(other.userIds_))
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
		num ^= userIds_.GetHashCode();
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
		userIds_.WriteTo(ref output, _repeated_userIds_codec);
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
		num += userIds_.CalculateSize(_repeated_userIds_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetLeaderboardFilteredRequest other)
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
		userIds_.Add(other.userIds_);
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
				userIds_.AddEntriesFrom(ref input, _repeated_userIds_codec);
				break;
			}
		}
	}
}
