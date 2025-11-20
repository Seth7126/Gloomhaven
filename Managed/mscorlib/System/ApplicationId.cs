using System.Text;

namespace System;

/// <summary>Contains information used to uniquely identify a manifest-based application. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class ApplicationId
{
	private readonly byte[] _publicKeyToken;

	/// <summary>Gets a string representing the culture information for the application.</summary>
	/// <returns>The culture information for the application.</returns>
	/// <filterpriority>2</filterpriority>
	public string Culture { get; }

	/// <summary>Gets the name of the application.</summary>
	/// <returns>The name of the application.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name { get; }

	/// <summary>Gets the target processor architecture for the application.</summary>
	/// <returns>The processor architecture of the application.</returns>
	/// <filterpriority>2</filterpriority>
	public string ProcessorArchitecture { get; }

	/// <summary>Gets the version of the application.</summary>
	/// <returns>A <see cref="T:System.Version" /> that specifies the version of the application.</returns>
	/// <filterpriority>2</filterpriority>
	public Version Version { get; }

	/// <summary>Gets the public key token for the application.</summary>
	/// <returns>A byte array containing the public key token for the application.</returns>
	/// <filterpriority>2</filterpriority>
	public byte[] PublicKeyToken => (byte[])_publicKeyToken.Clone();

	/// <summary>Initializes a new instance of the <see cref="T:System.ApplicationId" /> class.</summary>
	/// <param name="publicKeyToken">The array of bytes representing the raw public key data. </param>
	/// <param name="name">The name of the application. </param>
	/// <param name="version">A <see cref="T:System.Version" /> object that specifies the version of the application. </param>
	/// <param name="processorArchitecture">The processor architecture of the application. </param>
	/// <param name="culture">The culture of the application. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name " />is null.-or-<paramref name="version " />is null.-or-<paramref name="publicKeyToken " />is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name " />is an empty string.</exception>
	public ApplicationId(byte[] publicKeyToken, string name, Version version, string processorArchitecture, string culture)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("ApplicationId cannot have an empty string for the name.");
		}
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		if (publicKeyToken == null)
		{
			throw new ArgumentNullException("publicKeyToken");
		}
		_publicKeyToken = (byte[])publicKeyToken.Clone();
		Name = name;
		Version = version;
		ProcessorArchitecture = processorArchitecture;
		Culture = culture;
	}

	/// <summary>Creates and returns an identical copy of the current application identity.</summary>
	/// <returns>An <see cref="T:System.ApplicationId" /> object that represents an exact copy of the original.</returns>
	/// <filterpriority>2</filterpriority>
	public ApplicationId Copy()
	{
		return new ApplicationId(_publicKeyToken, Name, Version, ProcessorArchitecture, Culture);
	}

	/// <summary>Creates and returns a string representation of the application identity.</summary>
	/// <returns>A string representation of the application identity.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		stringBuilder.Append(Name);
		if (Culture != null)
		{
			stringBuilder.Append(", culture=\"");
			stringBuilder.Append(Culture);
			stringBuilder.Append('"');
		}
		stringBuilder.Append(", version=\"");
		stringBuilder.Append(Version.ToString());
		stringBuilder.Append('"');
		if (_publicKeyToken != null)
		{
			stringBuilder.Append(", publicKeyToken=\"");
			stringBuilder.Append(EncodeHexString(_publicKeyToken));
			stringBuilder.Append('"');
		}
		if (ProcessorArchitecture != null)
		{
			stringBuilder.Append(", processorArchitecture =\"");
			stringBuilder.Append(ProcessorArchitecture);
			stringBuilder.Append('"');
		}
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	private static char HexDigit(int num)
	{
		return (char)((num < 10) ? (num + 48) : (num + 55));
	}

	private static string EncodeHexString(byte[] sArray)
	{
		string result = null;
		if (sArray != null)
		{
			char[] array = new char[sArray.Length * 2];
			int i = 0;
			int num = 0;
			for (; i < sArray.Length; i++)
			{
				int num2 = (sArray[i] & 0xF0) >> 4;
				array[num++] = HexDigit(num2);
				num2 = sArray[i] & 0xF;
				array[num++] = HexDigit(num2);
			}
			result = new string(array);
		}
		return result;
	}

	/// <summary>Determines whether the specified <see cref="T:System.ApplicationId" /> object is equivalent to the current <see cref="T:System.ApplicationId" />.</summary>
	/// <returns>true if the specified <see cref="T:System.ApplicationId" /> object is equivalent to the current <see cref="T:System.ApplicationId" />; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.ApplicationId" /> object to compare to the current <see cref="T:System.ApplicationId" />. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object o)
	{
		if (!(o is ApplicationId applicationId))
		{
			return false;
		}
		if (!object.Equals(Name, applicationId.Name) || !object.Equals(Version, applicationId.Version) || !object.Equals(ProcessorArchitecture, applicationId.ProcessorArchitecture) || !object.Equals(Culture, applicationId.Culture))
		{
			return false;
		}
		if (_publicKeyToken.Length != applicationId._publicKeyToken.Length)
		{
			return false;
		}
		for (int i = 0; i < _publicKeyToken.Length; i++)
		{
			if (_publicKeyToken[i] != applicationId._publicKeyToken[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Gets the hash code for the current application identity.</summary>
	/// <returns>The hash code for the current application identity.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return Name.GetHashCode() ^ Version.GetHashCode();
	}
}
