public class AutotestResult
{
	public string TestName { get; private set; }

	public bool Result { get; private set; }

	public string TestLog { get; private set; }

	public AutotestResult(string name, bool result, string log)
	{
		TestName = name;
		Result = result;
		TestLog = log;
	}
}
