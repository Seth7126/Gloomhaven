using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using GLOOM;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

[Serializable]
[DebuggerDisplay("ButtonHierarchy: {ButtonHierarchyAsString}")]
public class CAutoButtonClick : CAuto, ISerializable
{
	private const int c_CurrentVersion = 2;

	public List<AutoLogUtility.ButtonAndOrder> ButtonHierarchy { get; private set; }

	public string ButtonHierarchyAsString => string.Join("/", ButtonHierarchy.Select((AutoLogUtility.ButtonAndOrder s) => s.ButtonName));

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ButtonHierarchy", ButtonHierarchy);
	}

	protected CAutoButtonClick(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "ButtonHierarchy")
				{
					ButtonHierarchy = (List<AutoLogUtility.ButtonAndOrder>)info.GetValue("ButtonHierarchy", typeof(List<AutoLogUtility.ButtonAndOrder>));
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize CAutoButtonClick entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (base.Version >= 2)
		{
			return;
		}
		if (base.Version < 1 && ButtonHierarchy != null && ButtonHierarchy.Count > 0)
		{
			if (ButtonHierarchy.Any((AutoLogUtility.ButtonAndOrder x) => x.ButtonName == "UseItemsBar"))
			{
				int num = ButtonHierarchy.FindIndex((AutoLogUtility.ButtonAndOrder x) => x.ButtonName == "UseItemsBar");
				int num2 = 3;
				ButtonHierarchy[num + 1].OrderInSiblings += num2;
			}
			AutoLogUtility.ButtonAndOrder buttonAndOrder = ButtonHierarchy.Last();
			AutoLogUtility.ButtonAndOrder buttonAndOrder2 = ButtonHierarchy[ButtonHierarchy.Count - 2];
			if ((buttonAndOrder.ButtonName == "Ready Button" || buttonAndOrder.ButtonName == "Undo Button" || buttonAndOrder.ButtonName == "Skip Button") && !ButtonHierarchy.Exists((AutoLogUtility.ButtonAndOrder it) => it.ButtonName == "Bottom Options") && buttonAndOrder2.ButtonName != "UIItemCardPicker")
			{
				ButtonHierarchy.Insert(ButtonHierarchy.Count - 1, new AutoLogUtility.ButtonAndOrder("Bottom Options", 0));
			}
		}
		if (base.Version < 2 && ButtonHierarchy != null && ButtonHierarchy.Count > 0)
		{
			foreach (AutoLogUtility.ButtonAndOrder buttonOrder in ButtonHierarchy)
			{
				if (CCharacterClass.CharacterNames.Any((ECharacter x) => buttonOrder.ButtonName.Contains(x.ToString())) && buttonOrder.ButtonName.Contains("- (Clone)(Clone)"))
				{
					string[] substrings = buttonOrder.ButtonName.Split(' ');
					AbilityCardYMLData abilityCardYMLData = ScenarioRuleClient.SRLYML.AbilityCards.SingleOrDefault((AbilityCardYMLData x) => x.CharacterID == substrings[0] + "ID" && x.ID == int.Parse(substrings[1]));
					if (abilityCardYMLData != null)
					{
						string translation = LocalizationManager.GetTranslation(abilityCardYMLData.Name);
						buttonOrder.ButtonName = substrings[0] + " " + substrings[1] + " - " + translation + "(Clone)(Clone)";
					}
				}
			}
		}
		base.Version = 2;
	}

	public CAutoButtonClick(int id, List<AutoLogUtility.ButtonAndOrder> buttonHierarchy)
		: base(EAutoType.ButtonClick, id)
	{
		base.Version = 2;
		ButtonHierarchy = buttonHierarchy;
	}
}
