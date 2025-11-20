using System;

namespace AsmodeeNet.Utils;

[Serializable]
public class ActionState
{
	public string Name { get; set; }

	public Action ActionEnter { get; set; }

	public Action ActionUpdate { get; set; }

	public Action ActionExit { get; set; }

	public ActionState(string name, Action actionEnter, Action actionUpdate, Action actionExit)
	{
		Name = name;
		ActionEnter = actionEnter;
		ActionUpdate = actionUpdate;
		ActionExit = actionExit;
	}
}
