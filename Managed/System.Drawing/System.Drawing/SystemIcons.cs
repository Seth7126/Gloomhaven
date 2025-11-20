namespace System.Drawing;

/// <summary>Each property of the <see cref="T:System.Drawing.SystemIcons" /> class is an <see cref="T:System.Drawing.Icon" /> object for Windows system-wide icons. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class SystemIcons
{
	private static Icon[] icons;

	private const int Application_Winlogo = 0;

	private const int Asterisk_Information = 1;

	private const int Error_Hand = 2;

	private const int Exclamation_Warning = 3;

	private const int Question_ = 4;

	private const int Shield_ = 5;

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the default application icon (WIN32: IDI_APPLICATION).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the default application icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon Application => icons[0];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the system asterisk icon (WIN32: IDI_ASTERISK).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the system asterisk icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon Asterisk => icons[1];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the system error icon (WIN32: IDI_ERROR).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the system error icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon Error => icons[2];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the system exclamation icon (WIN32: IDI_EXCLAMATION).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the system exclamation icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon Exclamation => icons[3];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the system hand icon (WIN32: IDI_HAND).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the system hand icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon Hand => icons[2];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the system information icon (WIN32: IDI_INFORMATION).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the system information icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon Information => icons[1];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the system question icon (WIN32: IDI_QUESTION).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the system question icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon Question => icons[4];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the system warning icon (WIN32: IDI_WARNING).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the system warning icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon Warning => icons[3];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the Windows logo icon (WIN32: IDI_WINLOGO).</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the Windows logo icon.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Icon WinLogo => icons[0];

	/// <summary>Gets an <see cref="T:System.Drawing.Icon" /> object that contains the shield icon.</summary>
	/// <returns>An <see cref="T:System.Drawing.Icon" /> object that contains the shield icon.</returns>
	public static Icon Shield => icons[5];

	static SystemIcons()
	{
		icons = new Icon[6];
		icons[0] = new Icon("Mono.ico", undisposable: true);
		icons[1] = new Icon("Information.ico", undisposable: true);
		icons[2] = new Icon("Error.ico", undisposable: true);
		icons[3] = new Icon("Warning.ico", undisposable: true);
		icons[4] = new Icon("Question.ico", undisposable: true);
		icons[5] = new Icon("Shield.ico", undisposable: true);
	}

	private SystemIcons()
	{
	}
}
