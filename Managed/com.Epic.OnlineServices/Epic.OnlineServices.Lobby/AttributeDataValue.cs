namespace Epic.OnlineServices.Lobby;

public struct AttributeDataValue
{
	private long? m_AsInt64;

	private double? m_AsDouble;

	private bool? m_AsBool;

	private Utf8String m_AsUtf8;

	private AttributeType m_ValueType;

	public long? AsInt64
	{
		get
		{
			Helper.Get(m_AsInt64, out var to, m_ValueType, AttributeType.Int64);
			return to;
		}
		set
		{
			Helper.Set(value, ref m_AsInt64, AttributeType.Int64, ref m_ValueType);
		}
	}

	public double? AsDouble
	{
		get
		{
			Helper.Get(m_AsDouble, out var to, m_ValueType, AttributeType.Double);
			return to;
		}
		set
		{
			Helper.Set(value, ref m_AsDouble, AttributeType.Double, ref m_ValueType);
		}
	}

	public bool? AsBool
	{
		get
		{
			Helper.Get(m_AsBool, out var to, m_ValueType, AttributeType.Boolean);
			return to;
		}
		set
		{
			Helper.Set(value, ref m_AsBool, AttributeType.Boolean, ref m_ValueType);
		}
	}

	public Utf8String AsUtf8
	{
		get
		{
			Helper.Get(m_AsUtf8, out var to, m_ValueType, AttributeType.String);
			return to;
		}
		set
		{
			Helper.Set(value, ref m_AsUtf8, AttributeType.String, ref m_ValueType);
		}
	}

	public AttributeType ValueType
	{
		get
		{
			return m_ValueType;
		}
		private set
		{
			m_ValueType = value;
		}
	}

	public static implicit operator AttributeDataValue(long value)
	{
		return new AttributeDataValue
		{
			AsInt64 = value
		};
	}

	public static implicit operator AttributeDataValue(double value)
	{
		return new AttributeDataValue
		{
			AsDouble = value
		};
	}

	public static implicit operator AttributeDataValue(bool value)
	{
		return new AttributeDataValue
		{
			AsBool = value
		};
	}

	public static implicit operator AttributeDataValue(Utf8String value)
	{
		return new AttributeDataValue
		{
			AsUtf8 = value
		};
	}

	public static implicit operator AttributeDataValue(string value)
	{
		return new AttributeDataValue
		{
			AsUtf8 = value
		};
	}

	internal void Set(ref AttributeDataValueInternal other)
	{
		AsInt64 = other.AsInt64;
		AsDouble = other.AsDouble;
		AsBool = other.AsBool;
		AsUtf8 = other.AsUtf8;
	}
}
