namespace Script.GUI.Utils;

public static class StringExtensions
{
	public static string RemoveStartSpaces(this string content)
	{
		string[] array = content.Split("\n");
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].TrimStart();
		}
		return string.Join("\n", array);
	}
}
