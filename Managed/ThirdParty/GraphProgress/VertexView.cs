using System;
using TMPro;
using UnityEngine;

namespace GraphProgress;

[RequireComponent(typeof(SpriteRenderer))]
public class VertexView : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro _textMeshPro;

	[SerializeField]
	private AnimationCurve _animationCurveSize;

	private SpriteRenderer _spriteRenderer;

	private readonly Color _unlockLine = Color.black;

	private readonly Color _requiredLine = Color.green;

	private readonly Color _blockLine = Color.red;

	private readonly Color _defaultQuest = Color.gray;

	private readonly Color _visibleQuest = Color.white;

	private readonly Color _avaliableQuest = Color.yellow;

	private readonly Color _completedQuest = Color.green;

	private readonly Color _blockedQuest = Color.red;

	private CampaignGraphViewController _campaignGraphViewController;

	private QuestVertex _questVertex;

	private Action<int> _clickCallback;

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Init(CampaignGraphViewController campaignGraphViewController, QuestVertex questVertex, Action<int> clickCallback)
	{
		_campaignGraphViewController = campaignGraphViewController;
		_questVertex = questVertex;
		TextMeshPro textMeshPro = _textMeshPro;
		string text = (base.gameObject.name = _questVertex.Id.ToString());
		textMeshPro.text = text;
		_clickCallback = clickCallback;
	}

	public void Click()
	{
		_clickCallback?.Invoke(_questVertex.Id);
	}

	public void CreateEdges()
	{
		Material material = new Material(Shader.Find("Sprites/Default"));
		foreach (QuestEdge questEdge in _questVertex.QuestEdges)
		{
			GameObject obj = new GameObject();
			obj.transform.parent = base.transform;
			LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();
			lineRenderer.widthCurve = _animationCurveSize;
			lineRenderer.positionCount = 2;
			lineRenderer.SetPosition(0, _campaignGraphViewController.GetVertexPosition(_questVertex.Id));
			lineRenderer.SetPosition(1, _campaignGraphViewController.GetVertexPosition(questEdge.From.Id));
			lineRenderer.material = material;
			if (questEdge.ConditionType == ConditionType.Unclock)
			{
				lineRenderer.startColor = _unlockLine;
				lineRenderer.endColor = _unlockLine;
			}
			else if (questEdge.ConditionType == ConditionType.Required)
			{
				lineRenderer.startColor = _requiredLine;
				lineRenderer.endColor = _requiredLine;
			}
			else if (questEdge.ConditionType == ConditionType.Block)
			{
				lineRenderer.startColor = _blockLine;
				lineRenderer.endColor = _blockLine;
			}
		}
	}

	public void UpdateView()
	{
		switch (_questVertex.QuestType)
		{
		case QuestType.Default:
			_spriteRenderer.color = _defaultQuest;
			break;
		case QuestType.Visible:
			_spriteRenderer.color = _visibleQuest;
			break;
		case QuestType.Available:
			_spriteRenderer.color = _avaliableQuest;
			break;
		case QuestType.Completed:
			_spriteRenderer.color = _completedQuest;
			break;
		case QuestType.Blocked:
			_spriteRenderer.color = _blockedQuest;
			break;
		default:
			_spriteRenderer.color = _defaultQuest;
			break;
		}
	}

	public void Active()
	{
		_spriteRenderer.color = _completedQuest;
	}

	public void Reset()
	{
		_spriteRenderer.color = _defaultQuest;
	}
}
