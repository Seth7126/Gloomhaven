using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class UserChallengesUpdateVersion : IMessage<UserChallengesUpdateVersion>, IMessage, IEquatable<UserChallengesUpdateVersion>, IDeepCloneable<UserChallengesUpdateVersion>, IBufferMessage
{
	private static readonly MessageParser<UserChallengesUpdateVersion> _parser = new MessageParser<UserChallengesUpdateVersion>(() => new UserChallengesUpdateVersion());

	private UnknownFieldSet _unknownFields;

	public const int VersionFieldNumber = 1;

	private int version_;

	public const int FromChallengesVersionFieldNumber = 2;

	private long fromChallengesVersion_;

	public const int ToChallengesVersionFieldNumber = 3;

	private long toChallengesVersion_;

	public const int UpdateFieldNumber = 4;

	private UserChallengesIncrementalUpdate update_;

	[DebuggerNonUserCode]
	public static MessageParser<UserChallengesUpdateVersion> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int Version
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
	public long FromChallengesVersion
	{
		get
		{
			return fromChallengesVersion_;
		}
		set
		{
			fromChallengesVersion_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long ToChallengesVersion
	{
		get
		{
			return toChallengesVersion_;
		}
		set
		{
			toChallengesVersion_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UserChallengesIncrementalUpdate Update
	{
		get
		{
			return update_;
		}
		set
		{
			update_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UserChallengesUpdateVersion()
	{
	}

	[DebuggerNonUserCode]
	public UserChallengesUpdateVersion(UserChallengesUpdateVersion other)
		: this()
	{
		version_ = other.version_;
		fromChallengesVersion_ = other.fromChallengesVersion_;
		toChallengesVersion_ = other.toChallengesVersion_;
		update_ = ((other.update_ != null) ? other.update_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserChallengesUpdateVersion Clone()
	{
		return new UserChallengesUpdateVersion(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserChallengesUpdateVersion);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserChallengesUpdateVersion other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Version != other.Version)
		{
			return false;
		}
		if (FromChallengesVersion != other.FromChallengesVersion)
		{
			return false;
		}
		if (ToChallengesVersion != other.ToChallengesVersion)
		{
			return false;
		}
		if (!object.Equals(Update, other.Update))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Version != 0)
		{
			num ^= Version.GetHashCode();
		}
		if (FromChallengesVersion != 0)
		{
			num ^= FromChallengesVersion.GetHashCode();
		}
		if (ToChallengesVersion != 0)
		{
			num ^= ToChallengesVersion.GetHashCode();
		}
		if (update_ != null)
		{
			num ^= Update.GetHashCode();
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
		if (Version != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(Version);
		}
		if (FromChallengesVersion != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(FromChallengesVersion);
		}
		if (ToChallengesVersion != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt64(ToChallengesVersion);
		}
		if (update_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Update);
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
		if (Version != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Version);
		}
		if (FromChallengesVersion != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FromChallengesVersion);
		}
		if (ToChallengesVersion != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ToChallengesVersion);
		}
		if (update_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Update);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserChallengesUpdateVersion other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Version != 0)
		{
			Version = other.Version;
		}
		if (other.FromChallengesVersion != 0)
		{
			FromChallengesVersion = other.FromChallengesVersion;
		}
		if (other.ToChallengesVersion != 0)
		{
			ToChallengesVersion = other.ToChallengesVersion;
		}
		if (other.update_ != null)
		{
			if (update_ == null)
			{
				Update = new UserChallengesIncrementalUpdate();
			}
			Update.MergeFrom(other.Update);
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
			case 8u:
				Version = input.ReadInt32();
				break;
			case 16u:
				FromChallengesVersion = input.ReadInt64();
				break;
			case 24u:
				ToChallengesVersion = input.ReadInt64();
				break;
			case 34u:
				if (update_ == null)
				{
					Update = new UserChallengesIncrementalUpdate();
				}
				input.ReadMessage(Update);
				break;
			}
		}
	}
}
