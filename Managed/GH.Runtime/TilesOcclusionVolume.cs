using UnityEngine;

public class TilesOcclusionVolume : MonoBehaviour
{
	public TileBehaviour CentralTile;

	public MeshRenderer[] Renderers;

	private void Start()
	{
		if (TilesOcclusionGenerator.s_Instance != null)
		{
			TilesOcclusionGenerator.s_Instance.AddVolume(this);
		}
	}

	public bool IsVisible()
	{
		if (CentralTile == null)
		{
			Debug.LogError("Central tile is NULL, can't define is volume visible or not");
			return false;
		}
		if (SaveData.Instance.Global.GameMode == EGameMode.LevelEditor)
		{
			return LevelEditorController.s_Instance.GetRoomVisibilityOverride(CentralTile?.m_ClientTile?.m_Tile?.m_HexMap);
		}
		return CentralTile.m_ClientTile.m_Tile.m_HexMap.Revealed;
	}
}
