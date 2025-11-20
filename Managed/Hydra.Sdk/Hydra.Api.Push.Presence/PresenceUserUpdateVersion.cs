using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Presence;

public sealed class PresenceUserUpdateVersion : IMessage<PresenceUserUpdateVersion>, IMessage, IEquatable<PresenceUserUpdateVersion>, IDeepCloneable<PresenceUserUpdateVersion>, IBufferMessage
{
	private static readonly MessageParser<PresenceUserUpdateVersion> _parser = new MessageParser<PresenceUserUpdateVersion>(() => new PresenceUserUpdateVersion());

	private UnknownFieldSet _unknownFields;

	public const int VersionFieldNumber = 1;

	private int version_;

	public const int UpdateFieldNumber = 2;

	private PresenceUserUpdate update_;

	[DebuggerNonUserCode]
	public static MessageParser<PresenceUserUpdateVersion> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[9];

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
	public PresenceUserUpdate Update
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
	public PresenceUserUpdateVersion()
	{
	}

	[DebuggerNonUserCode]
	public PresenceUserUpdateVersion(PresenceUserUpdateVersion other)
		: this()
	{
		version_ = other.version_;
		update_ = ((other.update_ != null) ? other.update_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PresenceUserUpdateVersion Clone()
	{
		return new PresenceUserUpdateVersion(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PresenceUserUpdateVersion);
	}

	[DebuggerNonUserCode]
	public bool Equals(PresenceUserUpdateVersion other)
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
		if (update_ != null)
		{
			output.WriteRawTag(18);
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
	public void MergeFrom(PresenceUserUpdateVersion other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Version != 0)
		{
			Version = other.Version;
		}
		if (other.update_ != null)
		{
			if (update_ == null)
			{
				Update = new PresenceUserUpdate();
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
			case 18u:
				if (update_ == null)
				{
					Update = new PresenceUserUpdate();
				}
				input.ReadMessage(Update);
				break;
			}
		}
	}
}
