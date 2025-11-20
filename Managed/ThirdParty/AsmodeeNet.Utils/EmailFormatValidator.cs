using System.Text.RegularExpressions;

namespace AsmodeeNet.Utils;

public static class EmailFormatValidator
{
	public static bool IsValidEmail(string emailCandidate)
	{
		return Regex.IsMatch(emailCandidate, "^(?(\")(\"[^\"]+?\"@)|(([0-9a-zA-Z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-zA-Z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,6}))$");
	}
}
