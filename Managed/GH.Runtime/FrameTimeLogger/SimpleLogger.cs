#define ENABLE_LOGS
using System.Collections.Generic;

namespace FrameTimeLogger;

public class SimpleLogger<T>
{
	private readonly ICSVConverter _converter;

	private readonly ICSVWriter _writer;

	private readonly List<T> _frameTimeValues = new List<T>();

	private readonly List<T> _totalTimeValues = new List<T>();

	private const string TotalTimeColumnString = "TotalTime(s)";

	private const string FrameTimeColumnString = "FrameTime(ms)";

	public SimpleLogger(ICSVConverter converter, ICSVWriter writer)
	{
		_converter = converter;
		_writer = writer;
		Debug.Log(string.Format("ICSVWriter {0} type is {1}", writer, (writer != null) ? writer.GetType().Name : "empty"));
	}

	public void AddValue(T totalTimeLogValue, T frameTimeLogValue)
	{
		_totalTimeValues.Add(totalTimeLogValue);
		_frameTimeValues.Add(frameTimeLogValue);
	}

	public void SaveLog(string fileName, string columnName)
	{
		_writer?.Write(fileName ?? "", _converter.ConvertToString(new Column<T>[2]
		{
			new Column<T>("TotalTime(s)", _totalTimeValues),
			new Column<T>("FrameTime(ms)", _frameTimeValues)
		}));
		_totalTimeValues.Clear();
		_frameTimeValues.Clear();
	}
}
