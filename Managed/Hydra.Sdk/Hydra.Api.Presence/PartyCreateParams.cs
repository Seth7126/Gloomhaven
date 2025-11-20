using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class PartyCreateParams : IMessage<PartyCreateParams>, IMessage, IEquatable<PartyCreateParams>, IDeepCloneable<PartyCreateParams>, IBufferMessage
{
	private static readonly MessageParser<PartyCreateParams> _parser = new MessageParser<PartyCreateParams>(() => new PartyCreateParams());

	private UnknownFieldSet _unknownFields;

	public const int SettingsFieldNumber = 1;

	private PartySettings settings_;

	public const int DataFieldNumber = 2;

	private string data_ = "";

	public const int MemberDataFieldNumber = 3;

	private string memberData_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<PartyCreateParams> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PartyStatusReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PartySettings Settings
	{
		get
		{
			return settings_;
		}
		set
		{
			settings_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Data
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
	public string MemberData
	{
		get
		{
			return memberData_;
		}
		set
		{
			memberData_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PartyCreateParams()
	{
	}

	[DebuggerNonUserCode]
	public PartyCreateParams(PartyCreateParams other)
		: this()
	{
		settings_ = ((other.settings_ != null) ? other.settings_.Clone() : null);
		data_ = other.data_;
		memberData_ = other.memberData_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PartyCreateParams Clone()
	{
		return new PartyCreateParams(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PartyCreateParams);
	}

	[DebuggerNonUserCode]
	public bool Equals(PartyCreateParams other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Settings, other.Settings))
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (MemberData != other.MemberData)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (settings_ != null)
		{
			num ^= Settings.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
		}
		if (MemberData.Length != 0)
		{
			num ^= MemberData.GetHashCode();
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
		if (settings_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Settings);
		}
		if (Data.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Data);
		}
		if (MemberData.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(MemberData);
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
		if (settings_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Settings);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Data);
		}
		if (MemberData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(MemberData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PartyCreateParams other)
	{
		if (other == null)
		{
			return;
		}
		if (other.settings_ != null)
		{
			if (settings_ == null)
			{
				Settings = new PartySettings();
			}
			Settings.MergeFrom(other.Settings);
		}
		if (other.Data.Length != 0)
		{
			Data = other.Data;
		}
		if (other.MemberData.Length != 0)
		{
			MemberData = other.MemberData;
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
				if (settings_ == null)
				{
					Settings = new PartySettings();
				}
				input.ReadMessage(Settings);
				break;
			case 18u:
				Data = input.ReadString();
				break;
			case 26u:
				MemberData = input.ReadString();
				break;
			}
		}
	}
}
