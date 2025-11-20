using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace RedLynx.Api.CrossSave;

public sealed class CompleteTransferRequest : IMessage<CompleteTransferRequest>, IMessage, IEquatable<CompleteTransferRequest>, IDeepCloneable<CompleteTransferRequest>, IBufferMessage
{
	private static readonly MessageParser<CompleteTransferRequest> _parser = new MessageParser<CompleteTransferRequest>(() => new CompleteTransferRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int TransferIdFieldNumber = 2;

	private string transferId_ = "";

	public const int BackupSnapshotFieldNumber = 3;

	private SaveSnapshot backupSnapshot_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CompleteTransferRequest> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => CrossSaveContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
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
	[GeneratedCode("protoc", null)]
	public string TransferId
	{
		get
		{
			return transferId_;
		}
		set
		{
			transferId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SaveSnapshot BackupSnapshot
	{
		get
		{
			return backupSnapshot_;
		}
		set
		{
			backupSnapshot_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CompleteTransferRequest()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CompleteTransferRequest(CompleteTransferRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		transferId_ = other.transferId_;
		backupSnapshot_ = ((other.backupSnapshot_ != null) ? other.backupSnapshot_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CompleteTransferRequest Clone()
	{
		return new CompleteTransferRequest(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CompleteTransferRequest);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CompleteTransferRequest other)
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
		if (TransferId != other.TransferId)
		{
			return false;
		}
		if (!object.Equals(BackupSnapshot, other.BackupSnapshot))
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
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		if (TransferId.Length != 0)
		{
			num ^= TransferId.GetHashCode();
		}
		if (backupSnapshot_ != null)
		{
			num ^= BackupSnapshot.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		if (TransferId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(TransferId);
		}
		if (backupSnapshot_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(BackupSnapshot);
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
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (TransferId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TransferId);
		}
		if (backupSnapshot_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(BackupSnapshot);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CompleteTransferRequest other)
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
		if (other.TransferId.Length != 0)
		{
			TransferId = other.TransferId;
		}
		if (other.backupSnapshot_ != null)
		{
			if (backupSnapshot_ == null)
			{
				BackupSnapshot = new SaveSnapshot();
			}
			BackupSnapshot.MergeFrom(other.BackupSnapshot);
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
				TransferId = input.ReadString();
				break;
			case 26u:
				if (backupSnapshot_ == null)
				{
					BackupSnapshot = new SaveSnapshot();
				}
				input.ReadMessage(BackupSnapshot);
				break;
			}
		}
	}
}
