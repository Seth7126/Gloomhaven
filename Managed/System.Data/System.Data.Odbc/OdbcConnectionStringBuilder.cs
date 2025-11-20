using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;

namespace System.Data.Odbc;

/// <summary>Provides a simple way to create and manage the contents of connection strings used by the <see cref="T:System.Data.Odbc.OdbcConnection" /> class.</summary>
/// <filterpriority>2</filterpriority>
public sealed class OdbcConnectionStringBuilder : DbConnectionStringBuilder
{
	private enum Keywords
	{
		Dsn,
		Driver
	}

	private static readonly string[] s_validKeywords;

	private static readonly Dictionary<string, Keywords> s_keywords;

	private string[] _knownKeywords;

	private string _dsn = "";

	private string _driver = "";

	/// <summary>Gets or sets the value associated with the specified key. In C#, this property is the indexer.</summary>
	/// <returns>The value associated with the specified key.</returns>
	/// <param name="keyword">The key of the item to get or set.</param>
	/// <exception cref="T:System.ArgumentException">The connection string is incorrectly formatted (perhaps missing the required "=" within a key/value pair).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object this[string keyword]
	{
		get
		{
			ADP.CheckArgumentNull(keyword, "keyword");
			if (s_keywords.TryGetValue(keyword, out var value))
			{
				return GetAt(value);
			}
			return base[keyword];
		}
		set
		{
			ADP.CheckArgumentNull(keyword, "keyword");
			if (value != null)
			{
				if (s_keywords.TryGetValue(keyword, out var value2))
				{
					switch (value2)
					{
					case Keywords.Driver:
						Driver = ConvertToString(value);
						break;
					case Keywords.Dsn:
						Dsn = ConvertToString(value);
						break;
					default:
						throw ADP.KeywordNotSupported(keyword);
					}
				}
				else
				{
					base[keyword] = value;
					ClearPropertyDescriptors();
					_knownKeywords = null;
				}
			}
			else
			{
				Remove(keyword);
			}
		}
	}

	/// <summary>Gets or sets the name of the ODBC driver associated with the connection.</summary>
	/// <returns>The value of the <see cref="P:System.Data.Odbc.OdbcConnectionStringBuilder.Driver" /> property, or String.Empty if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[DisplayName("Driver")]
	public string Driver
	{
		get
		{
			return _driver;
		}
		set
		{
			SetValue("Driver", value);
			_driver = value;
		}
	}

	/// <summary>Gets or sets the name of the data source name (DSN) associated with the connection.</summary>
	/// <returns>The value of the <see cref="P:System.Data.Odbc.OdbcConnectionStringBuilder.Dsn" /> property, or String.Empty if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[DisplayName("Dsn")]
	public string Dsn
	{
		get
		{
			return _dsn;
		}
		set
		{
			SetValue("Dsn", value);
			_dsn = value;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> that contains the keys in the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that contains the keys in the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override ICollection Keys
	{
		get
		{
			string[] array = _knownKeywords;
			if (array == null)
			{
				array = s_validKeywords;
				int num = 0;
				foreach (string key in base.Keys)
				{
					bool flag = true;
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						if (array2[i] == key)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						num++;
					}
				}
				if (0 < num)
				{
					string[] array3 = new string[array.Length + num];
					array.CopyTo(array3, 0);
					int num2 = array.Length;
					foreach (string key2 in base.Keys)
					{
						bool flag2 = true;
						string[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							if (array2[i] == key2)
							{
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							array3[num2++] = key2;
						}
					}
					array = array3;
				}
				_knownKeywords = array;
			}
			return new ReadOnlyCollection<string>(array);
		}
	}

	static OdbcConnectionStringBuilder()
	{
		string[] array = new string[2];
		array[1] = "Driver";
		array[0] = "Dsn";
		s_validKeywords = array;
		s_keywords = new Dictionary<string, Keywords>(2, StringComparer.OrdinalIgnoreCase)
		{
			{
				"Driver",
				Keywords.Driver
			},
			{
				"Dsn",
				Keywords.Dsn
			}
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" /> class.</summary>
	public OdbcConnectionStringBuilder()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" /> class. The provided connection string provides the data for the instance's internal connection information.</summary>
	/// <param name="connectionString">The basis for the object's internal connection information. Parsed into key/value pairs.</param>
	/// <exception cref="T:System.ArgumentException">The connection string is incorrectly formatted (perhaps missing the required "=" within a key/value pair).</exception>
	public OdbcConnectionStringBuilder(string connectionString)
		: base(useOdbcRules: true)
	{
		if (!string.IsNullOrEmpty(connectionString))
		{
			base.ConnectionString = connectionString;
		}
	}

	/// <summary>Clears the contents of the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" /> instance.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override void Clear()
	{
		base.Clear();
		for (int i = 0; i < s_validKeywords.Length; i++)
		{
			Reset((Keywords)i);
		}
		_knownKeywords = s_validKeywords;
	}

	/// <summary>Determines whether the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" /> contains an element that has the specified key; otherwise false.</returns>
	/// <param name="keyword">The key to locate in the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is null (Nothing in Visual Basic).</exception>
	/// <filterpriority>1</filterpriority>
	public override bool ContainsKey(string keyword)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		if (!s_keywords.ContainsKey(keyword))
		{
			return base.ContainsKey(keyword);
		}
		return true;
	}

	private static string ConvertToString(object value)
	{
		return DbConnectionStringBuilderUtil.ConvertToString(value);
	}

	private object GetAt(Keywords index)
	{
		return index switch
		{
			Keywords.Driver => Driver, 
			Keywords.Dsn => Dsn, 
			_ => throw ADP.KeywordNotSupported(s_validKeywords[(int)index]), 
		};
	}

	/// <summary>Removes the entry with the specified key from the <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" /> instance.</summary>
	/// <returns>true if the key existed within the connection string and was removed; false if the key did not exist.</returns>
	/// <param name="keyword">The key of the key/value pair to be removed from the connection string in this <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is null (Nothing in Visual Basic).</exception>
	/// <filterpriority>1</filterpriority>
	public override bool Remove(string keyword)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		if (base.Remove(keyword))
		{
			if (s_keywords.TryGetValue(keyword, out var value))
			{
				Reset(value);
			}
			else
			{
				ClearPropertyDescriptors();
				_knownKeywords = null;
			}
			return true;
		}
		return false;
	}

	private void Reset(Keywords index)
	{
		switch (index)
		{
		case Keywords.Driver:
			_driver = "";
			break;
		case Keywords.Dsn:
			_dsn = "";
			break;
		default:
			throw ADP.KeywordNotSupported(s_validKeywords[(int)index]);
		}
	}

	private void SetValue(string keyword, string value)
	{
		ADP.CheckArgumentNull(value, keyword);
		base[keyword] = value;
	}

	/// <summary>Retrieves a value corresponding to the supplied key from this <see cref="T:System.Data.Odbc.OdbcConnectionStringBuilder" />.</summary>
	/// <returns>true if <paramref name="keyword" /> was found within the connection string; otherwise false.</returns>
	/// <param name="keyword">The key of the item to retrieve.</param>
	/// <param name="value">The value corresponding to <paramref name="keyword." /></param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool TryGetValue(string keyword, out object value)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		if (s_keywords.TryGetValue(keyword, out var value2))
		{
			value = GetAt(value2);
			return true;
		}
		return base.TryGetValue(keyword, out value);
	}
}
