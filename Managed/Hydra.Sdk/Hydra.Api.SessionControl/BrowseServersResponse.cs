using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class BrowseServersResponse : IMessage<BrowseServersResponse>, IMessage, IEquatable<BrowseServersResponse>, IDeepCloneable<BrowseServersResponse>, IBufferMessage
{
	private static readonly MessageParser<BrowseServersResponse> _parser = new MessageParser<BrowseServersResponse>(() => new BrowseServersResponse());

	private UnknownFieldSet _unknownFields;

	public const int SlotsTotalFieldNumber = 1;

	private int slotsTotal_;

	public const int SlotsOccupiedFieldNumber = 2;

	private int slotsOccupied_;

	public const int ServersFieldNumber = 3;

	private static readonly FieldCodec<ServerData> _repeated_servers_codec = FieldCodec.ForMessage(26u, ServerData.Parser);

	private readonly RepeatedField<ServerData> servers_ = new RepeatedField<ServerData>();

	[DebuggerNonUserCode]
	public static MessageParser<BrowseServersResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[25];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int SlotsTotal
	{
		get
		{
			return slotsTotal_;
		}
		set
		{
			slotsTotal_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int SlotsOccupied
	{
		get
		{
			return slotsOccupied_;
		}
		set
		{
			slotsOccupied_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ServerData> Servers => servers_;

	[DebuggerNonUserCode]
	public BrowseServersResponse()
	{
	}

	[DebuggerNonUserCode]
	public BrowseServersResponse(BrowseServersResponse other)
		: this()
	{
		slotsTotal_ = other.slotsTotal_;
		slotsOccupied_ = other.slotsOccupied_;
		servers_ = other.servers_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public BrowseServersResponse Clone()
	{
		return new BrowseServersResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as BrowseServersResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(BrowseServersResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SlotsTotal != other.SlotsTotal)
		{
			return false;
		}
		if (SlotsOccupied != other.SlotsOccupied)
		{
			return false;
		}
		if (!servers_.Equals(other.servers_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (SlotsTotal != 0)
		{
			num ^= SlotsTotal.GetHashCode();
		}
		if (SlotsOccupied != 0)
		{
			num ^= SlotsOccupied.GetHashCode();
		}
		num ^= servers_.GetHashCode();
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
		if (SlotsTotal != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(SlotsTotal);
		}
		if (SlotsOccupied != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(SlotsOccupied);
		}
		servers_.WriteTo(ref output, _repeated_servers_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (SlotsTotal != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SlotsTotal);
		}
		if (SlotsOccupied != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SlotsOccupied);
		}
		num += servers_.CalculateSize(_repeated_servers_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(BrowseServersResponse other)
	{
		if (other != null)
		{
			if (other.SlotsTotal != 0)
			{
				SlotsTotal = other.SlotsTotal;
			}
			if (other.SlotsOccupied != 0)
			{
				SlotsOccupied = other.SlotsOccupied;
			}
			servers_.Add(other.servers_);
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
				SlotsTotal = input.ReadInt32();
				break;
			case 16u:
				SlotsOccupied = input.ReadInt32();
				break;
			case 26u:
				servers_.AddEntriesFrom(ref input, _repeated_servers_codec);
				break;
			}
		}
	}
}
