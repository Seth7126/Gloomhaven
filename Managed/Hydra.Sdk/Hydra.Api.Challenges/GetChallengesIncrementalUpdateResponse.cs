using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class GetChallengesIncrementalUpdateResponse : IMessage<GetChallengesIncrementalUpdateResponse>, IMessage, IEquatable<GetChallengesIncrementalUpdateResponse>, IDeepCloneable<GetChallengesIncrementalUpdateResponse>, IBufferMessage
{
	private static readonly MessageParser<GetChallengesIncrementalUpdateResponse> _parser = new MessageParser<GetChallengesIncrementalUpdateResponse>(() => new GetChallengesIncrementalUpdateResponse());

	private UnknownFieldSet _unknownFields;

	public const int UserChallengesIncrementalUpdateFieldNumber = 1;

	private UserChallengesIncrementalUpdate userChallengesIncrementalUpdate_;

	public const int VersionFieldNumber = 2;

	private long version_;

	[DebuggerNonUserCode]
	public static MessageParser<GetChallengesIncrementalUpdateResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserChallengesIncrementalUpdate UserChallengesIncrementalUpdate
	{
		get
		{
			return userChallengesIncrementalUpdate_;
		}
		set
		{
			userChallengesIncrementalUpdate_ = value;
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
	public GetChallengesIncrementalUpdateResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetChallengesIncrementalUpdateResponse(GetChallengesIncrementalUpdateResponse other)
		: this()
	{
		userChallengesIncrementalUpdate_ = ((other.userChallengesIncrementalUpdate_ != null) ? other.userChallengesIncrementalUpdate_.Clone() : null);
		version_ = other.version_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetChallengesIncrementalUpdateResponse Clone()
	{
		return new GetChallengesIncrementalUpdateResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetChallengesIncrementalUpdateResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetChallengesIncrementalUpdateResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserChallengesIncrementalUpdate, other.UserChallengesIncrementalUpdate))
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
		if (userChallengesIncrementalUpdate_ != null)
		{
			num ^= UserChallengesIncrementalUpdate.GetHashCode();
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
		if (userChallengesIncrementalUpdate_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserChallengesIncrementalUpdate);
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
		if (userChallengesIncrementalUpdate_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserChallengesIncrementalUpdate);
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
	public void MergeFrom(GetChallengesIncrementalUpdateResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userChallengesIncrementalUpdate_ != null)
		{
			if (userChallengesIncrementalUpdate_ == null)
			{
				UserChallengesIncrementalUpdate = new UserChallengesIncrementalUpdate();
			}
			UserChallengesIncrementalUpdate.MergeFrom(other.UserChallengesIncrementalUpdate);
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
				if (userChallengesIncrementalUpdate_ == null)
				{
					UserChallengesIncrementalUpdate = new UserChallengesIncrementalUpdate();
				}
				input.ReadMessage(UserChallengesIncrementalUpdate);
				break;
			case 16u:
				Version = input.ReadInt64();
				break;
			}
		}
	}
}
