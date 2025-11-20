using System.Collections.Generic;

public static class BoltAssemblies
{
	private static readonly List<string> AssemblyList;

	public static IEnumerator<string> AllAssemblies => AssemblyList.GetEnumerator();

	static BoltAssemblies()
	{
		AssemblyList = new List<string>();
		AssemblyList.Add("GH.Runtime");
		AssemblyList.Add("InControl");
		AssemblyList.Add("ThirdParty");
		AssemblyList.Add("PhotonVoice.API");
		AssemblyList.Add("ScenarioRuleLibrary");
		AssemblyList.Add("StompyRobot.SRF");
		AssemblyList.Add("StompyRobot.SRDebugger");
		AssemblyList.Add("PhotonVoice");
		AssemblyList.Add("MapRuleLibrary");
		AssemblyList.Add("SM.Consoles");
		AssemblyList.Add("SM.Gamepad");
		AssemblyList.Add("GH.Runtime.FirstPass");
		AssemblyList.Add("Cinemachine");
		AssemblyList.Add("InControl.Examples");
		AssemblyList.Add("SharedLibrary");
		AssemblyList.Add("PhotonBolt");
		AssemblyList.Add("GH.Shared");
		AssemblyList.Add("Utilities");
		AssemblyList.Add("SM.Utils");
	}
}
