namespace DynamicFogAndMist;

internal static class FOG_TYPE_Ext
{
	public static bool isPlus(this FOG_TYPE fogType)
	{
		if (fogType != FOG_TYPE.DesktopFogPlusWithSkyHaze && fogType != FOG_TYPE.MobileFogSimple && fogType != FOG_TYPE.MobileFogBasic && fogType != FOG_TYPE.MobileFogOrthogonal)
		{
			return fogType == FOG_TYPE.DesktopFogPlusOrthogonal;
		}
		return true;
	}
}
