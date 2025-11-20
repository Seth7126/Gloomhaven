using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Facts;

public sealed class WriteBinaryPackUserRequest : IMessage<WriteBinaryPackUserRequest>, IMessage, IEquatable<WriteBinaryPackUserRequest>, IDeepCloneable<WriteBinaryPackUserRequest>, IBufferMessage
{
	private static readonly MessageParser<WriteBinaryPackUserRequest> _parser = new MessageParser<WriteBinaryPackUserRequest>(() => new WriteBinaryPackUserRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int EntriesFieldNumber = 2;

	private ByteString entries_ = ByteString.Empty;

	public const int DataFieldNumber = 3;

	private ByteString data_ = ByteString.Empty;

	public const int HeaderFieldNumber = 4;

	private FactsPackHeader header_;

	[DebuggerNonUserCode]
	public static MessageParser<WriteBinaryPackUserRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => FactsContractsReflection.Descriptor.MessageTypes[0];

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
	public WriteBinaryPackUserRequest()
	{
	}

	[DebuggerNonUserCode]
	public WriteBinaryPackUserRequest(WriteBinaryPackUserRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		entries_ = other.entries_;
		data_ = other.data_;
		header_ = ((other.header_ != null) ? other.header_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public WriteBinaryPackUserRequest Clone()
	{
		return new WriteBinaryPackUserRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as WriteBinaryPackUserRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(WriteBinaryPackUserRequest other)
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
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
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
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
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
	public void MergeFrom(WriteBinaryPackUserRequest other)
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
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
