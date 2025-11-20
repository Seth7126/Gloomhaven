using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.UserReports;

public sealed class SendUserReportResponse : IMessage<SendUserReportResponse>, IMessage, IEquatable<SendUserReportResponse>, IDeepCloneable<SendUserReportResponse>, IBufferMessage
{
	private static readonly MessageParser<SendUserReportResponse> _parser = new MessageParser<SendUserReportResponse>(() => new SendUserReportResponse());

	private UnknownFieldSet _unknownFields;

	public const int ResultFieldNumber = 1;

	private SendUserReportResult result_ = SendUserReportResult.None;

	public const int UserReportIdFieldNumber = 2;

	private string userReportId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SendUserReportResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => UserReportsContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public SendUserReportResult Result
	{
		get
		{
			return result_;
		}
		set
		{
			result_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string UserReportId
	{
		get
		{
			return userReportId_;
		}
		set
		{
			userReportId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SendUserReportResponse()
	{
	}

	[DebuggerNonUserCode]
	public SendUserReportResponse(SendUserReportResponse other)
		: this()
	{
		result_ = other.result_;
		userReportId_ = other.userReportId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SendUserReportResponse Clone()
	{
		return new SendUserReportResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SendUserReportResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(SendUserReportResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Result != other.Result)
		{
			return false;
		}
		if (UserReportId != other.UserReportId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Result != SendUserReportResult.None)
		{
			num ^= Result.GetHashCode();
		}
		if (UserReportId.Length != 0)
		{
			num ^= UserReportId.GetHashCode();
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
		if (Result != SendUserReportResult.None)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)Result);
		}
		if (UserReportId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(UserReportId);
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
		if (Result != SendUserReportResult.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Result);
		}
		if (UserReportId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserReportId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SendUserReportResponse other)
	{
		if (other != null)
		{
			if (other.Result != SendUserReportResult.None)
			{
				Result = other.Result;
			}
			if (other.UserReportId.Length != 0)
			{
				UserReportId = other.UserReportId;
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
				Result = (SendUserReportResult)input.ReadEnum();
				break;
			case 18u:
				UserReportId = input.ReadString();
				break;
			}
		}
	}
}
