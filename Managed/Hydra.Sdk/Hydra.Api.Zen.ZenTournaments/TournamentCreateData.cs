using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class TournamentCreateData : IMessage<TournamentCreateData>, IMessage, IEquatable<TournamentCreateData>, IDeepCloneable<TournamentCreateData>, IBufferMessage
{
	private static readonly MessageParser<TournamentCreateData> _parser = new MessageParser<TournamentCreateData>(() => new TournamentCreateData());

	private UnknownFieldSet _unknownFields;

	public const int TitleIdFieldNumber = 1;

	private string titleId_ = "";

	public const int IsVisibleFieldNumber = 2;

	private bool isVisible_;

	public const int TtlFieldNumber = 3;

	private Duration ttl_;

	public const int SettingsFieldNumber = 4;

	private TournamentSettings settings_;

	public const int NameFieldNumber = 5;

	private string name_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<TournamentCreateData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string TitleId
	{
		get
		{
			return titleId_;
		}
		set
		{
			titleId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public bool IsVisible
	{
		get
		{
			return isVisible_;
		}
		set
		{
			isVisible_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Duration Ttl
	{
		get
		{
			return ttl_;
		}
		set
		{
			ttl_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentSettings Settings
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
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TournamentCreateData()
	{
	}

	[DebuggerNonUserCode]
	public TournamentCreateData(TournamentCreateData other)
		: this()
	{
		titleId_ = other.titleId_;
		isVisible_ = other.isVisible_;
		ttl_ = ((other.ttl_ != null) ? other.ttl_.Clone() : null);
		settings_ = ((other.settings_ != null) ? other.settings_.Clone() : null);
		name_ = other.name_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TournamentCreateData Clone()
	{
		return new TournamentCreateData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TournamentCreateData);
	}

	[DebuggerNonUserCode]
	public bool Equals(TournamentCreateData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TitleId != other.TitleId)
		{
			return false;
		}
		if (IsVisible != other.IsVisible)
		{
			return false;
		}
		if (!object.Equals(Ttl, other.Ttl))
		{
			return false;
		}
		if (!object.Equals(Settings, other.Settings))
		{
			return false;
		}
		if (Name != other.Name)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TitleId.Length != 0)
		{
			num ^= TitleId.GetHashCode();
		}
		if (IsVisible)
		{
			num ^= IsVisible.GetHashCode();
		}
		if (ttl_ != null)
		{
			num ^= Ttl.GetHashCode();
		}
		if (settings_ != null)
		{
			num ^= Settings.GetHashCode();
		}
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
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
		if (TitleId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TitleId);
		}
		if (IsVisible)
		{
			output.WriteRawTag(16);
			output.WriteBool(IsVisible);
		}
		if (ttl_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Ttl);
		}
		if (settings_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Settings);
		}
		if (Name.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(Name);
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
		if (TitleId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleId);
		}
		if (IsVisible)
		{
			num += 2;
		}
		if (ttl_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Ttl);
		}
		if (settings_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Settings);
		}
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TournamentCreateData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.TitleId.Length != 0)
		{
			TitleId = other.TitleId;
		}
		if (other.IsVisible)
		{
			IsVisible = other.IsVisible;
		}
		if (other.ttl_ != null)
		{
			if (ttl_ == null)
			{
				Ttl = new Duration();
			}
			Ttl.MergeFrom(other.Ttl);
		}
		if (other.settings_ != null)
		{
			if (settings_ == null)
			{
				Settings = new TournamentSettings();
			}
			Settings.MergeFrom(other.Settings);
		}
		if (other.Name.Length != 0)
		{
			Name = other.Name;
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
				TitleId = input.ReadString();
				break;
			case 16u:
				IsVisible = input.ReadBool();
				break;
			case 26u:
				if (ttl_ == null)
				{
					Ttl = new Duration();
				}
				input.ReadMessage(Ttl);
				break;
			case 34u:
				if (settings_ == null)
				{
					Settings = new TournamentSettings();
				}
				input.ReadMessage(Settings);
				break;
			case 42u:
				Name = input.ReadString();
				break;
			}
		}
	}
}
