using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.CrossSave;

public sealed class CompleteTransferResponse : IMessage<CompleteTransferResponse>, IMessage, IEquatable<CompleteTransferResponse>, IDeepCloneable<CompleteTransferResponse>, IBufferMessage
{
	private static readonly MessageParser<CompleteTransferResponse> _parser = new MessageParser<CompleteTransferResponse>(() => new CompleteTransferResponse());

	private UnknownFieldSet _unknownFields;

	public const int TransferSnapshotFieldNumber = 1;

	private SaveSnapshot transferSnapshot_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CompleteTransferResponse> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => CrossSaveContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SaveSnapshot TransferSnapshot
	{
		get
		{
			return transferSnapshot_;
		}
		set
		{
			transferSnapshot_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CompleteTransferResponse()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CompleteTransferResponse(CompleteTransferResponse other)
		: this()
	{
		transferSnapshot_ = ((other.transferSnapshot_ != null) ? other.transferSnapshot_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CompleteTransferResponse Clone()
	{
		return new CompleteTransferResponse(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CompleteTransferResponse);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CompleteTransferResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(TransferSnapshot, other.TransferSnapshot))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (transferSnapshot_ != null)
		{
			num ^= TransferSnapshot.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (transferSnapshot_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(TransferSnapshot);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (transferSnapshot_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(TransferSnapshot);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CompleteTransferResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.transferSnapshot_ != null)
		{
			if (transferSnapshot_ == null)
			{
				TransferSnapshot = new SaveSnapshot();
			}
			TransferSnapshot.MergeFrom(other.TransferSnapshot);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			if (num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				continue;
			}
			if (transferSnapshot_ == null)
			{
				TransferSnapshot = new SaveSnapshot();
			}
			input.ReadMessage(TransferSnapshot);
		}
	}
}
