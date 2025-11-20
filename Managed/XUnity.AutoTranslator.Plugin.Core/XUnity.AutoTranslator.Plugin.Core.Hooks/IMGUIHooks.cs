using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class IMGUIHooks
{
	public static readonly Type[][] All = new Type[9][]
	{
		new Type[2]
		{
			typeof(GUI_BeginGroup_Hook_New),
			typeof(GUI_BeginGroup_Hook)
		},
		new Type[2]
		{
			typeof(GUI_DoLabel_Hook_New),
			typeof(GUI_DoLabel_Hook)
		},
		new Type[2]
		{
			typeof(GUI_DoButton_Hook_New),
			typeof(GUI_DoButton_Hook)
		},
		new Type[3]
		{
			typeof(GUI_DoButtonGrid_Hook_2019),
			typeof(GUI_DoButtonGrid_Hook_2018),
			typeof(GUI_DoButtonGrid_Hook)
		},
		new Type[2]
		{
			typeof(GUI_DoToggle_Hook_New),
			typeof(GUI_DoToggle_Hook)
		},
		new Type[1] { typeof(GUI_Box_Hook) },
		new Type[1] { typeof(GUI_DoRepeatButton_Hook) },
		new Type[1] { typeof(GUI_DoModalWindow_Hook) },
		new Type[1] { typeof(GUI_DoWindow_Hook) }
	};
}
