using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.BuildServers;

public sealed class AnalyzeBuildPackConfigToolRequest : IMessage<AnalyzeBuildPackConfigToolRequest>, IMessage, IEquatable<AnalyzeBuildPackConfigToolRequest>, IDeepCloneable<AnalyzeBuildPackConfigToolRequest>, IBufferMessage
{
	private static readonly MessageParser<AnalyzeBuildPackConfigToolRequest> _parser = new MessageParser<AnalyzeBuildPackConfigToolRequest>(() => new AnalyzeBuildPackConfigToolRequest());

	private UnknownFieldSet _unknownFields;

	public const int ToolContextFieldNumber = 1;

	private ToolContext toolContext_;

	public const int ConfigDataFieldNumber = 2;

	private string configData_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<AnalyzeBuildPackConfigToolRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => BuildServersContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ToolContext ToolContext
	{
		get
		{
			return toolContext_;
		}
		set
		{
			toolContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string ConfigData
	{
		get
		{
			return configData_;
		}
		set
		{
			configData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public AnalyzeBuildPackConfigToolRequest()
	{
	}

	[DebuggerNonUserCode]
	public AnalyzeBuildPackConfigToolRequest(AnalyzeBuildPackConfigToolRequest other)
		: this()
	{
		toolContext_ = ((other.toolContext_ != null) ? other.toolContext_.Clone() : null);
		configData_ = other.configData_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public AnalyzeBuildPackConfigToolRequest Clone()
	{
		return new AnalyzeBuildPackConfigToolRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as AnalyzeBuildPackConfigToolRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(AnalyzeBuildPackConfigToolRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ToolContext, other.ToolContext))
		{
			return false;
		}
		if (ConfigData != other.ConfigData)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (toolContext_ != null)
		{
			num ^= ToolContext.GetHashCode();
		}
		if (ConfigData.Length != 0)
		{
			num ^= ConfigData.GetHashCode();
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
		if (toolContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ToolContext);
		}
		if (ConfigData.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ConfigData);
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
		if (toolContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ToolContext);
		}
		if (ConfigData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ConfigData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(AnalyzeBuildPackConfigToolRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.toolContext_ != null)
		{
			if (toolContext_ == null)
			{
				ToolContext = new ToolContext();
			}
			ToolContext.MergeFrom(other.ToolContext);
		}
		if (other.ConfigData.Length != 0)
		{
			ConfigData = other.ConfigData;
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
				if (toolContext_ == null)
				{
					ToolContext = new ToolContext();
				}
				input.ReadMessage(ToolContext);
				break;
			case 18u:
				ConfigData = input.ReadString();
				break;
			}
		}
	}
}
