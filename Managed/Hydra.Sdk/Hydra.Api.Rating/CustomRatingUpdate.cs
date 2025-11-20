using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public sealed class CustomRatingUpdate : IMessage<CustomRatingUpdate>, IMessage, IEquatable<CustomRatingUpdate>, IDeepCloneable<CustomRatingUpdate>, IBufferMessage
{
	private static readonly MessageParser<CustomRatingUpdate> _parser = new MessageParser<CustomRatingUpdate>(() => new CustomRatingUpdate());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int RatingFieldNumber = 2;

	private double rating_;

	public const int CustomDataFieldNumber = 3;

	private string customData_ = "";

	public const int ResultFieldNumber = 4;

	private MatchResult result_ = MatchResult.None;

	[DebuggerNonUserCode]
	public static MessageParser<CustomRatingUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RatingContractsReflection.Descriptor.MessageTypes[17];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string UserId
	{
		get
		{
			return userId_;
		}
		set
		{
			userId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public double Rating
	{
		get
		{
			return rating_;
		}
		set
		{
			rating_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string CustomData
	{
		get
		{
			return customData_;
		}
		set
		{
			customData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public MatchResult Result
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
	public CustomRatingUpdate()
	{
	}

	[DebuggerNonUserCode]
	public CustomRatingUpdate(CustomRatingUpdate other)
		: this()
	{
		userId_ = other.userId_;
		rating_ = other.rating_;
		customData_ = other.customData_;
		result_ = other.result_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CustomRatingUpdate Clone()
	{
		return new CustomRatingUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CustomRatingUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(CustomRatingUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UserId != other.UserId)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Rating, other.Rating))
		{
			return false;
		}
		if (CustomData != other.CustomData)
		{
			return false;
		}
		if (Result != other.Result)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UserId.Length != 0)
		{
			num ^= UserId.GetHashCode();
		}
		if (Rating != 0.0)
		{
			num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Rating);
		}
		if (CustomData.Length != 0)
		{
			num ^= CustomData.GetHashCode();
		}
		if (Result != MatchResult.None)
		{
			num ^= Result.GetHashCode();
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
		if (UserId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(UserId);
		}
		if (Rating != 0.0)
		{
			output.WriteRawTag(17);
			output.WriteDouble(Rating);
		}
		if (CustomData.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(CustomData);
		}
		if (Result != MatchResult.None)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)Result);
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
		if (UserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserId);
		}
		if (Rating != 0.0)
		{
			num += 9;
		}
		if (CustomData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CustomData);
		}
		if (Result != MatchResult.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Result);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CustomRatingUpdate other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			if (other.Rating != 0.0)
			{
				Rating = other.Rating;
			}
			if (other.CustomData.Length != 0)
			{
				CustomData = other.CustomData;
			}
			if (other.Result != MatchResult.None)
			{
				Result = other.Result;
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
			case 10u:
				UserId = input.ReadString();
				break;
			case 17u:
				Rating = input.ReadDouble();
				break;
			case 26u:
				CustomData = input.ReadString();
				break;
			case 32u:
				Result = (MatchResult)input.ReadEnum();
				break;
			}
		}
	}
}
