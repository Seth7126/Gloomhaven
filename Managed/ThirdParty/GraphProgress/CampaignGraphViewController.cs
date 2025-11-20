#define ENABLE_LOGS
using System.Collections.Generic;
using SM.Utils;
using UnityEngine;

namespace GraphProgress;

public class CampaignGraphViewController : MonoBehaviour
{
	[SerializeField]
	private InfoView _infoView;

	[SerializeField]
	private List<Vector2> _vertexPositions = new List<Vector2>(51);

	[SerializeField]
	private VertexView _templeVertexView;

	private List<VertexView> _vertexViews = new List<VertexView>(51);

	private const string GraphGOName = "Graph";

	private IReadOnlyList<QuestVertex> QuestVertices => CampaignProgressGraph.Instance.QuestVertices;

	private void Awake()
	{
		CreateVertexViews();
		CalculateAndSetVertexCost();
		_vertexViews[0].Click();
	}

	public Vector3 GetVertexPosition(int id)
	{
		return _vertexPositions[id - 1];
	}

	private void CreateVertexViews()
	{
		GameObject gameObject = GameObject.Find("Graph");
		if (gameObject != null)
		{
			Object.DestroyImmediate(gameObject);
			_vertexViews.Clear();
		}
		GameObject gameObject2 = new GameObject();
		gameObject2.name = "Graph";
		foreach (QuestVertex questVertex in QuestVertices)
		{
			VertexView vertexView = Object.Instantiate(_templeVertexView, GetVertexPosition(questVertex.Id), Quaternion.identity, gameObject2.transform);
			vertexView.Init(this, questVertex, OnVertexViewClicked);
			_vertexViews.Add(vertexView);
		}
		foreach (VertexView vertexView2 in _vertexViews)
		{
			vertexView2.CreateEdges();
		}
	}

	private void CalculateAndSetVertexCost()
	{
		List<int> list = new List<int>();
		QuestVertices[50].GetNeededQuestsToUnlock(list);
		list.Add(51);
		float cost = 100f / (float)list.Count;
		list.Sort();
		foreach (int item in list)
		{
			QuestVertices[item - 1].SetCost(cost);
			LogUtils.Log(item.ToString());
		}
	}

	private void OnVertexViewClicked(int id)
	{
		CampaignProgressGraph.Instance.TryCompleteQuest(id);
		UpdatesVertexViews();
		_infoView.UpdateView(id, CampaignProgressGraph.Instance.CalculateProgress());
	}

	private void UpdatesVertexViews()
	{
		foreach (VertexView vertexView in _vertexViews)
		{
			vertexView.UpdateView();
		}
	}
}
