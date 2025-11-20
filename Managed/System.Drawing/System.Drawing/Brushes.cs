namespace System.Drawing;

/// <summary>Brushes for all the standard colors. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class Brushes
{
	private static SolidBrush aliceBlue;

	private static SolidBrush antiqueWhite;

	private static SolidBrush aqua;

	private static SolidBrush aquamarine;

	private static SolidBrush azure;

	private static SolidBrush beige;

	private static SolidBrush bisque;

	private static SolidBrush black;

	private static SolidBrush blanchedAlmond;

	private static SolidBrush blue;

	private static SolidBrush blueViolet;

	private static SolidBrush brown;

	private static SolidBrush burlyWood;

	private static SolidBrush cadetBlue;

	private static SolidBrush chartreuse;

	private static SolidBrush chocolate;

	private static SolidBrush coral;

	private static SolidBrush cornflowerBlue;

	private static SolidBrush cornsilk;

	private static SolidBrush crimson;

	private static SolidBrush cyan;

	private static SolidBrush darkBlue;

	private static SolidBrush darkCyan;

	private static SolidBrush darkGoldenrod;

	private static SolidBrush darkGray;

	private static SolidBrush darkGreen;

	private static SolidBrush darkKhaki;

	private static SolidBrush darkMagenta;

	private static SolidBrush darkOliveGreen;

	private static SolidBrush darkOrange;

	private static SolidBrush darkOrchid;

	private static SolidBrush darkRed;

	private static SolidBrush darkSalmon;

	private static SolidBrush darkSeaGreen;

	private static SolidBrush darkSlateBlue;

	private static SolidBrush darkSlateGray;

	private static SolidBrush darkTurquoise;

	private static SolidBrush darkViolet;

	private static SolidBrush deepPink;

	private static SolidBrush deepSkyBlue;

	private static SolidBrush dimGray;

	private static SolidBrush dodgerBlue;

	private static SolidBrush firebrick;

	private static SolidBrush floralWhite;

	private static SolidBrush forestGreen;

	private static SolidBrush fuchsia;

	private static SolidBrush gainsboro;

	private static SolidBrush ghostWhite;

	private static SolidBrush gold;

	private static SolidBrush goldenrod;

	private static SolidBrush gray;

	private static SolidBrush green;

	private static SolidBrush greenYellow;

	private static SolidBrush honeydew;

	private static SolidBrush hotPink;

	private static SolidBrush indianRed;

	private static SolidBrush indigo;

	private static SolidBrush ivory;

	private static SolidBrush khaki;

	private static SolidBrush lavender;

	private static SolidBrush lavenderBlush;

	private static SolidBrush lawnGreen;

	private static SolidBrush lemonChiffon;

	private static SolidBrush lightBlue;

	private static SolidBrush lightCoral;

	private static SolidBrush lightCyan;

	private static SolidBrush lightGoldenrodYellow;

	private static SolidBrush lightGray;

	private static SolidBrush lightGreen;

	private static SolidBrush lightPink;

	private static SolidBrush lightSalmon;

	private static SolidBrush lightSeaGreen;

	private static SolidBrush lightSkyBlue;

	private static SolidBrush lightSlateGray;

	private static SolidBrush lightSteelBlue;

	private static SolidBrush lightYellow;

	private static SolidBrush lime;

	private static SolidBrush limeGreen;

	private static SolidBrush linen;

	private static SolidBrush magenta;

	private static SolidBrush maroon;

	private static SolidBrush mediumAquamarine;

	private static SolidBrush mediumBlue;

	private static SolidBrush mediumOrchid;

	private static SolidBrush mediumPurple;

	private static SolidBrush mediumSeaGreen;

	private static SolidBrush mediumSlateBlue;

	private static SolidBrush mediumSpringGreen;

	private static SolidBrush mediumTurquoise;

	private static SolidBrush mediumVioletRed;

	private static SolidBrush midnightBlue;

	private static SolidBrush mintCream;

	private static SolidBrush mistyRose;

	private static SolidBrush moccasin;

	private static SolidBrush navajoWhite;

	private static SolidBrush navy;

	private static SolidBrush oldLace;

	private static SolidBrush olive;

	private static SolidBrush oliveDrab;

	private static SolidBrush orange;

	private static SolidBrush orangeRed;

	private static SolidBrush orchid;

	private static SolidBrush paleGoldenrod;

	private static SolidBrush paleGreen;

	private static SolidBrush paleTurquoise;

	private static SolidBrush paleVioletRed;

	private static SolidBrush papayaWhip;

	private static SolidBrush peachPuff;

	private static SolidBrush peru;

	private static SolidBrush pink;

	private static SolidBrush plum;

	private static SolidBrush powderBlue;

	private static SolidBrush purple;

	private static SolidBrush red;

	private static SolidBrush rosyBrown;

	private static SolidBrush royalBlue;

	private static SolidBrush saddleBrown;

	private static SolidBrush salmon;

	private static SolidBrush sandyBrown;

	private static SolidBrush seaGreen;

	private static SolidBrush seaShell;

	private static SolidBrush sienna;

	private static SolidBrush silver;

	private static SolidBrush skyBlue;

	private static SolidBrush slateBlue;

	private static SolidBrush slateGray;

	private static SolidBrush snow;

	private static SolidBrush springGreen;

	private static SolidBrush steelBlue;

	private static SolidBrush tan;

	private static SolidBrush teal;

	private static SolidBrush thistle;

	private static SolidBrush tomato;

	private static SolidBrush transparent;

	private static SolidBrush turquoise;

	private static SolidBrush violet;

	private static SolidBrush wheat;

	private static SolidBrush white;

	private static SolidBrush whiteSmoke;

	private static SolidBrush yellow;

	private static SolidBrush yellowGreen;

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush AliceBlue
	{
		get
		{
			if (aliceBlue == null)
			{
				aliceBlue = new SolidBrush(Color.AliceBlue);
			}
			return aliceBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush AntiqueWhite
	{
		get
		{
			if (antiqueWhite == null)
			{
				antiqueWhite = new SolidBrush(Color.AntiqueWhite);
			}
			return antiqueWhite;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Aqua
	{
		get
		{
			if (aqua == null)
			{
				aqua = new SolidBrush(Color.Aqua);
			}
			return aqua;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Aquamarine
	{
		get
		{
			if (aquamarine == null)
			{
				aquamarine = new SolidBrush(Color.Aquamarine);
			}
			return aquamarine;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Azure
	{
		get
		{
			if (azure == null)
			{
				azure = new SolidBrush(Color.Azure);
			}
			return azure;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Beige
	{
		get
		{
			if (beige == null)
			{
				beige = new SolidBrush(Color.Beige);
			}
			return beige;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Bisque
	{
		get
		{
			if (bisque == null)
			{
				bisque = new SolidBrush(Color.Bisque);
			}
			return bisque;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Black
	{
		get
		{
			if (black == null)
			{
				black = new SolidBrush(Color.Black);
			}
			return black;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush BlanchedAlmond
	{
		get
		{
			if (blanchedAlmond == null)
			{
				blanchedAlmond = new SolidBrush(Color.BlanchedAlmond);
			}
			return blanchedAlmond;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Blue
	{
		get
		{
			if (blue == null)
			{
				blue = new SolidBrush(Color.Blue);
			}
			return blue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush BlueViolet
	{
		get
		{
			if (blueViolet == null)
			{
				blueViolet = new SolidBrush(Color.BlueViolet);
			}
			return blueViolet;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Brown
	{
		get
		{
			if (brown == null)
			{
				brown = new SolidBrush(Color.Brown);
			}
			return brown;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush BurlyWood
	{
		get
		{
			if (burlyWood == null)
			{
				burlyWood = new SolidBrush(Color.BurlyWood);
			}
			return burlyWood;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush CadetBlue
	{
		get
		{
			if (cadetBlue == null)
			{
				cadetBlue = new SolidBrush(Color.CadetBlue);
			}
			return cadetBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Chartreuse
	{
		get
		{
			if (chartreuse == null)
			{
				chartreuse = new SolidBrush(Color.Chartreuse);
			}
			return chartreuse;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Chocolate
	{
		get
		{
			if (chocolate == null)
			{
				chocolate = new SolidBrush(Color.Chocolate);
			}
			return chocolate;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Coral
	{
		get
		{
			if (coral == null)
			{
				coral = new SolidBrush(Color.Coral);
			}
			return coral;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush CornflowerBlue
	{
		get
		{
			if (cornflowerBlue == null)
			{
				cornflowerBlue = new SolidBrush(Color.CornflowerBlue);
			}
			return cornflowerBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Cornsilk
	{
		get
		{
			if (cornsilk == null)
			{
				cornsilk = new SolidBrush(Color.Cornsilk);
			}
			return cornsilk;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Crimson
	{
		get
		{
			if (crimson == null)
			{
				crimson = new SolidBrush(Color.Crimson);
			}
			return crimson;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Cyan
	{
		get
		{
			if (cyan == null)
			{
				cyan = new SolidBrush(Color.Cyan);
			}
			return cyan;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkBlue
	{
		get
		{
			if (darkBlue == null)
			{
				darkBlue = new SolidBrush(Color.DarkBlue);
			}
			return darkBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkCyan
	{
		get
		{
			if (darkCyan == null)
			{
				darkCyan = new SolidBrush(Color.DarkCyan);
			}
			return darkCyan;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkGoldenrod
	{
		get
		{
			if (darkGoldenrod == null)
			{
				darkGoldenrod = new SolidBrush(Color.DarkGoldenrod);
			}
			return darkGoldenrod;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkGray
	{
		get
		{
			if (darkGray == null)
			{
				darkGray = new SolidBrush(Color.DarkGray);
			}
			return darkGray;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkGreen
	{
		get
		{
			if (darkGreen == null)
			{
				darkGreen = new SolidBrush(Color.DarkGreen);
			}
			return darkGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkKhaki
	{
		get
		{
			if (darkKhaki == null)
			{
				darkKhaki = new SolidBrush(Color.DarkKhaki);
			}
			return darkKhaki;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkMagenta
	{
		get
		{
			if (darkMagenta == null)
			{
				darkMagenta = new SolidBrush(Color.DarkMagenta);
			}
			return darkMagenta;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkOliveGreen
	{
		get
		{
			if (darkOliveGreen == null)
			{
				darkOliveGreen = new SolidBrush(Color.DarkOliveGreen);
			}
			return darkOliveGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkOrange
	{
		get
		{
			if (darkOrange == null)
			{
				darkOrange = new SolidBrush(Color.DarkOrange);
			}
			return darkOrange;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkOrchid
	{
		get
		{
			if (darkOrchid == null)
			{
				darkOrchid = new SolidBrush(Color.DarkOrchid);
			}
			return darkOrchid;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkRed
	{
		get
		{
			if (darkRed == null)
			{
				darkRed = new SolidBrush(Color.DarkRed);
			}
			return darkRed;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkSalmon
	{
		get
		{
			if (darkSalmon == null)
			{
				darkSalmon = new SolidBrush(Color.DarkSalmon);
			}
			return darkSalmon;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkSeaGreen
	{
		get
		{
			if (darkSeaGreen == null)
			{
				darkSeaGreen = new SolidBrush(Color.DarkSeaGreen);
			}
			return darkSeaGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkSlateBlue
	{
		get
		{
			if (darkSlateBlue == null)
			{
				darkSlateBlue = new SolidBrush(Color.DarkSlateBlue);
			}
			return darkSlateBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkSlateGray
	{
		get
		{
			if (darkSlateGray == null)
			{
				darkSlateGray = new SolidBrush(Color.DarkSlateGray);
			}
			return darkSlateGray;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkTurquoise
	{
		get
		{
			if (darkTurquoise == null)
			{
				darkTurquoise = new SolidBrush(Color.DarkTurquoise);
			}
			return darkTurquoise;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DarkViolet
	{
		get
		{
			if (darkViolet == null)
			{
				darkViolet = new SolidBrush(Color.DarkViolet);
			}
			return darkViolet;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DeepPink
	{
		get
		{
			if (deepPink == null)
			{
				deepPink = new SolidBrush(Color.DeepPink);
			}
			return deepPink;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DeepSkyBlue
	{
		get
		{
			if (deepSkyBlue == null)
			{
				deepSkyBlue = new SolidBrush(Color.DeepSkyBlue);
			}
			return deepSkyBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DimGray
	{
		get
		{
			if (dimGray == null)
			{
				dimGray = new SolidBrush(Color.DimGray);
			}
			return dimGray;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush DodgerBlue
	{
		get
		{
			if (dodgerBlue == null)
			{
				dodgerBlue = new SolidBrush(Color.DodgerBlue);
			}
			return dodgerBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Firebrick
	{
		get
		{
			if (firebrick == null)
			{
				firebrick = new SolidBrush(Color.Firebrick);
			}
			return firebrick;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush FloralWhite
	{
		get
		{
			if (floralWhite == null)
			{
				floralWhite = new SolidBrush(Color.FloralWhite);
			}
			return floralWhite;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush ForestGreen
	{
		get
		{
			if (forestGreen == null)
			{
				forestGreen = new SolidBrush(Color.ForestGreen);
			}
			return forestGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Fuchsia
	{
		get
		{
			if (fuchsia == null)
			{
				fuchsia = new SolidBrush(Color.Fuchsia);
			}
			return fuchsia;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Gainsboro
	{
		get
		{
			if (gainsboro == null)
			{
				gainsboro = new SolidBrush(Color.Gainsboro);
			}
			return gainsboro;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush GhostWhite
	{
		get
		{
			if (ghostWhite == null)
			{
				ghostWhite = new SolidBrush(Color.GhostWhite);
			}
			return ghostWhite;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Gold
	{
		get
		{
			if (gold == null)
			{
				gold = new SolidBrush(Color.Gold);
			}
			return gold;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Goldenrod
	{
		get
		{
			if (goldenrod == null)
			{
				goldenrod = new SolidBrush(Color.Goldenrod);
			}
			return goldenrod;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Gray
	{
		get
		{
			if (gray == null)
			{
				gray = new SolidBrush(Color.Gray);
			}
			return gray;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Green
	{
		get
		{
			if (green == null)
			{
				green = new SolidBrush(Color.Green);
			}
			return green;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush GreenYellow
	{
		get
		{
			if (greenYellow == null)
			{
				greenYellow = new SolidBrush(Color.GreenYellow);
			}
			return greenYellow;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Honeydew
	{
		get
		{
			if (honeydew == null)
			{
				honeydew = new SolidBrush(Color.Honeydew);
			}
			return honeydew;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush HotPink
	{
		get
		{
			if (hotPink == null)
			{
				hotPink = new SolidBrush(Color.HotPink);
			}
			return hotPink;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush IndianRed
	{
		get
		{
			if (indianRed == null)
			{
				indianRed = new SolidBrush(Color.IndianRed);
			}
			return indianRed;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Indigo
	{
		get
		{
			if (indigo == null)
			{
				indigo = new SolidBrush(Color.Indigo);
			}
			return indigo;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Ivory
	{
		get
		{
			if (ivory == null)
			{
				ivory = new SolidBrush(Color.Ivory);
			}
			return ivory;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Khaki
	{
		get
		{
			if (khaki == null)
			{
				khaki = new SolidBrush(Color.Khaki);
			}
			return khaki;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Lavender
	{
		get
		{
			if (lavender == null)
			{
				lavender = new SolidBrush(Color.Lavender);
			}
			return lavender;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LavenderBlush
	{
		get
		{
			if (lavenderBlush == null)
			{
				lavenderBlush = new SolidBrush(Color.LavenderBlush);
			}
			return lavenderBlush;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LawnGreen
	{
		get
		{
			if (lawnGreen == null)
			{
				lawnGreen = new SolidBrush(Color.LawnGreen);
			}
			return lawnGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LemonChiffon
	{
		get
		{
			if (lemonChiffon == null)
			{
				lemonChiffon = new SolidBrush(Color.LemonChiffon);
			}
			return lemonChiffon;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightBlue
	{
		get
		{
			if (lightBlue == null)
			{
				lightBlue = new SolidBrush(Color.LightBlue);
			}
			return lightBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightCoral
	{
		get
		{
			if (lightCoral == null)
			{
				lightCoral = new SolidBrush(Color.LightCoral);
			}
			return lightCoral;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightCyan
	{
		get
		{
			if (lightCyan == null)
			{
				lightCyan = new SolidBrush(Color.LightCyan);
			}
			return lightCyan;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightGoldenrodYellow
	{
		get
		{
			if (lightGoldenrodYellow == null)
			{
				lightGoldenrodYellow = new SolidBrush(Color.LightGoldenrodYellow);
			}
			return lightGoldenrodYellow;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightGray
	{
		get
		{
			if (lightGray == null)
			{
				lightGray = new SolidBrush(Color.LightGray);
			}
			return lightGray;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightGreen
	{
		get
		{
			if (lightGreen == null)
			{
				lightGreen = new SolidBrush(Color.LightGreen);
			}
			return lightGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightPink
	{
		get
		{
			if (lightPink == null)
			{
				lightPink = new SolidBrush(Color.LightPink);
			}
			return lightPink;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightSalmon
	{
		get
		{
			if (lightSalmon == null)
			{
				lightSalmon = new SolidBrush(Color.LightSalmon);
			}
			return lightSalmon;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightSeaGreen
	{
		get
		{
			if (lightSeaGreen == null)
			{
				lightSeaGreen = new SolidBrush(Color.LightSeaGreen);
			}
			return lightSeaGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightSkyBlue
	{
		get
		{
			if (lightSkyBlue == null)
			{
				lightSkyBlue = new SolidBrush(Color.LightSkyBlue);
			}
			return lightSkyBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightSlateGray
	{
		get
		{
			if (lightSlateGray == null)
			{
				lightSlateGray = new SolidBrush(Color.LightSlateGray);
			}
			return lightSlateGray;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightSteelBlue
	{
		get
		{
			if (lightSteelBlue == null)
			{
				lightSteelBlue = new SolidBrush(Color.LightSteelBlue);
			}
			return lightSteelBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LightYellow
	{
		get
		{
			if (lightYellow == null)
			{
				lightYellow = new SolidBrush(Color.LightYellow);
			}
			return lightYellow;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Lime
	{
		get
		{
			if (lime == null)
			{
				lime = new SolidBrush(Color.Lime);
			}
			return lime;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush LimeGreen
	{
		get
		{
			if (limeGreen == null)
			{
				limeGreen = new SolidBrush(Color.LimeGreen);
			}
			return limeGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Linen
	{
		get
		{
			if (linen == null)
			{
				linen = new SolidBrush(Color.Linen);
			}
			return linen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Magenta
	{
		get
		{
			if (magenta == null)
			{
				magenta = new SolidBrush(Color.Magenta);
			}
			return magenta;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Maroon
	{
		get
		{
			if (maroon == null)
			{
				maroon = new SolidBrush(Color.Maroon);
			}
			return maroon;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumAquamarine
	{
		get
		{
			if (mediumAquamarine == null)
			{
				mediumAquamarine = new SolidBrush(Color.MediumAquamarine);
			}
			return mediumAquamarine;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumBlue
	{
		get
		{
			if (mediumBlue == null)
			{
				mediumBlue = new SolidBrush(Color.MediumBlue);
			}
			return mediumBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumOrchid
	{
		get
		{
			if (mediumOrchid == null)
			{
				mediumOrchid = new SolidBrush(Color.MediumOrchid);
			}
			return mediumOrchid;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumPurple
	{
		get
		{
			if (mediumPurple == null)
			{
				mediumPurple = new SolidBrush(Color.MediumPurple);
			}
			return mediumPurple;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumSeaGreen
	{
		get
		{
			if (mediumSeaGreen == null)
			{
				mediumSeaGreen = new SolidBrush(Color.MediumSeaGreen);
			}
			return mediumSeaGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumSlateBlue
	{
		get
		{
			if (mediumSlateBlue == null)
			{
				mediumSlateBlue = new SolidBrush(Color.MediumSlateBlue);
			}
			return mediumSlateBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumSpringGreen
	{
		get
		{
			if (mediumSpringGreen == null)
			{
				mediumSpringGreen = new SolidBrush(Color.MediumSpringGreen);
			}
			return mediumSpringGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumTurquoise
	{
		get
		{
			if (mediumTurquoise == null)
			{
				mediumTurquoise = new SolidBrush(Color.MediumTurquoise);
			}
			return mediumTurquoise;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MediumVioletRed
	{
		get
		{
			if (mediumVioletRed == null)
			{
				mediumVioletRed = new SolidBrush(Color.MediumVioletRed);
			}
			return mediumVioletRed;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MidnightBlue
	{
		get
		{
			if (midnightBlue == null)
			{
				midnightBlue = new SolidBrush(Color.MidnightBlue);
			}
			return midnightBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MintCream
	{
		get
		{
			if (mintCream == null)
			{
				mintCream = new SolidBrush(Color.MintCream);
			}
			return mintCream;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush MistyRose
	{
		get
		{
			if (mistyRose == null)
			{
				mistyRose = new SolidBrush(Color.MistyRose);
			}
			return mistyRose;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Moccasin
	{
		get
		{
			if (moccasin == null)
			{
				moccasin = new SolidBrush(Color.Moccasin);
			}
			return moccasin;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush NavajoWhite
	{
		get
		{
			if (navajoWhite == null)
			{
				navajoWhite = new SolidBrush(Color.NavajoWhite);
			}
			return navajoWhite;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Navy
	{
		get
		{
			if (navy == null)
			{
				navy = new SolidBrush(Color.Navy);
			}
			return navy;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush OldLace
	{
		get
		{
			if (oldLace == null)
			{
				oldLace = new SolidBrush(Color.OldLace);
			}
			return oldLace;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Olive
	{
		get
		{
			if (olive == null)
			{
				olive = new SolidBrush(Color.Olive);
			}
			return olive;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush OliveDrab
	{
		get
		{
			if (oliveDrab == null)
			{
				oliveDrab = new SolidBrush(Color.OliveDrab);
			}
			return oliveDrab;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Orange
	{
		get
		{
			if (orange == null)
			{
				orange = new SolidBrush(Color.Orange);
			}
			return orange;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush OrangeRed
	{
		get
		{
			if (orangeRed == null)
			{
				orangeRed = new SolidBrush(Color.OrangeRed);
			}
			return orangeRed;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Orchid
	{
		get
		{
			if (orchid == null)
			{
				orchid = new SolidBrush(Color.Orchid);
			}
			return orchid;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush PaleGoldenrod
	{
		get
		{
			if (paleGoldenrod == null)
			{
				paleGoldenrod = new SolidBrush(Color.PaleGoldenrod);
			}
			return paleGoldenrod;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush PaleGreen
	{
		get
		{
			if (paleGreen == null)
			{
				paleGreen = new SolidBrush(Color.PaleGreen);
			}
			return paleGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush PaleTurquoise
	{
		get
		{
			if (paleTurquoise == null)
			{
				paleTurquoise = new SolidBrush(Color.PaleTurquoise);
			}
			return paleTurquoise;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush PaleVioletRed
	{
		get
		{
			if (paleVioletRed == null)
			{
				paleVioletRed = new SolidBrush(Color.PaleVioletRed);
			}
			return paleVioletRed;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush PapayaWhip
	{
		get
		{
			if (papayaWhip == null)
			{
				papayaWhip = new SolidBrush(Color.PapayaWhip);
			}
			return papayaWhip;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush PeachPuff
	{
		get
		{
			if (peachPuff == null)
			{
				peachPuff = new SolidBrush(Color.PeachPuff);
			}
			return peachPuff;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Peru
	{
		get
		{
			if (peru == null)
			{
				peru = new SolidBrush(Color.Peru);
			}
			return peru;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Pink
	{
		get
		{
			if (pink == null)
			{
				pink = new SolidBrush(Color.Pink);
			}
			return pink;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Plum
	{
		get
		{
			if (plum == null)
			{
				plum = new SolidBrush(Color.Plum);
			}
			return plum;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush PowderBlue
	{
		get
		{
			if (powderBlue == null)
			{
				powderBlue = new SolidBrush(Color.PowderBlue);
			}
			return powderBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Purple
	{
		get
		{
			if (purple == null)
			{
				purple = new SolidBrush(Color.Purple);
			}
			return purple;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Red
	{
		get
		{
			if (red == null)
			{
				red = new SolidBrush(Color.Red);
			}
			return red;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush RosyBrown
	{
		get
		{
			if (rosyBrown == null)
			{
				rosyBrown = new SolidBrush(Color.RosyBrown);
			}
			return rosyBrown;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush RoyalBlue
	{
		get
		{
			if (royalBlue == null)
			{
				royalBlue = new SolidBrush(Color.RoyalBlue);
			}
			return royalBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SaddleBrown
	{
		get
		{
			if (saddleBrown == null)
			{
				saddleBrown = new SolidBrush(Color.SaddleBrown);
			}
			return saddleBrown;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Salmon
	{
		get
		{
			if (salmon == null)
			{
				salmon = new SolidBrush(Color.Salmon);
			}
			return salmon;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SandyBrown
	{
		get
		{
			if (sandyBrown == null)
			{
				sandyBrown = new SolidBrush(Color.SandyBrown);
			}
			return sandyBrown;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SeaGreen
	{
		get
		{
			if (seaGreen == null)
			{
				seaGreen = new SolidBrush(Color.SeaGreen);
			}
			return seaGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SeaShell
	{
		get
		{
			if (seaShell == null)
			{
				seaShell = new SolidBrush(Color.SeaShell);
			}
			return seaShell;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Sienna
	{
		get
		{
			if (sienna == null)
			{
				sienna = new SolidBrush(Color.Sienna);
			}
			return sienna;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Silver
	{
		get
		{
			if (silver == null)
			{
				silver = new SolidBrush(Color.Silver);
			}
			return silver;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SkyBlue
	{
		get
		{
			if (skyBlue == null)
			{
				skyBlue = new SolidBrush(Color.SkyBlue);
			}
			return skyBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SlateBlue
	{
		get
		{
			if (slateBlue == null)
			{
				slateBlue = new SolidBrush(Color.SlateBlue);
			}
			return slateBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SlateGray
	{
		get
		{
			if (slateGray == null)
			{
				slateGray = new SolidBrush(Color.SlateGray);
			}
			return slateGray;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Snow
	{
		get
		{
			if (snow == null)
			{
				snow = new SolidBrush(Color.Snow);
			}
			return snow;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SpringGreen
	{
		get
		{
			if (springGreen == null)
			{
				springGreen = new SolidBrush(Color.SpringGreen);
			}
			return springGreen;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush SteelBlue
	{
		get
		{
			if (steelBlue == null)
			{
				steelBlue = new SolidBrush(Color.SteelBlue);
			}
			return steelBlue;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Tan
	{
		get
		{
			if (tan == null)
			{
				tan = new SolidBrush(Color.Tan);
			}
			return tan;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Teal
	{
		get
		{
			if (teal == null)
			{
				teal = new SolidBrush(Color.Teal);
			}
			return teal;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Thistle
	{
		get
		{
			if (thistle == null)
			{
				thistle = new SolidBrush(Color.Thistle);
			}
			return thistle;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Tomato
	{
		get
		{
			if (tomato == null)
			{
				tomato = new SolidBrush(Color.Tomato);
			}
			return tomato;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Transparent
	{
		get
		{
			if (transparent == null)
			{
				transparent = new SolidBrush(Color.Transparent);
			}
			return transparent;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Turquoise
	{
		get
		{
			if (turquoise == null)
			{
				turquoise = new SolidBrush(Color.Turquoise);
			}
			return turquoise;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Violet
	{
		get
		{
			if (violet == null)
			{
				violet = new SolidBrush(Color.Violet);
			}
			return violet;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Wheat
	{
		get
		{
			if (wheat == null)
			{
				wheat = new SolidBrush(Color.Wheat);
			}
			return wheat;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush White
	{
		get
		{
			if (white == null)
			{
				white = new SolidBrush(Color.White);
			}
			return white;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush WhiteSmoke
	{
		get
		{
			if (whiteSmoke == null)
			{
				whiteSmoke = new SolidBrush(Color.WhiteSmoke);
			}
			return whiteSmoke;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush Yellow
	{
		get
		{
			if (yellow == null)
			{
				yellow = new SolidBrush(Color.Yellow);
			}
			return yellow;
		}
	}

	/// <summary>Gets a system-defined <see cref="T:System.Drawing.Brush" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Brush YellowGreen
	{
		get
		{
			if (yellowGreen == null)
			{
				yellowGreen = new SolidBrush(Color.YellowGreen);
			}
			return yellowGreen;
		}
	}

	private Brushes()
	{
	}
}
