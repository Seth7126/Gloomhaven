using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents information about an operating system, such as the version and platform identifier. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class OperatingSystem : ISerializable, ICloneable
{
	private readonly Version _version;

	private readonly PlatformID _platform;

	private readonly string _servicePack;

	private string _versionString;

	/// <summary>Gets a <see cref="T:System.PlatformID" /> enumeration value that identifies the operating system platform.</summary>
	/// <returns>One of the <see cref="T:System.PlatformID" /> values.</returns>
	/// <filterpriority>2</filterpriority>
	public PlatformID Platform => _platform;

	/// <summary>Gets the service pack version represented by this <see cref="T:System.OperatingSystem" /> object.</summary>
	/// <returns>The service pack version, if service packs are supported and at least one is installed; otherwise, an empty string (""). </returns>
	/// <filterpriority>2</filterpriority>
	public string ServicePack => _servicePack ?? string.Empty;

	/// <summary>Gets a <see cref="T:System.Version" /> object that identifies the operating system.</summary>
	/// <returns>A <see cref="T:System.Version" /> object that describes the major version, minor version, build, and revision numbers for the operating system.</returns>
	/// <filterpriority>2</filterpriority>
	public Version Version => _version;

	/// <summary>Gets the concatenated string representation of the platform identifier, version, and service pack that are currently installed on the operating system. </summary>
	/// <returns>The string representation of the values returned by the <see cref="P:System.OperatingSystem.Platform" />, <see cref="P:System.OperatingSystem.Version" />, and <see cref="P:System.OperatingSystem.ServicePack" /> properties.</returns>
	/// <filterpriority>2</filterpriority>
	public string VersionString
	{
		get
		{
			if (_versionString == null)
			{
				string text = _platform switch
				{
					PlatformID.Win32S => "Microsoft Win32S ", 
					PlatformID.Win32Windows => (_version.Major > 4 || (_version.Major == 4 && _version.Minor > 0)) ? "Microsoft Windows 98 " : "Microsoft Windows 95 ", 
					PlatformID.Win32NT => "Microsoft Windows NT ", 
					PlatformID.WinCE => "Microsoft Windows CE ", 
					PlatformID.Unix => "Unix ", 
					PlatformID.Xbox => "Xbox ", 
					PlatformID.MacOSX => "Mac OS X ", 
					_ => "<unknown> ", 
				};
				_versionString = (string.IsNullOrEmpty(_servicePack) ? (text + _version.ToString()) : (text + _version.ToString(3) + " " + _servicePack));
			}
			return _versionString;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.OperatingSystem" /> class, using the specified platform identifier value and version object.</summary>
	/// <param name="platform">One of the <see cref="T:System.PlatformID" /> values that indicates the operating system platform. </param>
	/// <param name="version">A <see cref="T:System.Version" /> object that indicates the version of the operating system. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="version" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="platform" /> is not a <see cref="T:System.PlatformID" /> enumeration value.</exception>
	public OperatingSystem(PlatformID platform, Version version)
		: this(platform, version, null)
	{
	}

	internal OperatingSystem(PlatformID platform, Version version, string servicePack)
	{
		if (platform < PlatformID.Win32S || platform > PlatformID.MacOSX)
		{
			throw new ArgumentOutOfRangeException("platform", platform, SR.Format("Illegal enum value: {0}.", platform));
		}
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		_platform = platform;
		_version = version;
		_servicePack = servicePack;
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data necessary to deserialize this instance.</summary>
	/// <param name="info">The object to populate with serialization information.</param>
	/// <param name="context">The place to store and retrieve serialized data. Reserved for future use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new PlatformNotSupportedException();
	}

	/// <summary>Creates an <see cref="T:System.OperatingSystem" /> object that is identical to this instance.</summary>
	/// <returns>An <see cref="T:System.OperatingSystem" /> object that is a copy of this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new OperatingSystem(_platform, _version, _servicePack);
	}

	/// <summary>Converts the value of this <see cref="T:System.OperatingSystem" /> object to its equivalent string representation.</summary>
	/// <returns>The string representation of the values returned by the <see cref="P:System.OperatingSystem.Platform" />, <see cref="P:System.OperatingSystem.Version" />, and <see cref="P:System.OperatingSystem.ServicePack" /> properties.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return VersionString;
	}
}
