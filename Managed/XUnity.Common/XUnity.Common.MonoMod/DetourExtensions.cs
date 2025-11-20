using System.Linq;
using System.Reflection;

namespace XUnity.Common.MonoMod;

public static class DetourExtensions
{
	public static T GenerateTrampolineEx<T>(this object detour)
	{
		return (T)(from x in detour.GetType().GetMethods()
			where x.Name == "GenerateTrampoline" && x.IsGenericMethod
			select x).FirstOrDefault().MakeGenericMethod(typeof(T)).Invoke(detour, null);
	}
}
