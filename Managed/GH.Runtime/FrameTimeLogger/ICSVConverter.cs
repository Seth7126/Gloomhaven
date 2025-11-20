using System.Collections.Generic;

namespace FrameTimeLogger;

public interface ICSVConverter
{
	string ConvertToString<T>(IEnumerable<Column<T>> columns);
}
