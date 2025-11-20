#define ENABLE_LOGS
using ScenarioRuleLibrary;
using UnityEngine;

public class CustomObjectPositionToChildMaterials : MonoBehaviour
{
	private static readonly int _toggleFadeSourcePos = Shader.PropertyToID("_ToggleFadeSourcePos");

	private static readonly int _fadeSourcePos = Shader.PropertyToID("_FadeSourcePos");

	public GameObject fadeSourceObject;

	public bool enableFade;

	private Renderer[] childRenderers;

	private bool isFading;

	private bool toggleFadeProperty = true;

	public Renderer[] overrideChildRenderers;

	private void Start()
	{
		bool flag = true;
		if (overrideChildRenderers.Length >= 1)
		{
			flag = false;
		}
		Debug.Log(flag);
		if (flag)
		{
			childRenderers = GetComponentsInChildren<Renderer>();
		}
		else
		{
			childRenderers = overrideChildRenderers;
		}
		UnityGameEditorObject componentInParent = GetComponentInParent<UnityGameEditorObject>();
		if (componentInParent == null || componentInParent.m_ObjectType != ScenarioManager.ObjectImportType.Obstacle)
		{
			return;
		}
		CObjectObstacle cObjectObstacle = (CObjectObstacle)componentInParent.PropObject;
		if (!ScenarioManager.CurrentScenarioState.TransparentProps.Contains(cObjectObstacle))
		{
			return;
		}
		CActor cActor = null;
		foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
		{
			CTile cTile = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
			CActor cActor2 = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
			if (cActor2 != null)
			{
				cActor = cActor2;
			}
		}
		if (cActor == null)
		{
			ScenarioManager.CurrentScenarioState.TransparentProps.Remove(cObjectObstacle);
			fadeSourceObject = null;
			enableFade = false;
		}
		else
		{
			fadeSourceObject = Choreographer.s_Choreographer.FindClientActorGameObject(cActor);
			enableFade = true;
		}
	}

	private void Update()
	{
		if (enableFade && fadeSourceObject != null && childRenderers.Length != 0)
		{
			isFading = true;
			Vector3 position = fadeSourceObject.transform.position;
			Renderer[] array;
			if (toggleFadeProperty)
			{
				array = childRenderers;
				foreach (Renderer renderer in array)
				{
					if (!(renderer == null) && renderer.materials.Length != 0)
					{
						if (renderer.material.HasProperty(_toggleFadeSourcePos))
						{
							renderer.material.SetFloat(_toggleFadeSourcePos, 1f);
						}
						else
						{
							Debug.Log("Material needs to use Amp_Basic_Prop_Shader!");
						}
					}
				}
				toggleFadeProperty = false;
			}
			array = childRenderers;
			foreach (Renderer renderer2 in array)
			{
				if (!(renderer2 == null) && renderer2.materials.Length != 0)
				{
					renderer2.material.SetVector(_fadeSourcePos, position);
				}
			}
		}
		else
		{
			if (enableFade || !isFading)
			{
				return;
			}
			isFading = false;
			Renderer[] array = childRenderers;
			foreach (Renderer renderer3 in array)
			{
				if (!(renderer3 == null) && renderer3.materials.Length != 0)
				{
					renderer3.material.SetFloat(_toggleFadeSourcePos, 0f);
				}
			}
			toggleFadeProperty = true;
		}
	}
}
