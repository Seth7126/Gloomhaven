using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Challenges;

public sealed class GetChallengesIncrementalUpdateRequest : IMessage<GetChallengesIncrementalUpdateRequest>, IMessage, IEquatable<GetChallengesIncrementalUpdateRequest>, IDeepCloneable<GetChallengesIncrementalUpdateRequest>, IBufferMessage
{
	private static readonly MessageParser<GetChallengesIncrementalUpdateRequest> _parser = new MessageParser<GetChallengesIncrementalUpdateRequest>(() => new GetChallengesIncrementalUpdateRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private UserContext context_;

	public const int ConfigurationContextFieldNumber = 2;

	private ConfigurationContext configurationContext_;

	public const int FromVersionFieldNumber = 3;

	private long fromVersion_;

	[DebuggerNonUserCode]
	public static MessageParser<GetChallengesIncrementalUpdateRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[4];

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
	public ConfigurationContext ConfigurationContext
	{
		get
		{
			return configurationContext_;
		}
		set
		{
			configurationContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long FromVersion
	{
		get
		{
			return fromVersion_;
		}
		set
		{
			fromVersion_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetChallengesIncrementalUpdateRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetChallengesIncrementalUpdateRequest(GetChallengesIncrementalUpdateRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		configurationContext_ = ((other.configurationContext_ != null) ? other.configurationContext_.Clone() : null);
		fromVersion_ = other.fromVersion_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetChallengesIncrementalUpdateRequest Clone()
	{
		return new GetChallengesIncrementalUpdateRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetChallengesIncrementalUpdateRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetChallengesIncrementalUpdateRequest other)
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
		if (!object.Equals(ConfigurationContext, other.ConfigurationContext))
		{
			return false;
		}
		if (FromVersion != other.FromVersion)
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
		if (configurationContext_ != null)
		{
			num ^= ConfigurationContext.GetHashCode();
		}
		if (FromVersion != 0)
		{
			num ^= FromVersion.GetHashCode();
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
		if (configurationContext_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ConfigurationContext);
		}
		if (FromVersion != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt64(FromVersion);
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
		if (configurationContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ConfigurationContext);
		}
		if (FromVersion != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FromVersion);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetChallengesIncrementalUpdateRequest other)
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
		if (other.configurationContext_ != null)
		{
			if (configurationContext_ == null)
			{
				ConfigurationContext = new ConfigurationContext();
			}
			ConfigurationContext.MergeFrom(other.ConfigurationContext);
		}
		if (other.FromVersion != 0)
		{
			FromVersion = other.FromVersion;
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
				if (configurationContext_ == null)
				{
					ConfigurationContext = new ConfigurationContext();
				}
				input.ReadMessage(ConfigurationContext);
				break;
			case 24u:
				FromVersion = input.ReadInt64();
				break;
			}
		}
	}
}
