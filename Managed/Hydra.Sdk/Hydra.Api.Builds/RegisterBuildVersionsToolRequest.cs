using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Builds;

public sealed class RegisterBuildVersionsToolRequest : IMessage<RegisterBuildVersionsToolRequest>, IMessage, IEquatable<RegisterBuildVersionsToolRequest>, IDeepCloneable<RegisterBuildVersionsToolRequest>, IBufferMessage
{
	private static readonly MessageParser<RegisterBuildVersionsToolRequest> _parser = new MessageParser<RegisterBuildVersionsToolRequest>(() => new RegisterBuildVersionsToolRequest());

	private UnknownFieldSet _unknownFields;

	public const int ToolContextFieldNumber = 1;

	private ToolContext toolContext_;

	public const int GroupNameFieldNumber = 2;

	private string groupName_ = "";

	public const int BuildVersionInfosFieldNumber = 3;

	private static readonly FieldCodec<BuildVersionInfoDto> _repeated_buildVersionInfos_codec = FieldCodec.ForMessage(26u, BuildVersionInfoDto.Parser);

	private readonly RepeatedField<BuildVersionInfoDto> buildVersionInfos_ = new RepeatedField<BuildVersionInfoDto>();

	[DebuggerNonUserCode]
	public static MessageParser<RegisterBuildVersionsToolRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => BuildsGroupContractsReflection.Descriptor.MessageTypes[0];

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
	public string GroupName
	{
		get
		{
			return groupName_;
		}
		set
		{
			groupName_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<BuildVersionInfoDto> BuildVersionInfos => buildVersionInfos_;

	[DebuggerNonUserCode]
	public RegisterBuildVersionsToolRequest()
	{
	}

	[DebuggerNonUserCode]
	public RegisterBuildVersionsToolRequest(RegisterBuildVersionsToolRequest other)
		: this()
	{
		toolContext_ = ((other.toolContext_ != null) ? other.toolContext_.Clone() : null);
		groupName_ = other.groupName_;
		buildVersionInfos_ = other.buildVersionInfos_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RegisterBuildVersionsToolRequest Clone()
	{
		return new RegisterBuildVersionsToolRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RegisterBuildVersionsToolRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(RegisterBuildVersionsToolRequest other)
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
		if (GroupName != other.GroupName)
		{
			return false;
		}
		if (!buildVersionInfos_.Equals(other.buildVersionInfos_))
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
		if (GroupName.Length != 0)
		{
			num ^= GroupName.GetHashCode();
		}
		num ^= buildVersionInfos_.GetHashCode();
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
		if (GroupName.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(GroupName);
		}
		buildVersionInfos_.WriteTo(ref output, _repeated_buildVersionInfos_codec);
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
		if (GroupName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(GroupName);
		}
		num += buildVersionInfos_.CalculateSize(_repeated_buildVersionInfos_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RegisterBuildVersionsToolRequest other)
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
		if (other.GroupName.Length != 0)
		{
			GroupName = other.GroupName;
		}
		buildVersionInfos_.Add(other.buildVersionInfos_);
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
				GroupName = input.ReadString();
				break;
			case 26u:
				buildVersionInfos_.AddEntriesFrom(ref input, _repeated_buildVersionInfos_codec);
				break;
			}
		}
	}
}
