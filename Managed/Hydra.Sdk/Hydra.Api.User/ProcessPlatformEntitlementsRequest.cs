using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.User;

public sealed class ProcessPlatformEntitlementsRequest : IMessage<ProcessPlatformEntitlementsRequest>, IMessage, IEquatable<ProcessPlatformEntitlementsRequest>, IDeepCloneable<ProcessPlatformEntitlementsRequest>, IBufferMessage
{
	private static readonly MessageParser<ProcessPlatformEntitlementsRequest> _parser = new MessageParser<ProcessPlatformEntitlementsRequest>(() => new ProcessPlatformEntitlementsRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int ConfigurationContextFieldNumber = 2;

	private ConfigurationContext configurationContext_;

	[DebuggerNonUserCode]
	public static MessageParser<ProcessPlatformEntitlementsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[19];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext UserContext
	{
		get
		{
			return userContext_;
		}
		set
		{
			userContext_ = value;
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
	public ProcessPlatformEntitlementsRequest()
	{
	}

	[DebuggerNonUserCode]
	public ProcessPlatformEntitlementsRequest(ProcessPlatformEntitlementsRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		configurationContext_ = ((other.configurationContext_ != null) ? other.configurationContext_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ProcessPlatformEntitlementsRequest Clone()
	{
		return new ProcessPlatformEntitlementsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ProcessPlatformEntitlementsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ProcessPlatformEntitlementsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		if (!object.Equals(ConfigurationContext, other.ConfigurationContext))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		if (configurationContext_ != null)
		{
			num ^= ConfigurationContext.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		if (configurationContext_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ConfigurationContext);
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
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (configurationContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ConfigurationContext);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ProcessPlatformEntitlementsRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
		}
		if (other.configurationContext_ != null)
		{
			if (configurationContext_ == null)
			{
				ConfigurationContext = new ConfigurationContext();
			}
			ConfigurationContext.MergeFrom(other.ConfigurationContext);
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 18u:
				if (configurationContext_ == null)
				{
					ConfigurationContext = new ConfigurationContext();
				}
				input.ReadMessage(ConfigurationContext);
				break;
			}
		}
	}
}
