namespace System.Drawing;

/// <summary>Each property of the <see cref="T:System.Drawing.SystemPens" /> class is a <see cref="T:System.Drawing.Pen" /> that is the color of a Windows display element and that has a width of 1 pixel.</summary>
/// <filterpriority>1</filterpriority>
public sealed class SystemPens
{
	private static Pen active_caption_text;

	private static Pen control;

	private static Pen control_dark;

	private static Pen control_dark_dark;

	private static Pen control_light;

	private static Pen control_light_light;

	private static Pen control_text;

	private static Pen gray_text;

	private static Pen highlight;

	private static Pen highlight_text;

	private static Pen inactive_caption_text;

	private static Pen info_text;

	private static Pen menu_text;

	private static Pen window_frame;

	private static Pen window_text;

	private static Pen active_border;

	private static Pen active_caption;

	private static Pen app_workspace;

	private static Pen button_face;

	private static Pen button_highlight;

	private static Pen button_shadow;

	private static Pen desktop;

	private static Pen gradient_activecaption;

	private static Pen gradient_inactivecaption;

	private static Pen hot_track;

	private static Pen inactive_border;

	private static Pen inactive_caption;

	private static Pen info;

	private static Pen menu;

	private static Pen menu_bar;

	private static Pen menu_highlight;

	private static Pen scroll_bar;

	private static Pen window;

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the text in the active window's title bar.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the text in the active window's title bar.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ActiveCaptionText
	{
		get
		{
			if (active_caption_text == null)
			{
				active_caption_text = new Pen(SystemColors.ActiveCaptionText);
				active_caption_text.isModifiable = false;
			}
			return active_caption_text;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the face color of a 3-D element.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the face color of a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Control
	{
		get
		{
			if (control == null)
			{
				control = new Pen(SystemColors.Control);
				control.isModifiable = false;
			}
			return control;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the shadow color of a 3-D element. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the shadow color of a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ControlDark
	{
		get
		{
			if (control_dark == null)
			{
				control_dark = new Pen(SystemColors.ControlDark);
				control_dark.isModifiable = false;
			}
			return control_dark;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the dark shadow color of a 3-D element. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the dark shadow color of a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ControlDarkDark
	{
		get
		{
			if (control_dark_dark == null)
			{
				control_dark_dark = new Pen(SystemColors.ControlDarkDark);
				control_dark_dark.isModifiable = false;
			}
			return control_dark_dark;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the light color of a 3-D element. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the light color of a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ControlLight
	{
		get
		{
			if (control_light == null)
			{
				control_light = new Pen(SystemColors.ControlLight);
				control_light.isModifiable = false;
			}
			return control_light;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the highlight color of a 3-D element. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the highlight color of a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ControlLightLight
	{
		get
		{
			if (control_light_light == null)
			{
				control_light_light = new Pen(SystemColors.ControlLightLight);
				control_light_light.isModifiable = false;
			}
			return control_light_light;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of text in a 3-D element.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of text in a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ControlText
	{
		get
		{
			if (control_text == null)
			{
				control_text = new Pen(SystemColors.ControlText);
				control_text.isModifiable = false;
			}
			return control_text;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of dimmed text. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of dimmed text.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen GrayText
	{
		get
		{
			if (gray_text == null)
			{
				gray_text = new Pen(SystemColors.GrayText);
				gray_text.isModifiable = false;
			}
			return gray_text;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the background of selected items. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the background of selected items.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Highlight
	{
		get
		{
			if (highlight == null)
			{
				highlight = new Pen(SystemColors.Highlight);
				highlight.isModifiable = false;
			}
			return highlight;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the text of selected items. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the text of selected items.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen HighlightText
	{
		get
		{
			if (highlight_text == null)
			{
				highlight_text = new Pen(SystemColors.HighlightText);
				highlight_text.isModifiable = false;
			}
			return highlight_text;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the text in an inactive window's title bar.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the text in an inactive window's title bar.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen InactiveCaptionText
	{
		get
		{
			if (inactive_caption_text == null)
			{
				inactive_caption_text = new Pen(SystemColors.InactiveCaptionText);
				inactive_caption_text.isModifiable = false;
			}
			return inactive_caption_text;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the text of a ToolTip.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the text of a ToolTip.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen InfoText
	{
		get
		{
			if (info_text == null)
			{
				info_text = new Pen(SystemColors.InfoText);
				info_text.isModifiable = false;
			}
			return info_text;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of a menu's text.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of a menu's text.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MenuText
	{
		get
		{
			if (menu_text == null)
			{
				menu_text = new Pen(SystemColors.MenuText);
				menu_text.isModifiable = false;
			}
			return menu_text;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of a window frame.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of a window frame.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen WindowFrame
	{
		get
		{
			if (window_frame == null)
			{
				window_frame = new Pen(SystemColors.WindowFrame);
				window_frame.isModifiable = false;
			}
			return window_frame;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the text in the client area of a window.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the text in the client area of a window.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen WindowText
	{
		get
		{
			if (window_text == null)
			{
				window_text = new Pen(SystemColors.WindowText);
				window_text.isModifiable = false;
			}
			return window_text;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the active window's border.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the active window's border.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ActiveBorder
	{
		get
		{
			if (active_border == null)
			{
				active_border = new Pen(SystemColors.ActiveBorder);
				active_border.isModifiable = false;
			}
			return active_border;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the background of the active window's title bar.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the background of the active window's title bar.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ActiveCaption
	{
		get
		{
			if (active_caption == null)
			{
				active_caption = new Pen(SystemColors.ActiveCaption);
				active_caption.isModifiable = false;
			}
			return active_caption;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the application workspace.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the application workspace.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen AppWorkspace
	{
		get
		{
			if (app_workspace == null)
			{
				app_workspace = new Pen(SystemColors.AppWorkspace);
				app_workspace.isModifiable = false;
			}
			return app_workspace;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the face color of a 3-D element.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the face color of a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ButtonFace
	{
		get
		{
			if (button_face == null)
			{
				button_face = new Pen(SystemColors.ButtonFace);
				button_face.isModifiable = false;
			}
			return button_face;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the highlight color of a 3-D element. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the highlight color of a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ButtonHighlight
	{
		get
		{
			if (button_highlight == null)
			{
				button_highlight = new Pen(SystemColors.ButtonHighlight);
				button_highlight.isModifiable = false;
			}
			return button_highlight;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the shadow color of a 3-D element. </summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the shadow color of a 3-D element.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ButtonShadow
	{
		get
		{
			if (button_shadow == null)
			{
				button_shadow = new Pen(SystemColors.ButtonShadow);
				button_shadow.isModifiable = false;
			}
			return button_shadow;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the Windows desktop.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the Windows desktop.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Desktop
	{
		get
		{
			if (desktop == null)
			{
				desktop = new Pen(SystemColors.Desktop);
				desktop.isModifiable = false;
			}
			return desktop;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the lightest color in the color gradient of an active window's title bar.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the lightest color in the color gradient of an active window's title bar.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen GradientActiveCaption
	{
		get
		{
			if (gradient_activecaption == null)
			{
				gradient_activecaption = new Pen(SystemColors.GradientActiveCaption);
				gradient_activecaption.isModifiable = false;
			}
			return gradient_activecaption;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the lightest color in the color gradient of an inactive window's title bar.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the lightest color in the color gradient of an inactive window's title bar.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen GradientInactiveCaption
	{
		get
		{
			if (gradient_inactivecaption == null)
			{
				gradient_inactivecaption = new Pen(SystemColors.GradientInactiveCaption);
				gradient_inactivecaption.isModifiable = false;
			}
			return gradient_inactivecaption;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color used to designate a hot-tracked item.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color used to designate a hot-tracked item.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen HotTrack
	{
		get
		{
			if (hot_track == null)
			{
				hot_track = new Pen(SystemColors.HotTrack);
				hot_track.isModifiable = false;
			}
			return hot_track;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> is the color of the border of an inactive window.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the border of an inactive window.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen InactiveBorder
	{
		get
		{
			if (inactive_border == null)
			{
				inactive_border = new Pen(SystemColors.InactiveBorder);
				inactive_border.isModifiable = false;
			}
			return inactive_border;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the title bar caption of an inactive window.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the title bar caption of an inactive window.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen InactiveCaption
	{
		get
		{
			if (inactive_caption == null)
			{
				inactive_caption = new Pen(SystemColors.InactiveCaption);
				inactive_caption.isModifiable = false;
			}
			return inactive_caption;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the background of a ToolTip.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the background of a ToolTip.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Info
	{
		get
		{
			if (info == null)
			{
				info = new Pen(SystemColors.Info);
				info.isModifiable = false;
			}
			return info;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of a menu's background.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of a menu's background.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Menu
	{
		get
		{
			if (menu == null)
			{
				menu = new Pen(SystemColors.Menu);
				menu.isModifiable = false;
			}
			return menu;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the background of a menu bar.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the background of a menu bar.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MenuBar
	{
		get
		{
			if (menu_bar == null)
			{
				menu_bar = new Pen(SystemColors.MenuBar);
				menu_bar.isModifiable = false;
			}
			return menu_bar;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color used to highlight menu items when the menu appears as a flat menu.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color used to highlight menu items when the menu appears as a flat menu.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen MenuHighlight
	{
		get
		{
			if (menu_highlight == null)
			{
				menu_highlight = new Pen(SystemColors.MenuHighlight);
				menu_highlight.isModifiable = false;
			}
			return menu_highlight;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the background of a scroll bar.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the background of a scroll bar.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen ScrollBar
	{
		get
		{
			if (scroll_bar == null)
			{
				scroll_bar = new Pen(SystemColors.ScrollBar);
				scroll_bar.isModifiable = false;
			}
			return scroll_bar;
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Pen" /> that is the color of the background in the client area of a window.</summary>
	/// <returns>A <see cref="T:System.Drawing.Pen" /> that is the color of the background in the client area of a window.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen Window
	{
		get
		{
			if (window == null)
			{
				window = new Pen(SystemColors.Window);
				window.isModifiable = false;
			}
			return window;
		}
	}

	private SystemPens()
	{
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Pen" /> from the specified <see cref="T:System.Drawing.Color" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Pen" /> this method creates.</returns>
	/// <param name="c">The <see cref="T:System.Drawing.Color" /> for the new <see cref="T:System.Drawing.Pen" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Pen FromSystemColor(Color c)
	{
		if (c.IsSystemColor)
		{
			return new Pen(c)
			{
				isModifiable = false
			};
		}
		throw new ArgumentException($"The color {c} is not a system color.");
	}
}
