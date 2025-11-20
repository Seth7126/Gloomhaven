using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.UserReports;

public sealed class SendUserReportRequest : IMessage<SendUserReportRequest>, IMessage, IEquatable<SendUserReportRequest>, IDeepCloneable<SendUserReportRequest>, IBufferMessage
{
	private static readonly MessageParser<SendUserReportRequest> _parser = new MessageParser<SendUserReportRequest>(() => new SendUserReportRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int ToUserIdFieldNumber = 2;

	private string toUserId_ = "";

	public const int ReportReasonIdFieldNumber = 3;

	private string reportReasonId_ = "";

	public const int UserMessageFieldNumber = 4;

	private string userMessage_ = "";

	public const int UserReportsPropertiesFieldNumber = 5;

	private static readonly FieldCodec<UserReportsProperty> _repeated_userReportsProperties_codec = FieldCodec.ForMessage(42u, UserReportsProperty.Parser);

	private readonly RepeatedField<UserReportsProperty> userReportsProperties_ = new RepeatedField<UserReportsProperty>();

	[DebuggerNonUserCode]
	public static MessageParser<SendUserReportRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => UserReportsContractsReflection.Descriptor.MessageTypes[1];

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
	public string ToUserId
	{
		get
		{
			return toUserId_;
		}
		set
		{
			toUserId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ReportReasonId
	{
		get
		{
			return reportReasonId_;
		}
		set
		{
			reportReasonId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string UserMessage
	{
		get
		{
			return userMessage_;
		}
		set
		{
			userMessage_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<UserReportsProperty> UserReportsProperties => userReportsProperties_;

	[DebuggerNonUserCode]
	public SendUserReportRequest()
	{
	}

	[DebuggerNonUserCode]
	public SendUserReportRequest(SendUserReportRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		toUserId_ = other.toUserId_;
		reportReasonId_ = other.reportReasonId_;
		userMessage_ = other.userMessage_;
		userReportsProperties_ = other.userReportsProperties_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SendUserReportRequest Clone()
	{
		return new SendUserReportRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SendUserReportRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SendUserReportRequest other)
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
		if (ToUserId != other.ToUserId)
		{
			return false;
		}
		if (ReportReasonId != other.ReportReasonId)
		{
			return false;
		}
		if (UserMessage != other.UserMessage)
		{
			return false;
		}
		if (!userReportsProperties_.Equals(other.userReportsProperties_))
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
		if (ToUserId.Length != 0)
		{
			num ^= ToUserId.GetHashCode();
		}
		if (ReportReasonId.Length != 0)
		{
			num ^= ReportReasonId.GetHashCode();
		}
		if (UserMessage.Length != 0)
		{
			num ^= UserMessage.GetHashCode();
		}
		num ^= userReportsProperties_.GetHashCode();
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
		if (ToUserId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ToUserId);
		}
		if (ReportReasonId.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(ReportReasonId);
		}
		if (UserMessage.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(UserMessage);
		}
		userReportsProperties_.WriteTo(ref output, _repeated_userReportsProperties_codec);
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
		if (ToUserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ToUserId);
		}
		if (ReportReasonId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ReportReasonId);
		}
		if (UserMessage.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserMessage);
		}
		num += userReportsProperties_.CalculateSize(_repeated_userReportsProperties_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SendUserReportRequest other)
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
		if (other.ToUserId.Length != 0)
		{
			ToUserId = other.ToUserId;
		}
		if (other.ReportReasonId.Length != 0)
		{
			ReportReasonId = other.ReportReasonId;
		}
		if (other.UserMessage.Length != 0)
		{
			UserMessage = other.UserMessage;
		}
		userReportsProperties_.Add(other.userReportsProperties_);
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
				ToUserId = input.ReadString();
				break;
			case 26u:
				ReportReasonId = input.ReadString();
				break;
			case 34u:
				UserMessage = input.ReadString();
				break;
			case 42u:
				userReportsProperties_.AddEntriesFrom(ref input, _repeated_userReportsProperties_codec);
				break;
			}
		}
	}
}
