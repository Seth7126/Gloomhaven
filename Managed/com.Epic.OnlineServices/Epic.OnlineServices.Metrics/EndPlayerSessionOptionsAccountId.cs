namespace Epic.OnlineServices.Metrics;

public struct EndPlayerSessionOptionsAccountId
{
	private MetricsAccountIdType m_AccountIdType;

	private EpicAccountId m_Epic;

	private Utf8String m_External;

	public MetricsAccountIdType AccountIdType
	{
		get
		{
			return m_AccountIdType;
		}
		private set
		{
			m_AccountIdType = value;
		}
	}

	public EpicAccountId Epic
	{
		get
		{
			Helper.Get(m_Epic, out var to, m_AccountIdType, MetricsAccountIdType.Epic);
			return to;
		}
		set
		{
			Helper.Set(value, ref m_Epic, MetricsAccountIdType.Epic, ref m_AccountIdType);
		}
	}

	public Utf8String External
	{
		get
		{
			Helper.Get(m_External, out var to, m_AccountIdType, MetricsAccountIdType.External);
			return to;
		}
		set
		{
			Helper.Set(value, ref m_External, MetricsAccountIdType.External, ref m_AccountIdType);
		}
	}

	public static implicit operator EndPlayerSessionOptionsAccountId(EpicAccountId value)
	{
		return new EndPlayerSessionOptionsAccountId
		{
			Epic = value
		};
	}

	public static implicit operator EndPlayerSessionOptionsAccountId(Utf8String value)
	{
		return new EndPlayerSessionOptionsAccountId
		{
			External = value
		};
	}

	public static implicit operator EndPlayerSessionOptionsAccountId(string value)
	{
		return new EndPlayerSessionOptionsAccountId
		{
			External = value
		};
	}

	internal void Set(ref EndPlayerSessionOptionsAccountIdInternal other)
	{
		Epic = other.Epic;
		External = other.External;
	}
}
