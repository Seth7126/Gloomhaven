using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Facts;

public sealed class WriteBinaryPackToolRequest : IMessage<WriteBinaryPackToolRequest>, IMessage, IEquatable<WriteBinaryPackToolRequest>, IDeepCloneable<WriteBinaryPackToolRequest>, IBufferMessage
{
	private static readonly MessageParser<WriteBinaryPackToolRequest> _parser = new MessageParser<WriteBinaryPackToolRequest>(() => new WriteBinaryPackToolRequest());

	private UnknownFieldSet _unknownFields;

	public const int ToolContextFieldNumber = 1;

	private ToolContext toolContext_;

	public const int EntriesFieldNumber = 2;

	private ByteString entries_ = ByteString.Empty;

	public const int DataFieldNumber = 3;

	private ByteString data_ = ByteString.Empty;

	public const int HeaderFieldNumber = 4;

	private FactsPackHeader header_;

	[DebuggerNonUserCode]
	public static MessageParser<WriteBinaryPackToolRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => FactsContractsReflection.Descriptor.MessageTypes[6];

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
	public ByteString Entries
	{
		get
		{
			return entries_;
		}
		set
		{
			entries_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ByteString Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public FactsPackHeader Header
	{
		get
		{
			return header_;
		}
		set
		{
			header_ = value;
		}
	}

	[DebuggerNonUserCode]
	public WriteBinaryPackToolRequest()
	{
	}

	[DebuggerNonUserCode]
	public WriteBinaryPackToolRequest(WriteBinaryPackToolRequest other)
		: this()
	{
		toolContext_ = ((other.toolContext_ != null) ? other.toolContext_.Clone() : null);
		entries_ = other.entries_;
		data_ = other.data_;
		header_ = ((other.header_ != null) ? other.header_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public WriteBinaryPackToolRequest Clone()
	{
		return new WriteBinaryPackToolRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as WriteBinaryPackToolRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(WriteBinaryPackToolRequest other)
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
		if (Entries != other.Entries)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (!object.Equals(Header, other.Header))
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
		if (Entries.Length != 0)
		{
			num ^= Entries.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
		}
		if (header_ != null)
		{
			num ^= Header.GetHashCode();
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
		if (Entries.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteBytes(Entries);
		}
		if (Data.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteBytes(Data);
		}
		if (header_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Header);
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
		if (Entries.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Entries);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Data);
		}
		if (header_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Header);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(WriteBinaryPackToolRequest other)
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
		if (other.Entries.Length != 0)
		{
			Entries = other.Entries;
		}
		if (other.Data.Length != 0)
		{
			Data = other.Data;
		}
		if (other.header_ != null)
		{
			if (header_ == null)
			{
				Header = new FactsPackHeader();
			}
			Header.MergeFrom(other.Header);
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
				Entries = input.ReadBytes();
				break;
			case 26u:
				Data = input.ReadBytes();
				break;
			case 34u:
				if (header_ == null)
				{
					Header = new FactsPackHeader();
				}
				input.ReadMessage(Header);
				break;
			}
		}
	}
}
