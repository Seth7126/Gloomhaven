namespace FrameTimeLogger;

public interface ICSVWriter
{
	void Write(string fileName, string csvString);
}
