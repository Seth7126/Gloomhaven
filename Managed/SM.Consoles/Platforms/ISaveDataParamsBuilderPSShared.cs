namespace Platforms;

public interface ISaveDataParamsBuilderPSShared
{
	ulong SaveDataSize { get; }

	void GetWriteParams(string originalPath, out string saveDataDirectory, out string innerPathToFile, out SaveDataParams saveDataParams, string[] tags);

	void GetReadParams(string originalPath, out string saveDataDirectory, out string innerPathToFile, string[] tags);
}
