using Apparance.Net;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ApparanceEntity))]
[RequireComponent(typeof(ProceduralStyle))]
[ExecuteInEditMode]
public class ProceduralProp : ProceduralBase, IProceduralContent, IContentUpdateMonitor
{
	private ParameterCollection Parameters;

	public UnityAction PlacementCompleteAction;

	private void Start()
	{
		Rebuild();
		Apply(force_build: false);
	}

	private int GetAdditionalOptions()
	{
		UnityGameEditorDoorProp component = GetComponent<UnityGameEditorDoorProp>();
		if (component != null)
		{
			return (int)component.LockType();
		}
		return 0;
	}

	private void Rebuild()
	{
		Parameters = ParameterCollection.CreateEmpty();
		Parameters.BeginWrite();
		int num = 0;
		num = ((!(GetRoom() != null)) ? GetScenarioSeed() : GetRoomSeed());
		ProceduralStyle proceduralStyle = GloomUtility.EnsureComponent<ProceduralStyle>(base.gameObject);
		ParameterCollection style_struct = Parameters.WriteListBegin(3);
		proceduralStyle.WriteStyleParameters(style_struct, GetScenarioSeed(), num, proceduralStyle.Seed);
		Parameters.WriteListEnd();
		Apparance.Net.Vector3 vector_value = default(Apparance.Net.Vector3);
		if (GetRoom() != null)
		{
			vector_value = Conversion.AVfromUV(GetRoom().transform.position);
		}
		Parameters.WriteVector3(vector_value, 4);
		int additionalOptions = GetAdditionalOptions();
		Parameters.WriteInteger(additionalOptions, 5);
		Parameters.EndWrite();
	}

	private void Apply(bool force_build = true)
	{
		ApparanceEntity component = GetComponent<ApparanceEntity>();
		component.PartialParameterOverride = Parameters;
		component.IsPopulated = false;
		component.NotifyPropertyChanged();
		component.IsPopulated = true;
		component.NotifyPropertyChanged();
	}

	void IProceduralContent.RebuildContent()
	{
		Rebuild();
		Apply();
	}

	public override void NotifyContentPlacementComplete()
	{
		base.NotifyContentPlacementComplete();
		PlacementCompleteAction?.Invoke();
	}

	public override void NotifyContentPlacementStarted()
	{
		base.NotifyContentPlacementStarted();
	}

	public override void NotifyContentRemovalComplete()
	{
		base.NotifyContentRemovalComplete();
	}

	public override void NotifyContentRemovalStarted()
	{
		base.NotifyContentRemovalStarted();
	}
}
