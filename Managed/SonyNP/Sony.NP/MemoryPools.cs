namespace Sony.NP;

public struct MemoryPools
{
	public const int TOOLKIT_MEM_DEFAULT_SIZE = 16777216;

	public const int JSON_MEM_MINIMUM_SIZE = 16384;

	public const int JSON_MEM_DEFAULT_SIZE = 4194304;

	public const int WEB_API_MEM_DEFAULT_SIZE = 1048576;

	public const int HTTP_MEM_DEFAULT_SIZE = 65536;

	public const int HTTP_MEM_MINIMUM_SIZE = 16384;

	public const int SSL_MEM_DEFAULT_SIZE = 262144;

	public const int SSL_MEM_MINIMUM_SIZE = 32768;

	public const int NET_MEM_DEFAULT_SIZE = 32768;

	public const int NET_MEM_MINIMUM_SIZE = 4096;

	public const int MATCHING_MEM_DEFAULT_SIZE = 524288;

	public const int MATCHING_SSL_MEM_DEFAULT_SIZE = 196608;

	public const int IN_GAME_MESSAGE_MEM_DEFAULT_SIZE = 16384;

	private ulong toolkitPoolSize;

	private ulong jsonPoolSize;

	private ulong webApiPoolSize;

	private ulong httpPoolSize;

	private ulong sslPoolSize;

	private ulong netPoolSize;

	private ulong matchingPoolSize;

	private ulong matchingSslPoolSize;

	private ulong inGameMessagePoolSize;

	public ulong ToolkitPoolSize
	{
		get
		{
			return toolkitPoolSize;
		}
		set
		{
			Validate("ToolkitPoolSize", value, 0uL, "", mustBe16kbAlligned: true);
			toolkitPoolSize = value;
		}
	}

	public ulong JsonPoolSize
	{
		get
		{
			return jsonPoolSize;
		}
		set
		{
			Validate("JsonPoolSize", value, 16384uL, "JSON_MEM_MINIMUM_SIZE", mustBe16kbAlligned: true);
			jsonPoolSize = value;
		}
	}

	public ulong WebApiPoolSize
	{
		get
		{
			return webApiPoolSize;
		}
		set
		{
			Validate("WebApiPoolSize", value, 0uL, "", mustBe16kbAlligned: true);
			webApiPoolSize = value;
		}
	}

	public ulong HttpPoolSize
	{
		get
		{
			return httpPoolSize;
		}
		set
		{
			Validate("HttpPoolSize", value, 16384uL, "HTTP_MEM_MINIMUM_SIZE", mustBe16kbAlligned: true);
			httpPoolSize = value;
		}
	}

	public ulong SslPoolSize
	{
		get
		{
			return sslPoolSize;
		}
		set
		{
			Validate("SslPoolSize", value, 32768uL, "SSL_MEM_MINIMUM_SIZE", mustBe16kbAlligned: true);
			sslPoolSize = value;
		}
	}

	public ulong NetPoolSize
	{
		get
		{
			return netPoolSize;
		}
		set
		{
			Validate("NetPoolSize", value, 4096uL, "NET_MEM_MINIMUM_SIZE", mustBe16kbAlligned: true);
			netPoolSize = value;
		}
	}

	public ulong MatchingPoolSize
	{
		get
		{
			return matchingPoolSize;
		}
		set
		{
			Validate("MatchingPoolSize", value, 0uL, "", mustBe16kbAlligned: true);
			matchingPoolSize = value;
		}
	}

	public ulong MatchingSslPoolSize
	{
		get
		{
			return matchingSslPoolSize;
		}
		set
		{
			Validate("MatchingSslPoolSize", value, 0uL, "", mustBe16kbAlligned: true);
			matchingSslPoolSize = value;
		}
	}

	public ulong InGameMessagePoolSize
	{
		get
		{
			return inGameMessagePoolSize;
		}
		set
		{
			Validate("InGameMessagePoolSize", value, 0uL, "", mustBe16kbAlligned: true);
			inGameMessagePoolSize = value;
		}
	}

	private void Validate(string propertyName, ulong size, ulong minSize, string minSizeName, bool mustBe16kbAlligned)
	{
		if (mustBe16kbAlligned && size % 16384 != 0)
		{
			throw new NpToolkitException("The size of the " + propertyName + " must be a multiple of 16 kbs (16384 bytes).");
		}
		if (minSize != 0 && size < minSize)
		{
			throw new NpToolkitException("The size of the " + propertyName + " must be greater than " + minSizeName + ".");
		}
	}

	public void Init()
	{
		toolkitPoolSize = 16777216uL;
		jsonPoolSize = 4194304uL;
		webApiPoolSize = 1048576uL;
		httpPoolSize = 65536uL;
		sslPoolSize = 262144uL;
		netPoolSize = 32768uL;
		matchingPoolSize = 524288uL;
		matchingSslPoolSize = 196608uL;
		inGameMessagePoolSize = 16384uL;
	}
}
