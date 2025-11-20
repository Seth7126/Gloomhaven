namespace System.Drawing;

/// <summary>Pens for all the standard colors. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class Pens
{
	private static Pen aliceblue;

	private static Pen antiquewhite;

	private static Pen aqua;

	private static Pen aquamarine;

	private static Pen azure;

	private static Pen beige;

	private static Pen bisque;

	private static Pen black;

	private static Pen blanchedalmond;

	private static Pen blue;

	private static Pen blueviolet;

	private static Pen brown;

	private static Pen burlywood;

	private static Pen cadetblue;

	private static Pen chartreuse;

	private static Pen chocolate;

	private static Pen coral;

	private static Pen cornflowerblue;

	private static Pen cornsilk;

	private static Pen crimson;

	private static Pen cyan;

	private static Pen darkblue;

	private static Pen darkcyan;

	private static Pen darkgoldenrod;

	private static Pen darkgray;

	private static Pen darkgreen;

	private static Pen darkkhaki;

	private static Pen darkmagenta;

	private static Pen darkolivegreen;

	private static Pen darkorange;

	private static Pen darkorchid;

	private static Pen darkred;

	private static Pen darksalmon;

	private static Pen darkseagreen;

	private static Pen darkslateblue;

	private static Pen darkslategray;

	private static Pen darkturquoise;

	private static Pen darkviolet;

	private static Pen deeppink;

	private static Pen deepskyblue;

	private static Pen dimgray;

	private static Pen dodgerblue;

	private static Pen firebrick;

	private static Pen floralwhite;

	private static Pen forestgreen;

	private static Pen fuchsia;

	private static Pen gainsboro;

	private static Pen ghostwhite;

	private static Pen gold;

	private static Pen goldenrod;

	private static Pen gray;

	private static Pen green;

	private static Pen greenyellow;

	private static Pen honeydew;

	private static Pen hotpink;

	private static Pen indianred;

	private static Pen indigo;

	private static Pen ivory;

	private static Pen khaki;

	private static Pen lavender;

	private static Pen lavenderblush;

	private static Pen lawngreen;

	private static Pen lemonchiffon;

	private static Pen lightblue;

	private static Pen lightcoral;

	private static Pen lightcyan;

	private static Pen lightgoldenrodyellow;

	private static Pen lightgray;

	private static Pen lightgreen;

	private static Pen lightpink;

	private static Pen lightsalmon;

	private static Pen lightseagreen;

	private static Pen lightskyblue;

	private static Pen lightslategray;

	private static Pen lightsteelblue;

	private static Pen lightyellow;

	private static Pen lime;

	private static Pen limegreen;

	private static Pen linen;

	private static Pen magenta;

	private static Pen maroon;

	private static Pen mediumaquamarine;

	private static Pen mediumblue;

	private static Pen mediumorchid;

	private static Pen mediumpurple;

	private static Pen mediumseagreen;

	private static Pen mediumslateblue;

	private static Pen mediumspringgreen;

	private static Pen mediumturquoise;

	private static Pen mediumvioletred;

	private static Pen midnightblue;

	private static Pen mintcream;

	private static Pen mistyrose;

	private static Pen moccasin;

	private static Pen navajowhite;

	private static Pen navy;

	private static Pen oldlace;

	private static Pen olive;

	private static Pen olivedrab;

	private static Pen orange;

	private static Pen orangered;

	private static Pen orchid;

	private static Pen palegoldenrod;

	private static Pen palegreen;

	private static Pen paleturquoise;

	private static Pen palevioletred;

	private static Pen papayawhip;

	private static Pen peachpuff;

	private static Pen peru;

	private static Pen pink;

	private static Pen plum;

	private static Pen powderblue;

	private static Pen purple;

	private static Pen red;

	private static Pen rosybrown;

	private static Pen royalblue;

	private static Pen saddlebrown;

	private static Pen salmon;

	private static Pen sandybrown;

	private static Pen seagreen;

	private static Pen seashell;

	private static Pen sienna;

	private static Pen silver;

	private static Pen skyblue;

	private static Pen slateblue;

	private static Pen slategray;

	private static Pen snow;

	private static Pen springgreen;

	private static Pen steelblue;

	private static Pen tan;

	private static Pen teal;

	private static Pen thistle;

	private static Pen tomato;

	private static Pen transparent;

	private static Pen turquoise;

	private static Pen violet;

	private static Pen wheat;

	private static Pen white;

	private static Pen whitesmoke;

	private static Pen yellow;

	private static Pen yellowgreen;

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen AliceBlue
	{
		get
		{
			if (aliceblue == null)
			{
				aliceblue = new Pen(Color.AliceBlue);
				aliceblue.isModifiable = false;
			}
			return aliceblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen AntiqueWhite
	{
		get
		{
			if (antiquewhite == null)
			{
				antiquewhite = new Pen(Color.AntiqueWhite);
				antiquewhite.isModifiable = false;
			}
			return antiquewhite;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Aqua
	{
		get
		{
			if (aqua == null)
			{
				aqua = new Pen(Color.Aqua);
				aqua.isModifiable = false;
			}
			return aqua;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Aquamarine
	{
		get
		{
			if (aquamarine == null)
			{
				aquamarine = new Pen(Color.Aquamarine);
				aquamarine.isModifiable = false;
			}
			return aquamarine;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Azure
	{
		get
		{
			if (azure == null)
			{
				azure = new Pen(Color.Azure);
				azure.isModifiable = false;
			}
			return azure;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Beige
	{
		get
		{
			if (beige == null)
			{
				beige = new Pen(Color.Beige);
				beige.isModifiable = false;
			}
			return beige;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Bisque
	{
		get
		{
			if (bisque == null)
			{
				bisque = new Pen(Color.Bisque);
				bisque.isModifiable = false;
			}
			return bisque;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Black
	{
		get
		{
			if (black == null)
			{
				black = new Pen(Color.Black);
				black.isModifiable = false;
			}
			return black;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen BlanchedAlmond
	{
		get
		{
			if (blanchedalmond == null)
			{
				blanchedalmond = new Pen(Color.BlanchedAlmond);
				blanchedalmond.isModifiable = false;
			}
			return blanchedalmond;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Blue
	{
		get
		{
			if (blue == null)
			{
				blue = new Pen(Color.Blue);
				blue.isModifiable = false;
			}
			return blue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen BlueViolet
	{
		get
		{
			if (blueviolet == null)
			{
				blueviolet = new Pen(Color.BlueViolet);
				blueviolet.isModifiable = false;
			}
			return blueviolet;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Brown
	{
		get
		{
			if (brown == null)
			{
				brown = new Pen(Color.Brown);
				brown.isModifiable = false;
			}
			return brown;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen BurlyWood
	{
		get
		{
			if (burlywood == null)
			{
				burlywood = new Pen(Color.BurlyWood);
				burlywood.isModifiable = false;
			}
			return burlywood;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen CadetBlue
	{
		get
		{
			if (cadetblue == null)
			{
				cadetblue = new Pen(Color.CadetBlue);
				cadetblue.isModifiable = false;
			}
			return cadetblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Chartreuse
	{
		get
		{
			if (chartreuse == null)
			{
				chartreuse = new Pen(Color.Chartreuse);
				chartreuse.isModifiable = false;
			}
			return chartreuse;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Chocolate
	{
		get
		{
			if (chocolate == null)
			{
				chocolate = new Pen(Color.Chocolate);
				chocolate.isModifiable = false;
			}
			return chocolate;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Coral
	{
		get
		{
			if (coral == null)
			{
				coral = new Pen(Color.Coral);
				coral.isModifiable = false;
			}
			return coral;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen CornflowerBlue
	{
		get
		{
			if (cornflowerblue == null)
			{
				cornflowerblue = new Pen(Color.CornflowerBlue);
				cornflowerblue.isModifiable = false;
			}
			return cornflowerblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Cornsilk
	{
		get
		{
			if (cornsilk == null)
			{
				cornsilk = new Pen(Color.Cornsilk);
				cornsilk.isModifiable = false;
			}
			return cornsilk;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Crimson
	{
		get
		{
			if (crimson == null)
			{
				crimson = new Pen(Color.Crimson);
				crimson.isModifiable = false;
			}
			return crimson;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Cyan
	{
		get
		{
			if (cyan == null)
			{
				cyan = new Pen(Color.Cyan);
				cyan.isModifiable = false;
			}
			return cyan;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkBlue
	{
		get
		{
			if (darkblue == null)
			{
				darkblue = new Pen(Color.DarkBlue);
				darkblue.isModifiable = false;
			}
			return darkblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkCyan
	{
		get
		{
			if (darkcyan == null)
			{
				darkcyan = new Pen(Color.DarkCyan);
				darkcyan.isModifiable = false;
			}
			return darkcyan;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkGoldenrod
	{
		get
		{
			if (darkgoldenrod == null)
			{
				darkgoldenrod = new Pen(Color.DarkGoldenrod);
				darkgoldenrod.isModifiable = false;
			}
			return darkgoldenrod;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkGray
	{
		get
		{
			if (darkgray == null)
			{
				darkgray = new Pen(Color.DarkGray);
				darkgray.isModifiable = false;
			}
			return darkgray;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkGreen
	{
		get
		{
			if (darkgreen == null)
			{
				darkgreen = new Pen(Color.DarkGreen);
				darkgreen.isModifiable = false;
			}
			return darkgreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkKhaki
	{
		get
		{
			if (darkkhaki == null)
			{
				darkkhaki = new Pen(Color.DarkKhaki);
				darkkhaki.isModifiable = false;
			}
			return darkkhaki;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkMagenta
	{
		get
		{
			if (darkmagenta == null)
			{
				darkmagenta = new Pen(Color.DarkMagenta);
				darkmagenta.isModifiable = false;
			}
			return darkmagenta;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkOliveGreen
	{
		get
		{
			if (darkolivegreen == null)
			{
				darkolivegreen = new Pen(Color.DarkOliveGreen);
				darkolivegreen.isModifiable = false;
			}
			return darkolivegreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkOrange
	{
		get
		{
			if (darkorange == null)
			{
				darkorange = new Pen(Color.DarkOrange);
				darkorange.isModifiable = false;
			}
			return darkorange;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkOrchid
	{
		get
		{
			if (darkorchid == null)
			{
				darkorchid = new Pen(Color.DarkOrchid);
				darkorchid.isModifiable = false;
			}
			return darkorchid;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkRed
	{
		get
		{
			if (darkred == null)
			{
				darkred = new Pen(Color.DarkRed);
				darkred.isModifiable = false;
			}
			return darkred;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkSalmon
	{
		get
		{
			if (darksalmon == null)
			{
				darksalmon = new Pen(Color.DarkSalmon);
				darksalmon.isModifiable = false;
			}
			return darksalmon;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkSeaGreen
	{
		get
		{
			if (darkseagreen == null)
			{
				darkseagreen = new Pen(Color.DarkSeaGreen);
				darkseagreen.isModifiable = false;
			}
			return darkseagreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkSlateBlue
	{
		get
		{
			if (darkslateblue == null)
			{
				darkslateblue = new Pen(Color.DarkSlateBlue);
				darkslateblue.isModifiable = false;
			}
			return darkslateblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkSlateGray
	{
		get
		{
			if (darkslategray == null)
			{
				darkslategray = new Pen(Color.DarkSlateGray);
				darkslategray.isModifiable = false;
			}
			return darkslategray;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkTurquoise
	{
		get
		{
			if (darkturquoise == null)
			{
				darkturquoise = new Pen(Color.DarkTurquoise);
				darkturquoise.isModifiable = false;
			}
			return darkturquoise;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DarkViolet
	{
		get
		{
			if (darkviolet == null)
			{
				darkviolet = new Pen(Color.DarkViolet);
				darkviolet.isModifiable = false;
			}
			return darkviolet;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DeepPink
	{
		get
		{
			if (deeppink == null)
			{
				deeppink = new Pen(Color.DeepPink);
				deeppink.isModifiable = false;
			}
			return deeppink;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DeepSkyBlue
	{
		get
		{
			if (deepskyblue == null)
			{
				deepskyblue = new Pen(Color.DeepSkyBlue);
				deepskyblue.isModifiable = false;
			}
			return deepskyblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DimGray
	{
		get
		{
			if (dimgray == null)
			{
				dimgray = new Pen(Color.DimGray);
				dimgray.isModifiable = false;
			}
			return dimgray;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen DodgerBlue
	{
		get
		{
			if (dodgerblue == null)
			{
				dodgerblue = new Pen(Color.DodgerBlue);
				dodgerblue.isModifiable = false;
			}
			return dodgerblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Firebrick
	{
		get
		{
			if (firebrick == null)
			{
				firebrick = new Pen(Color.Firebrick);
				firebrick.isModifiable = false;
			}
			return firebrick;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen FloralWhite
	{
		get
		{
			if (floralwhite == null)
			{
				floralwhite = new Pen(Color.FloralWhite);
				floralwhite.isModifiable = false;
			}
			return floralwhite;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ForestGreen
	{
		get
		{
			if (forestgreen == null)
			{
				forestgreen = new Pen(Color.ForestGreen);
				forestgreen.isModifiable = false;
			}
			return forestgreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Fuchsia
	{
		get
		{
			if (fuchsia == null)
			{
				fuchsia = new Pen(Color.Fuchsia);
				fuchsia.isModifiable = false;
			}
			return fuchsia;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Gainsboro
	{
		get
		{
			if (gainsboro == null)
			{
				gainsboro = new Pen(Color.Gainsboro);
				gainsboro.isModifiable = false;
			}
			return gainsboro;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen GhostWhite
	{
		get
		{
			if (ghostwhite == null)
			{
				ghostwhite = new Pen(Color.GhostWhite);
				ghostwhite.isModifiable = false;
			}
			return ghostwhite;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Gold
	{
		get
		{
			if (gold == null)
			{
				gold = new Pen(Color.Gold);
				gold.isModifiable = false;
			}
			return gold;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Goldenrod
	{
		get
		{
			if (goldenrod == null)
			{
				goldenrod = new Pen(Color.Goldenrod);
				goldenrod.isModifiable = false;
			}
			return goldenrod;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Gray
	{
		get
		{
			if (gray == null)
			{
				gray = new Pen(Color.Gray);
				gray.isModifiable = false;
			}
			return gray;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Green
	{
		get
		{
			if (green == null)
			{
				green = new Pen(Color.Green);
				green.isModifiable = false;
			}
			return green;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen GreenYellow
	{
		get
		{
			if (greenyellow == null)
			{
				greenyellow = new Pen(Color.GreenYellow);
				greenyellow.isModifiable = false;
			}
			return greenyellow;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Honeydew
	{
		get
		{
			if (honeydew == null)
			{
				honeydew = new Pen(Color.Honeydew);
				honeydew.isModifiable = false;
			}
			return honeydew;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen HotPink
	{
		get
		{
			if (hotpink == null)
			{
				hotpink = new Pen(Color.HotPink);
				hotpink.isModifiable = false;
			}
			return hotpink;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen IndianRed
	{
		get
		{
			if (indianred == null)
			{
				indianred = new Pen(Color.IndianRed);
				indianred.isModifiable = false;
			}
			return indianred;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Indigo
	{
		get
		{
			if (indigo == null)
			{
				indigo = new Pen(Color.Indigo);
				indigo.isModifiable = false;
			}
			return indigo;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Ivory
	{
		get
		{
			if (ivory == null)
			{
				ivory = new Pen(Color.Ivory);
				ivory.isModifiable = false;
			}
			return ivory;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Khaki
	{
		get
		{
			if (khaki == null)
			{
				khaki = new Pen(Color.Khaki);
				khaki.isModifiable = false;
			}
			return khaki;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Lavender
	{
		get
		{
			if (lavender == null)
			{
				lavender = new Pen(Color.Lavender);
				lavender.isModifiable = false;
			}
			return lavender;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LavenderBlush
	{
		get
		{
			if (lavenderblush == null)
			{
				lavenderblush = new Pen(Color.LavenderBlush);
				lavenderblush.isModifiable = false;
			}
			return lavenderblush;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LawnGreen
	{
		get
		{
			if (lawngreen == null)
			{
				lawngreen = new Pen(Color.LawnGreen);
				lawngreen.isModifiable = false;
			}
			return lawngreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LemonChiffon
	{
		get
		{
			if (lemonchiffon == null)
			{
				lemonchiffon = new Pen(Color.LemonChiffon);
				lemonchiffon.isModifiable = false;
			}
			return lemonchiffon;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightBlue
	{
		get
		{
			if (lightblue == null)
			{
				lightblue = new Pen(Color.LightBlue);
				lightblue.isModifiable = false;
			}
			return lightblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightCoral
	{
		get
		{
			if (lightcoral == null)
			{
				lightcoral = new Pen(Color.LightCoral);
				lightcoral.isModifiable = false;
			}
			return lightcoral;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightCyan
	{
		get
		{
			if (lightcyan == null)
			{
				lightcyan = new Pen(Color.LightCyan);
				lightcyan.isModifiable = false;
			}
			return lightcyan;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightGoldenrodYellow
	{
		get
		{
			if (lightgoldenrodyellow == null)
			{
				lightgoldenrodyellow = new Pen(Color.LightGoldenrodYellow);
				lightgoldenrodyellow.isModifiable = false;
			}
			return lightgoldenrodyellow;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightGray
	{
		get
		{
			if (lightgray == null)
			{
				lightgray = new Pen(Color.LightGray);
				lightgray.isModifiable = false;
			}
			return lightgray;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightGreen
	{
		get
		{
			if (lightgreen == null)
			{
				lightgreen = new Pen(Color.LightGreen);
				lightgreen.isModifiable = false;
			}
			return lightgreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightPink
	{
		get
		{
			if (lightpink == null)
			{
				lightpink = new Pen(Color.LightPink);
				lightpink.isModifiable = false;
			}
			return lightpink;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightSalmon
	{
		get
		{
			if (lightsalmon == null)
			{
				lightsalmon = new Pen(Color.LightSalmon);
				lightsalmon.isModifiable = false;
			}
			return lightsalmon;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightSeaGreen
	{
		get
		{
			if (lightseagreen == null)
			{
				lightseagreen = new Pen(Color.LightSeaGreen);
				lightseagreen.isModifiable = false;
			}
			return lightseagreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightSkyBlue
	{
		get
		{
			if (lightskyblue == null)
			{
				lightskyblue = new Pen(Color.LightSkyBlue);
				lightskyblue.isModifiable = false;
			}
			return lightskyblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightSlateGray
	{
		get
		{
			if (lightslategray == null)
			{
				lightslategray = new Pen(Color.LightSlateGray);
				lightslategray.isModifiable = false;
			}
			return lightslategray;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightSteelBlue
	{
		get
		{
			if (lightsteelblue == null)
			{
				lightsteelblue = new Pen(Color.LightSteelBlue);
				lightsteelblue.isModifiable = false;
			}
			return lightsteelblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LightYellow
	{
		get
		{
			if (lightyellow == null)
			{
				lightyellow = new Pen(Color.LightYellow);
				lightyellow.isModifiable = false;
			}
			return lightyellow;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Lime
	{
		get
		{
			if (lime == null)
			{
				lime = new Pen(Color.Lime);
				lime.isModifiable = false;
			}
			return lime;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen LimeGreen
	{
		get
		{
			if (limegreen == null)
			{
				limegreen = new Pen(Color.LimeGreen);
				limegreen.isModifiable = false;
			}
			return limegreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Linen
	{
		get
		{
			if (linen == null)
			{
				linen = new Pen(Color.Linen);
				linen.isModifiable = false;
			}
			return linen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Magenta
	{
		get
		{
			if (magenta == null)
			{
				magenta = new Pen(Color.Magenta);
				magenta.isModifiable = false;
			}
			return magenta;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Maroon
	{
		get
		{
			if (maroon == null)
			{
				maroon = new Pen(Color.Maroon);
				maroon.isModifiable = false;
			}
			return maroon;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumAquamarine
	{
		get
		{
			if (mediumaquamarine == null)
			{
				mediumaquamarine = new Pen(Color.MediumAquamarine);
				mediumaquamarine.isModifiable = false;
			}
			return mediumaquamarine;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumBlue
	{
		get
		{
			if (mediumblue == null)
			{
				mediumblue = new Pen(Color.MediumBlue);
				mediumblue.isModifiable = false;
			}
			return mediumblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumOrchid
	{
		get
		{
			if (mediumorchid == null)
			{
				mediumorchid = new Pen(Color.MediumOrchid);
				mediumorchid.isModifiable = false;
			}
			return mediumorchid;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumPurple
	{
		get
		{
			if (mediumpurple == null)
			{
				mediumpurple = new Pen(Color.MediumPurple);
				mediumpurple.isModifiable = false;
			}
			return mediumpurple;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumSeaGreen
	{
		get
		{
			if (mediumseagreen == null)
			{
				mediumseagreen = new Pen(Color.MediumSeaGreen);
				mediumseagreen.isModifiable = false;
			}
			return mediumseagreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumSlateBlue
	{
		get
		{
			if (mediumslateblue == null)
			{
				mediumslateblue = new Pen(Color.MediumSlateBlue);
				mediumslateblue.isModifiable = false;
			}
			return mediumslateblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumSpringGreen
	{
		get
		{
			if (mediumspringgreen == null)
			{
				mediumspringgreen = new Pen(Color.MediumSpringGreen);
				mediumspringgreen.isModifiable = false;
			}
			return mediumspringgreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumTurquoise
	{
		get
		{
			if (mediumturquoise == null)
			{
				mediumturquoise = new Pen(Color.MediumTurquoise);
				mediumturquoise.isModifiable = false;
			}
			return mediumturquoise;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MediumVioletRed
	{
		get
		{
			if (mediumvioletred == null)
			{
				mediumvioletred = new Pen(Color.MediumVioletRed);
				mediumvioletred.isModifiable = false;
			}
			return mediumvioletred;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MidnightBlue
	{
		get
		{
			if (midnightblue == null)
			{
				midnightblue = new Pen(Color.MidnightBlue);
				midnightblue.isModifiable = false;
			}
			return midnightblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MintCream
	{
		get
		{
			if (mintcream == null)
			{
				mintcream = new Pen(Color.MintCream);
				mintcream.isModifiable = false;
			}
			return mintcream;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MistyRose
	{
		get
		{
			if (mistyrose == null)
			{
				mistyrose = new Pen(Color.MistyRose);
				mistyrose.isModifiable = false;
			}
			return mistyrose;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Moccasin
	{
		get
		{
			if (moccasin == null)
			{
				moccasin = new Pen(Color.Moccasin);
				moccasin.isModifiable = false;
			}
			return moccasin;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen NavajoWhite
	{
		get
		{
			if (navajowhite == null)
			{
				navajowhite = new Pen(Color.NavajoWhite);
				navajowhite.isModifiable = false;
			}
			return navajowhite;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Navy
	{
		get
		{
			if (navy == null)
			{
				navy = new Pen(Color.Navy);
				navy.isModifiable = false;
			}
			return navy;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen OldLace
	{
		get
		{
			if (oldlace == null)
			{
				oldlace = new Pen(Color.OldLace);
				oldlace.isModifiable = false;
			}
			return oldlace;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Olive
	{
		get
		{
			if (olive == null)
			{
				olive = new Pen(Color.Olive);
				olive.isModifiable = false;
			}
			return olive;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen OliveDrab
	{
		get
		{
			if (olivedrab == null)
			{
				olivedrab = new Pen(Color.OliveDrab);
				olivedrab.isModifiable = false;
			}
			return olivedrab;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Orange
	{
		get
		{
			if (orange == null)
			{
				orange = new Pen(Color.Orange);
				orange.isModifiable = false;
			}
			return orange;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen OrangeRed
	{
		get
		{
			if (orangered == null)
			{
				orangered = new Pen(Color.OrangeRed);
				orangered.isModifiable = false;
			}
			return orangered;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Orchid
	{
		get
		{
			if (orchid == null)
			{
				orchid = new Pen(Color.Orchid);
				orchid.isModifiable = false;
			}
			return orchid;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen PaleGoldenrod
	{
		get
		{
			if (palegoldenrod == null)
			{
				palegoldenrod = new Pen(Color.PaleGoldenrod);
				palegoldenrod.isModifiable = false;
			}
			return palegoldenrod;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen PaleGreen
	{
		get
		{
			if (palegreen == null)
			{
				palegreen = new Pen(Color.PaleGreen);
				palegreen.isModifiable = false;
			}
			return palegreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen PaleTurquoise
	{
		get
		{
			if (paleturquoise == null)
			{
				paleturquoise = new Pen(Color.PaleTurquoise);
				paleturquoise.isModifiable = false;
			}
			return paleturquoise;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen PaleVioletRed
	{
		get
		{
			if (palevioletred == null)
			{
				palevioletred = new Pen(Color.PaleVioletRed);
				palevioletred.isModifiable = false;
			}
			return palevioletred;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen PapayaWhip
	{
		get
		{
			if (papayawhip == null)
			{
				papayawhip = new Pen(Color.PapayaWhip);
				papayawhip.isModifiable = false;
			}
			return papayawhip;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen PeachPuff
	{
		get
		{
			if (peachpuff == null)
			{
				peachpuff = new Pen(Color.PeachPuff);
				peachpuff.isModifiable = false;
			}
			return peachpuff;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Peru
	{
		get
		{
			if (peru == null)
			{
				peru = new Pen(Color.Peru);
				peru.isModifiable = false;
			}
			return peru;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Pink
	{
		get
		{
			if (pink == null)
			{
				pink = new Pen(Color.Pink);
				pink.isModifiable = false;
			}
			return pink;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Plum
	{
		get
		{
			if (plum == null)
			{
				plum = new Pen(Color.Plum);
				plum.isModifiable = false;
			}
			return plum;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen PowderBlue
	{
		get
		{
			if (powderblue == null)
			{
				powderblue = new Pen(Color.PowderBlue);
				powderblue.isModifiable = false;
			}
			return powderblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Purple
	{
		get
		{
			if (purple == null)
			{
				purple = new Pen(Color.Purple);
				purple.isModifiable = false;
			}
			return purple;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Red
	{
		get
		{
			if (red == null)
			{
				red = new Pen(Color.Red);
				red.isModifiable = false;
			}
			return red;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen RosyBrown
	{
		get
		{
			if (rosybrown == null)
			{
				rosybrown = new Pen(Color.RosyBrown);
				rosybrown.isModifiable = false;
			}
			return rosybrown;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen RoyalBlue
	{
		get
		{
			if (royalblue == null)
			{
				royalblue = new Pen(Color.RoyalBlue);
				royalblue.isModifiable = false;
			}
			return royalblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SaddleBrown
	{
		get
		{
			if (saddlebrown == null)
			{
				saddlebrown = new Pen(Color.SaddleBrown);
				saddlebrown.isModifiable = false;
			}
			return saddlebrown;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Salmon
	{
		get
		{
			if (salmon == null)
			{
				salmon = new Pen(Color.Salmon);
				salmon.isModifiable = false;
			}
			return salmon;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SandyBrown
	{
		get
		{
			if (sandybrown == null)
			{
				sandybrown = new Pen(Color.SandyBrown);
				sandybrown.isModifiable = false;
			}
			return sandybrown;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SeaGreen
	{
		get
		{
			if (seagreen == null)
			{
				seagreen = new Pen(Color.SeaGreen);
				seagreen.isModifiable = false;
			}
			return seagreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SeaShell
	{
		get
		{
			if (seashell == null)
			{
				seashell = new Pen(Color.SeaShell);
				seashell.isModifiable = false;
			}
			return seashell;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Sienna
	{
		get
		{
			if (sienna == null)
			{
				sienna = new Pen(Color.Sienna);
				sienna.isModifiable = false;
			}
			return sienna;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Silver
	{
		get
		{
			if (silver == null)
			{
				silver = new Pen(Color.Silver);
				silver.isModifiable = false;
			}
			return silver;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SkyBlue
	{
		get
		{
			if (skyblue == null)
			{
				skyblue = new Pen(Color.SkyBlue);
				skyblue.isModifiable = false;
			}
			return skyblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SlateBlue
	{
		get
		{
			if (slateblue == null)
			{
				slateblue = new Pen(Color.SlateBlue);
				slateblue.isModifiable = false;
			}
			return slateblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SlateGray
	{
		get
		{
			if (slategray == null)
			{
				slategray = new Pen(Color.SlateGray);
				slategray.isModifiable = false;
			}
			return slategray;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Snow
	{
		get
		{
			if (snow == null)
			{
				snow = new Pen(Color.Snow);
				snow.isModifiable = false;
			}
			return snow;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SpringGreen
	{
		get
		{
			if (springgreen == null)
			{
				springgreen = new Pen(Color.SpringGreen);
				springgreen.isModifiable = false;
			}
			return springgreen;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen SteelBlue
	{
		get
		{
			if (steelblue == null)
			{
				steelblue = new Pen(Color.SteelBlue);
				steelblue.isModifiable = false;
			}
			return steelblue;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Tan
	{
		get
		{
			if (tan == null)
			{
				tan = new Pen(Color.Tan);
				tan.isModifiable = false;
			}
			return tan;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Teal
	{
		get
		{
			if (teal == null)
			{
				teal = new Pen(Color.Teal);
				teal.isModifiable = false;
			}
			return teal;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Thistle
	{
		get
		{
			if (thistle == null)
			{
				thistle = new Pen(Color.Thistle);
				thistle.isModifiable = false;
			}
			return thistle;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Tomato
	{
		get
		{
			if (tomato == null)
			{
				tomato = new Pen(Color.Tomato);
				tomato.isModifiable = false;
			}
			return tomato;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Transparent
	{
		get
		{
			if (transparent == null)
			{
				transparent = new Pen(Color.Transparent);
				transparent.isModifiable = false;
			}
			return transparent;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Turquoise
	{
		get
		{
			if (turquoise == null)
			{
				turquoise = new Pen(Color.Turquoise);
				turquoise.isModifiable = false;
			}
			return turquoise;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Violet
	{
		get
		{
			if (violet == null)
			{
				violet = new Pen(Color.Violet);
				violet.isModifiable = false;
			}
			return violet;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Wheat
	{
		get
		{
			if (wheat == null)
			{
				wheat = new Pen(Color.Wheat);
				wheat.isModifiable = false;
			}
			return wheat;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen White
	{
		get
		{
			if (white == null)
			{
				white = new Pen(Color.White);
				white.isModifiable = false;
			}
			return white;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen WhiteSmoke
	{
		get
		{
			if (whitesmoke == null)
			{
				whitesmoke = new Pen(Color.WhiteSmoke);
				whitesmoke.isModifiable = false;
			}
			return whitesmoke;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Yellow
	{
		get
		{
			if (yellow == null)
			{
				yellow = new Pen(Color.Yellow);
				yellow.isModifiable = false;
			}
			return yellow;
		}
	}

	/// <summary>A system-defined <see cref="T:System.Drawing.Pen" /> object with a width of 1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> object set to a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen YellowGreen
	{
		get
		{
			if (yellowgreen == null)
			{
				yellowgreen = new Pen(Color.YellowGreen);
				yellowgreen.isModifiable = false;
			}
			return yellowgreen;
		}
	}

	private Pens()
	{
	}
}
