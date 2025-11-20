using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class Progress
{
	[DllImport("SaveData")]
	private static extern float PrxSaveDataGetProgress(out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataClearProgress(out APIResult result);

	public static void ClearProgress()
	{
		PrxSaveDataClearProgress(out var result);
		if (result.RaiseException)
		{
			throw new SaveDataException(result);
		}
	}

	public static float GetProgress()
	{
		APIResult result2;
		float result = PrxSaveDataGetProgress(out result2);
		if (result2.RaiseException)
		{
			throw new SaveDataException(result2);
		}
		return result;
	}
}
