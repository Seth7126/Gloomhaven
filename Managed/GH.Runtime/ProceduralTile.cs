using System.ComponentModel;
using Script.Controller;
using UnityEngine;

[ExecuteInEditMode]
public class ProceduralTile : MonoBehaviour, IProceduralTile
{
	public float ContentHeight;

	public float ContentRadius;

	public int TileFlags;

	[Description("Should this be included in the tile list (to be avoided)?")]
	public bool Include = true;

	[Description("Should have it's position snapped to the hex grid?")]
	public bool Snap = true;

	private Vector3 previousPosition = offGrid;

	private Vector3 previousSnapPosition = offGrid;

	private float previousContentRadius;

	private float previousContentHeight;

	private bool previousActive;

	private bool initialInit;

	private static Vector3 offGrid = new Vector3(100000f, 100000f, 0f);

	bool IProceduralTile.IsActive
	{
		get
		{
			if (Include)
			{
				return base.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	float IProceduralTile.ContentHeight => ContentHeight;

	float IProceduralTile.ContentRadius => ContentRadius;

	int IProceduralTile.Flags => TileFlags;

	float IProceduralTile.Orientation => base.transform.rotation.eulerAngles.y;

	Vector3 IProceduralTile.Position => base.transform.position;

	private void Start()
	{
		initialInit = true;
	}

	private void OnValidate()
	{
		Update();
	}

	private void OnEnable()
	{
		Singleton<ObjectCacheService>.Instance.AddProceduralTile(this);
		initialInit = true;
	}

	private void OnDisable()
	{
		if (Singleton<ObjectCacheService>.Instance != null)
		{
			Singleton<ObjectCacheService>.Instance.RemoveProceduralTile(this);
		}
		ProceduralTileTracker.NotifyTileMove(base.gameObject, previousSnapPosition, previousSnapPosition);
		previousPosition = offGrid;
		previousSnapPosition = offGrid;
		previousContentRadius = 0f;
		previousContentHeight = 0f;
		previousActive = false;
		initialInit = false;
	}

	private void Update()
	{
		if (base.gameObject.scene.name == null || base.gameObject.scene.rootCount == 0)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (position != previousPosition || initialInit)
		{
			initialInit = false;
			if (Snap && ProceduralTileTracker.SnapTilePosition(ref position))
			{
				base.transform.position = position;
			}
			previousPosition = position;
			if (position != previousSnapPosition)
			{
				ProceduralTileTracker.NotifyTileMove(base.gameObject, previousSnapPosition, position);
				previousSnapPosition = position;
			}
		}
		if (!Application.isPlaying)
		{
			bool activeInHierarchy = base.gameObject.activeInHierarchy;
			if (ContentRadius != previousContentRadius || ContentHeight != previousContentHeight || previousActive != activeInHierarchy)
			{
				ProceduralTileTracker.NotifyTileMove(base.gameObject, position, position);
				previousContentRadius = ContentRadius;
				previousContentHeight = ContentHeight;
				previousActive = activeInHierarchy;
			}
		}
	}

	internal ProceduralMapTile FindMapTileByPosition(Vector3 position, Color diags_colour, out Vector3 wall_size)
	{
		Vector2 p = new Vector2(position.x, position.z);
		float num = 1000f;
		ProceduralWall proceduralWall = null;
		foreach (ProceduralWall item in ProceduralWall.FindWallsNear(position, 2.2f))
		{
			Vector3 vector = (item.LeftCorner.Position + item.RightCorner.Position) * 0.5f;
			Vector2 vector2 = new Vector2(vector.x, vector.z);
			Vector2 vector3 = new Vector2(0f - item.transform.forward.x, 0f - item.transform.forward.z);
			float num2 = ProceduralWallAnalyser.ProjectPointOntoSection(vector2, vector2 + vector3, p);
			if (num2 > 0.1f && num2 < num)
			{
				num = num2;
				proceduralWall = item;
			}
		}
		if (proceduralWall != null)
		{
			wall_size = proceduralWall.GetComponent<BoxCollider>().size;
			return proceduralWall.GetComponentInParent<ProceduralMapTile>();
		}
		wall_size = Vector3.zero;
		return null;
	}
}
