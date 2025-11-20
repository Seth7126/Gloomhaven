using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Builds.Common;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.BuildServers;

public sealed class UploadBuildPackConfigToolRequest : IMessage<UploadBuildPackConfigToolRequest>, IMessage, IEquatable<UploadBuildPackConfigToolRequest>, IDeepCloneable<UploadBuildPackConfigToolRequest>, IBufferMessage
{
	private static readonly MessageParser<UploadBuildPackConfigToolRequest> _parser = new MessageParser<UploadBuildPackConfigToolRequest>(() => new UploadBuildPackConfigToolRequest());

	private UnknownFieldSet _unknownFields;

	public const int ToolContextFieldNumber = 1;

	private ToolContext toolContext_;

	public const int ConfigDataFieldNumber = 2;

	private string configData_ = "";

	public const int PackIdFieldNumber = 3;

	private string packId_ = "";

	public const int BuildVersionsWithIdFieldNumber = 4;

	private static readonly FieldCodec<BuildVersionWithIdDto> _repeated_buildVersionsWithId_codec = FieldCodec.ForMessage(34u, BuildVersionWithIdDto.Parser);

	private readonly RepeatedField<BuildVersionWithIdDto> buildVersionsWithId_ = new RepeatedField<BuildVersionWithIdDto>();

	public const int CompressedSizeFieldNumber = 5;

	private long compressedSize_;

	public const int DecompressedSizeFieldNumber = 6;

	private long decompressedSize_;

	[DebuggerNonUserCode]
	public static MessageParser<UploadBuildPackConfigToolRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => BuildServersContractsReflection.Descriptor.MessageTypes[4];

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
	public string PackId
	{
		get
		{
			return packId_;
		}
		set
		{
			packId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<BuildVersionWithIdDto> BuildVersionsWithId => buildVersionsWithId_;

	[DebuggerNonUserCode]
	public long CompressedSize
	{
		get
		{
			return compressedSize_;
		}
		set
		{
			compressedSize_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long DecompressedSize
	{
		get
		{
			return decompressedSize_;
		}
		set
		{
			decompressedSize_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UploadBuildPackConfigToolRequest()
	{
	}

	[DebuggerNonUserCode]
	public UploadBuildPackConfigToolRequest(UploadBuildPackConfigToolRequest other)
		: this()
	{
		toolContext_ = ((other.toolContext_ != null) ? other.toolContext_.Clone() : null);
		configData_ = other.configData_;
		packId_ = other.packId_;
		buildVersionsWithId_ = other.buildVersionsWithId_.Clone();
		compressedSize_ = other.compressedSize_;
		decompressedSize_ = other.decompressedSize_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UploadBuildPackConfigToolRequest Clone()
	{
		return new UploadBuildPackConfigToolRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UploadBuildPackConfigToolRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(UploadBuildPackConfigToolRequest other)
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
		if (PackId != other.PackId)
		{
			return false;
		}
		if (!buildVersionsWithId_.Equals(other.buildVersionsWithId_))
		{
			return false;
		}
		if (CompressedSize != other.CompressedSize)
		{
			return false;
		}
		if (DecompressedSize != other.DecompressedSize)
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
		if (PackId.Length != 0)
		{
			num ^= PackId.GetHashCode();
		}
		num ^= buildVersionsWithId_.GetHashCode();
		if (CompressedSize != 0)
		{
			num ^= CompressedSize.GetHashCode();
		}
		if (DecompressedSize != 0)
		{
			num ^= DecompressedSize.GetHashCode();
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
		if (PackId.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(PackId);
		}
		buildVersionsWithId_.WriteTo(ref output, _repeated_buildVersionsWithId_codec);
		if (CompressedSize != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt64(CompressedSize);
		}
		if (DecompressedSize != 0)
		{
			output.WriteRawTag(48);
			output.WriteInt64(DecompressedSize);
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
		if (PackId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PackId);
		}
		num += buildVersionsWithId_.CalculateSize(_repeated_buildVersionsWithId_codec);
		if (CompressedSize != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CompressedSize);
		}
		if (DecompressedSize != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(DecompressedSize);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UploadBuildPackConfigToolRequest other)
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
		if (other.PackId.Length != 0)
		{
			PackId = other.PackId;
		}
		buildVersionsWithId_.Add(other.buildVersionsWithId_);
		if (other.CompressedSize != 0)
		{
			CompressedSize = other.CompressedSize;
		}
		if (other.DecompressedSize != 0)
		{
			DecompressedSize = other.DecompressedSize;
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
			case 26u:
				PackId = input.ReadString();
				break;
			case 34u:
				buildVersionsWithId_.AddEntriesFrom(ref input, _repeated_buildVersionsWithId_codec);
				break;
			case 40u:
				CompressedSize = input.ReadInt64();
				break;
			case 48u:
				DecompressedSize = input.ReadInt64();
				break;
			}
		}
	}
}
