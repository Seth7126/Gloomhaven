#define ENABLE_LOGS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralPlacementNotifierHandler : MonoBehaviour
{
	private List<ProceduralBase> _proceduralBases;

	private Dictionary<ProceduralBase, ProceduralPlacementLoadingModel> _proceduralPlacementLoadingModels;

	private void Awake()
	{
		_proceduralBases = new List<ProceduralBase>();
		_proceduralPlacementLoadingModels = new Dictionary<ProceduralBase, ProceduralPlacementLoadingModel>();
	}

	public void UpdateProceduralCache(GameObject root)
	{
		_proceduralBases.Clear();
		root.GetComponentsInChildren(includeInactive: true, _proceduralBases);
	}

	public IEnumerator WaitForPlacementCompletedAll(int maxFramesToWait)
	{
		WaitForEndOfFrame waitForEndFrame = new WaitForEndOfFrame();
		int currentFrame = 0;
		_proceduralPlacementLoadingModels = _proceduralBases.ToDictionary((ProceduralBase x) => x, (ProceduralBase y) => new ProceduralPlacementLoadingModel
		{
			IsCompleted = y.IsPlacementCompleted
		});
		foreach (ProceduralBase proceduralBasis in _proceduralBases)
		{
			proceduralBasis.ContentPlacementCompleted += ProceduralBaseOnContentPlacementCompleted;
		}
		while (!_proceduralPlacementLoadingModels.All((KeyValuePair<ProceduralBase, ProceduralPlacementLoadingModel> x) => x.Value.IsCompleted) && currentFrame <= maxFramesToWait)
		{
			Debug.LogWarning($"CURRENT FRAME: {currentFrame}");
			currentFrame++;
			yield return waitForEndFrame;
		}
	}

	private void ProceduralBaseOnContentPlacementCompleted(ProceduralBase proceduralBase)
	{
		proceduralBase.ContentPlacementCompleted -= ProceduralBaseOnContentPlacementCompleted;
		_proceduralPlacementLoadingModels[proceduralBase].IsCompleted = true;
	}
}
