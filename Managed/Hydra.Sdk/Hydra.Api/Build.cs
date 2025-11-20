using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api;

public sealed class Build : IMessage<Build>, IMessage, IEquatable<Build>, IDeepCloneable<Build>, IBufferMessage
{
	private static readonly MessageParser<Build> _parser = new MessageParser<Build>(() => new Build());

	private UnknownFieldSet _unknownFields;

	public const int VersionFieldNumber = 1;

	private string version_ = "";

	public const int HashFieldNumber = 2;

	private string hash_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<Build> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => VersionReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Hash
	{
		get
		{
			return hash_;
		}
		set
		{
			hash_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public Build()
	{
	}

	[DebuggerNonUserCode]
	public Build(Build other)
		: this()
	{
		version_ = other.version_;
		hash_ = other.hash_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public Build Clone()
	{
		return new Build(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as Build);
	}

	[DebuggerNonUserCode]
	public bool Equals(Build other)
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
		if (Hash != other.Hash)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Version.Length != 0)
		{
			num ^= Version.GetHashCode();
		}
		if (Hash.Length != 0)
		{
			num ^= Hash.GetHashCode();
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
		if (Version.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Version);
		}
		if (Hash.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Hash);
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
		if (Version.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Version);
		}
		if (Hash.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Hash);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(Build other)
	{
		if (other != null)
		{
			if (other.Version.Length != 0)
			{
				Version = other.Version;
			}
			if (other.Hash.Length != 0)
			{
				Hash = other.Hash;
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
				Version = input.ReadString();
				break;
			case 18u:
				Hash = input.ReadString();
				break;
			}
		}
	}
}
