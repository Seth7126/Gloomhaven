using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ServerChallengesSettings : IMessage<ServerChallengesSettings>, IMessage, IEquatable<ServerChallengesSettings>, IDeepCloneable<ServerChallengesSettings>, IBufferMessage
{
	private static readonly MessageParser<ServerChallengesSettings> _parser = new MessageParser<ServerChallengesSettings>(() => new ServerChallengesSettings());

	private UnknownFieldSet _unknownFields;

	public const int FlushTimeCriteriaSecFieldNumber = 1;

	private long flushTimeCriteriaSec_;

	public const int FlushCountCriteriaFieldNumber = 2;

	private long flushCountCriteria_;

	[DebuggerNonUserCode]
	public static MessageParser<ServerChallengesSettings> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[11];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long FlushTimeCriteriaSec
	{
		get
		{
			return flushTimeCriteriaSec_;
		}
		set
		{
			flushTimeCriteriaSec_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long FlushCountCriteria
	{
		get
		{
			return flushCountCriteria_;
		}
		set
		{
			flushCountCriteria_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ServerChallengesSettings()
	{
	}

	[DebuggerNonUserCode]
	public ServerChallengesSettings(ServerChallengesSettings other)
		: this()
	{
		flushTimeCriteriaSec_ = other.flushTimeCriteriaSec_;
		flushCountCriteria_ = other.flushCountCriteria_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServerChallengesSettings Clone()
	{
		return new ServerChallengesSettings(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServerChallengesSettings);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServerChallengesSettings other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (FlushTimeCriteriaSec != other.FlushTimeCriteriaSec)
		{
			return false;
		}
		if (FlushCountCriteria != other.FlushCountCriteria)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (FlushTimeCriteriaSec != 0)
		{
			num ^= FlushTimeCriteriaSec.GetHashCode();
		}
		if (FlushCountCriteria != 0)
		{
			num ^= FlushCountCriteria.GetHashCode();
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
		if (FlushTimeCriteriaSec != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt64(FlushTimeCriteriaSec);
		}
		if (FlushCountCriteria != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(FlushCountCriteria);
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
		if (FlushTimeCriteriaSec != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FlushTimeCriteriaSec);
		}
		if (FlushCountCriteria != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FlushCountCriteria);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ServerChallengesSettings other)
	{
		if (other != null)
		{
			if (other.FlushTimeCriteriaSec != 0)
			{
				FlushTimeCriteriaSec = other.FlushTimeCriteriaSec;
			}
			if (other.FlushCountCriteria != 0)
			{
				FlushCountCriteria = other.FlushCountCriteria;
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
			case 8u:
				FlushTimeCriteriaSec = input.ReadInt64();
				break;
			case 16u:
				FlushCountCriteria = input.ReadInt64();
				break;
			}
		}
	}
}
