using System;

namespace Sony.NP;

internal static class Compatibility
{
	internal static ServiceTypes ConvertServiceToEnum(int serviceType)
	{
		if (Main.initResult.sceSDKVersion >= 122683392)
		{
			return (ServiceTypes)serviceType;
		}
		if (Main.initResult.sceSDKVersion >= 117440512)
		{
			return ConvertFrom((ServiceTypes_SDK7_0)serviceType);
		}
		if (Main.initResult.sceSDKVersion >= 83886080)
		{
			return ConvertFrom((ServiceTypes_SDK6_5)serviceType);
		}
		if (Main.initResult.sceSDKVersion >= 72351744)
		{
			return ConvertFrom((ServiceTypes_SDK4_5)serviceType);
		}
		return ServiceTypes.Invalid;
	}

	internal static FunctionTypes ConvertFunctionToEnum(int functionType)
	{
		if (Main.initResult.sceSDKVersion >= 122683392)
		{
			return (FunctionTypes)functionType;
		}
		if (Main.initResult.sceSDKVersion >= 117440512)
		{
			return ConvertFrom((FunctionTypes_SDK7_0)functionType);
		}
		if (Main.initResult.sceSDKVersion >= 105906176)
		{
			return ConvertFrom((FunctionTypes_SDK6_5)functionType);
		}
		if (Main.initResult.sceSDKVersion >= 83886080)
		{
			return ConvertFrom((FunctionTypes_SDK6_0)functionType);
		}
		if (Main.initResult.sceSDKVersion >= 72351744)
		{
			return ConvertFrom((FunctionTypes_SDK4_5)functionType);
		}
		return FunctionTypes.Invalid;
	}

	internal static int ConvertFromEnum(ServiceTypes serviceType)
	{
		if (Main.initResult.sceSDKVersion >= 122683392)
		{
			return (int)serviceType;
		}
		if (Main.initResult.sceSDKVersion >= 117440512)
		{
			return (int)ConvertToSDK70(serviceType);
		}
		if (Main.initResult.sceSDKVersion >= 83886080)
		{
			return (int)ConvertToSDK65(serviceType);
		}
		if (Main.initResult.sceSDKVersion >= 72351744)
		{
			return (int)ConvertToSDK45(serviceType);
		}
		return 0;
	}

	internal static int ConvertFromEnum(FunctionTypes functionType)
	{
		if (Main.initResult.sceSDKVersion >= 122683392)
		{
			return (int)functionType;
		}
		if (Main.initResult.sceSDKVersion >= 117440512)
		{
			return (int)ConvertToSDK70(functionType);
		}
		if (Main.initResult.sceSDKVersion >= 105906176)
		{
			return (int)ConvertToSDK65(functionType);
		}
		if (Main.initResult.sceSDKVersion >= 83886080)
		{
			return (int)ConvertToSDK60(functionType);
		}
		if (Main.initResult.sceSDKVersion >= 72351744)
		{
			return (int)ConvertToSDK45(functionType);
		}
		return 0;
	}

	private static ServiceTypes ConvertFrom(ServiceTypes_SDK4_5 oldServiceType)
	{
		try
		{
			return (ServiceTypes)Enum.Parse(typeof(ServiceTypes), oldServiceType.ToString());
		}
		catch (ArgumentException)
		{
			return ServiceTypes.Invalid;
		}
	}

	private static ServiceTypes ConvertFrom(ServiceTypes_SDK6_5 oldServiceType)
	{
		try
		{
			return (ServiceTypes)Enum.Parse(typeof(ServiceTypes), oldServiceType.ToString());
		}
		catch (ArgumentException)
		{
			return ServiceTypes.Invalid;
		}
	}

	private static ServiceTypes ConvertFrom(ServiceTypes_SDK7_0 oldServiceType)
	{
		try
		{
			return (ServiceTypes)Enum.Parse(typeof(ServiceTypes), oldServiceType.ToString());
		}
		catch (ArgumentException)
		{
			return ServiceTypes.Invalid;
		}
	}

	private static FunctionTypes ConvertFrom(FunctionTypes_SDK4_5 oldFunctionType)
	{
		try
		{
			return (FunctionTypes)Enum.Parse(typeof(FunctionTypes), oldFunctionType.ToString());
		}
		catch (ArgumentException)
		{
			return FunctionTypes.Invalid;
		}
	}

	private static FunctionTypes ConvertFrom(FunctionTypes_SDK6_0 oldFunctionType)
	{
		try
		{
			return (FunctionTypes)Enum.Parse(typeof(FunctionTypes), oldFunctionType.ToString());
		}
		catch (ArgumentException)
		{
			return FunctionTypes.Invalid;
		}
	}

	private static FunctionTypes ConvertFrom(FunctionTypes_SDK6_5 oldFunctionType)
	{
		try
		{
			return (FunctionTypes)Enum.Parse(typeof(FunctionTypes), oldFunctionType.ToString());
		}
		catch (ArgumentException)
		{
			return FunctionTypes.Invalid;
		}
	}

	private static FunctionTypes ConvertFrom(FunctionTypes_SDK7_0 oldFunctionType)
	{
		try
		{
			return (FunctionTypes)Enum.Parse(typeof(FunctionTypes), oldFunctionType.ToString());
		}
		catch (ArgumentException)
		{
			return FunctionTypes.Invalid;
		}
	}

	private static ServiceTypes_SDK4_5 ConvertToSDK45(ServiceTypes serviceType)
	{
		try
		{
			return (ServiceTypes_SDK4_5)Enum.Parse(typeof(ServiceTypes_SDK4_5), serviceType.ToString());
		}
		catch (ArgumentException)
		{
			return ServiceTypes_SDK4_5.Invalid;
		}
	}

	private static ServiceTypes_SDK7_0 ConvertToSDK70(ServiceTypes serviceType)
	{
		try
		{
			return (ServiceTypes_SDK7_0)Enum.Parse(typeof(ServiceTypes_SDK7_0), serviceType.ToString());
		}
		catch (ArgumentException)
		{
			return ServiceTypes_SDK7_0.Invalid;
		}
	}

	private static ServiceTypes_SDK6_5 ConvertToSDK65(ServiceTypes serviceType)
	{
		try
		{
			return (ServiceTypes_SDK6_5)Enum.Parse(typeof(ServiceTypes_SDK6_5), serviceType.ToString());
		}
		catch (ArgumentException)
		{
			return ServiceTypes_SDK6_5.Invalid;
		}
	}

	private static FunctionTypes_SDK4_5 ConvertToSDK45(FunctionTypes functionType)
	{
		try
		{
			return (FunctionTypes_SDK4_5)Enum.Parse(typeof(FunctionTypes_SDK4_5), functionType.ToString());
		}
		catch (ArgumentException)
		{
			return FunctionTypes_SDK4_5.Invalid;
		}
	}

	private static FunctionTypes_SDK6_0 ConvertToSDK60(FunctionTypes functionType)
	{
		try
		{
			return (FunctionTypes_SDK6_0)Enum.Parse(typeof(FunctionTypes_SDK6_0), functionType.ToString());
		}
		catch (ArgumentException)
		{
			return FunctionTypes_SDK6_0.Invalid;
		}
	}

	private static FunctionTypes_SDK6_5 ConvertToSDK65(FunctionTypes functionType)
	{
		try
		{
			return (FunctionTypes_SDK6_5)Enum.Parse(typeof(FunctionTypes_SDK6_5), functionType.ToString());
		}
		catch (ArgumentException)
		{
			return FunctionTypes_SDK6_5.Invalid;
		}
	}

	private static FunctionTypes_SDK7_0 ConvertToSDK70(FunctionTypes functionType)
	{
		try
		{
			return (FunctionTypes_SDK7_0)Enum.Parse(typeof(FunctionTypes_SDK7_0), functionType.ToString());
		}
		catch (ArgumentException)
		{
			return FunctionTypes_SDK7_0.Invalid;
		}
	}
}
