using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ConnectResponse : IMessage<ConnectResponse>, IMessage, IEquatable<ConnectResponse>, IDeepCloneable<ConnectResponse>, IBufferMessage
{
	private static readonly MessageParser<ConnectResponse> _parser = new MessageParser<ConnectResponse>(() => new ConnectResponse());

	private UnknownFieldSet _unknownFields;

	public const int UserChallengesInfoFieldNumber = 1;

	private UserChallengesInfo userChallengesInfo_;

	public const int VersionFieldNumber = 2;

	private long version_;

	[DebuggerNonUserCode]
	public static MessageParser<ConnectResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserChallengesInfo UserChallengesInfo
	{
		get
		{
			return userChallengesInfo_;
		}
		set
		{
			userChallengesInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ConnectResponse()
	{
	}

	[DebuggerNonUserCode]
	public ConnectResponse(ConnectResponse other)
		: this()
	{
		userChallengesInfo_ = ((other.userChallengesInfo_ != null) ? other.userChallengesInfo_.Clone() : null);
		version_ = other.version_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ConnectResponse Clone()
	{
		return new ConnectResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ConnectResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(ConnectResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserChallengesInfo, other.UserChallengesInfo))
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (userChallengesInfo_ != null)
		{
			num ^= UserChallengesInfo.GetHashCode();
		}
		if (Version != 0)
		{
			num ^= Version.GetHashCode();
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
		if (userChallengesInfo_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserChallengesInfo);
		}
		if (Version != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(Version);
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
		if (userChallengesInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserChallengesInfo);
		}
		if (Version != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Version);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ConnectResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userChallengesInfo_ != null)
		{
			if (userChallengesInfo_ == null)
			{
				UserChallengesInfo = new UserChallengesInfo();
			}
			UserChallengesInfo.MergeFrom(other.UserChallengesInfo);
		}
		if (other.Version != 0)
		{
			Version = other.Version;
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
				if (userChallengesInfo_ == null)
				{
					UserChallengesInfo = new UserChallengesInfo();
				}
				input.ReadMessage(UserChallengesInfo);
				break;
			case 16u:
				Version = input.ReadInt64();
				break;
			}
		}
	}
}
