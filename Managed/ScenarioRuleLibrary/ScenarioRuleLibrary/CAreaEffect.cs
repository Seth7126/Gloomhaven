using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class CAreaEffect
{
	public class CAreaEffectHex
	{
		public Point m_ArrayIndex;

		public bool m_Staggered;

		public bool m_Enabled;

		public CAreaEffectHex(int x, int y, bool enabled = true)
		{
			m_ArrayIndex = new Point(x, y);
			m_Staggered = (y & 1) == 1;
			m_Enabled = enabled;
		}

		public CAreaEffectHex Copy()
		{
			return new CAreaEffectHex(m_ArrayIndex.X, m_ArrayIndex.Y, m_Enabled);
		}
	}

	public string Name { get; private set; }

	public bool Melee { get; private set; }

	public List<CAreaEffectHex> Hexes { get; private set; }

	public List<CAreaEffectHex> EnhancementHexes { get; set; }

	public List<CAreaEffectHex> AllHexes => Hexes.Concat(EnhancementHexes.Where((CAreaEffectHex w) => w.m_Enabled)).ToList();

	public CAreaEffect(string name, bool melee, List<CAreaEffectHex> hexes, List<CAreaEffectHex> enhancementHexes)
	{
		Name = name;
		Melee = melee;
		Hexes = hexes;
		EnhancementHexes = enhancementHexes;
	}

	public CAreaEffect Copy()
	{
		return new CAreaEffect(Name, Melee, Hexes.Select((CAreaEffectHex s) => s.Copy()).ToList(), EnhancementHexes.Select((CAreaEffectHex s) => s.Copy()).ToList());
	}

	public static List<CTile> GetAllPossibleTiles(CActor actor, CAreaEffect areaEffect, int range, List<CTile> innerRange, bool getBlocked, ref List<CTile> validTilesIncludingBlockedOut)
	{
		List<CTile> tilesInRange = GameState.GetTilesInRange(actor, range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true);
		if (areaEffect.Melee)
		{
			return GetTilesAtAllRotations(actor, null, areaEffect, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut);
		}
		CTile tile = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
		List<CTile> list = new List<CTile>();
		list.AddRange(tilesInRange);
		foreach (CTile item in GameState.GetTilesInRange(actor, range + areaEffect.AllHexes.Count, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, innerRange, ignorePathLength: true))
		{
			List<CTile> areaTiles = GetRotatedHexTilesAtAllRotations(tile, item, areaEffect);
			if (tilesInRange.Any((CTile x) => areaTiles.Contains(x)))
			{
				list.AddRange(areaTiles);
			}
		}
		return GetValidTilesFromList(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y], tilesInRange, list.Distinct().ToList(), getBlocked, ref validTilesIncludingBlockedOut);
	}

	public static List<CTile> GetAllPossibleTiles(CTile inTile, CAreaEffect areaEffect, int range, List<CTile> innerRange, bool getBlocked, ref List<CTile> validTilesIncludingBlockedOut)
	{
		List<CTile> tilesInRange = GameState.GetTilesInRange(inTile.m_ArrayIndex, range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true);
		if (areaEffect.Melee)
		{
			return GetTilesAtAllRotations(inTile, null, areaEffect, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut);
		}
		List<CTile> list = new List<CTile>();
		list.AddRange(tilesInRange);
		foreach (CTile item in GameState.GetTilesInRange(inTile.m_ArrayIndex, range + areaEffect.AllHexes.Count, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, innerRange, ignorePathLength: true))
		{
			List<CTile> areaTiles = GetRotatedHexTilesAtAllRotations(inTile, item, areaEffect);
			if (tilesInRange.Any((CTile x) => areaTiles.Contains(x)))
			{
				list.AddRange(areaTiles);
			}
		}
		return GetValidTilesFromList(inTile, tilesInRange, list.Distinct().ToList(), getBlocked, ref validTilesIncludingBlockedOut);
	}

	private static List<CTile> GetTilesAtAllRotations(CActor actor, CTile positionTile, CAreaEffect areaEffect, List<CTile> tilesInRange, bool getBlocked, ref List<CTile> validTilesIncludingBlockedOut)
	{
		List<CTile> validTilesIncludingBlockedOut2 = new List<CTile>();
		List<CTile> validTiles = GetValidTiles(actor, positionTile, areaEffect, 0f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2);
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(actor, positionTile, areaEffect, 60f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(actor, positionTile, areaEffect, 120f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(actor, positionTile, areaEffect, 180f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(actor, positionTile, areaEffect, 240f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(actor, positionTile, areaEffect, 300f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTilesIncludingBlockedOut = validTilesIncludingBlockedOut.Distinct().ToList();
		return validTiles.Distinct().ToList();
	}

	private static List<CTile> GetTilesAtAllRotations(CTile tile, CTile positionTile, CAreaEffect areaEffect, List<CTile> tilesInRange, bool getBlocked, ref List<CTile> validTilesIncludingBlockedOut)
	{
		List<CTile> validTilesIncludingBlockedOut2 = new List<CTile>();
		List<CTile> validTiles = GetValidTiles(tile, positionTile, areaEffect, 0f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2);
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 60f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 120f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 180f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 240f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 300f, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut2));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlockedOut2);
		validTilesIncludingBlockedOut = validTilesIncludingBlockedOut.Distinct().ToList();
		return validTiles.Distinct().ToList();
	}

	private static List<CTile> GetRotatedHexTilesAtAllRotations(CTile tile, CTile positionTile, CAreaEffect areaEffect)
	{
		List<CTile> list = new List<CTile>(areaEffect.AllHexes.Count * 6);
		list.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, areaEffect.AllHexes, 0f));
		list.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, areaEffect.AllHexes, 60f));
		list.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, areaEffect.AllHexes, 120f));
		list.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, areaEffect.AllHexes, 180f));
		list.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, areaEffect.AllHexes, 240f));
		list.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, areaEffect.AllHexes, 300f));
		return list.Distinct().ToList();
	}

	public static List<CTile> GetValidTiles(CActor actor, CTile positionTile, CAreaEffect areaEffect, float rotation, List<CTile> tilesInRange, bool getBlocked, ref List<CTile> validTilesIncludingBlockedOut)
	{
		return GetValidTiles(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y], positionTile, areaEffect, rotation, tilesInRange, getBlocked, ref validTilesIncludingBlockedOut);
	}

	public static List<CTile> GetValidTiles(CTile actorOriginTile, CTile positionTile, CAreaEffect areaEffect, float rotation, List<CTile> tilesInRange, bool getBlocked, ref List<CTile> validTilesIncludingBlockedOut)
	{
		List<CTile> list = new List<CTile>();
		List<CTile> list2 = new List<CTile>();
		CTile cTile = (areaEffect.Melee ? actorOriginTile : positionTile);
		float num = rotation;
		float num2 = 0f;
		if (cTile != null)
		{
			List<Point> list3 = new List<Point>();
			foreach (CAreaEffectHex allHex in areaEffect.AllHexes)
			{
				rotation = num;
				MF.ArrayIndexToCartesianCoord(allHex.m_ArrayIndex, 1f, 1f, out var x, out var y);
				if (Math.Abs(allHex.m_ArrayIndex.X) > 3)
				{
					if (rotation == 300f)
					{
						rotation -= 5f;
						if (allHex.m_ArrayIndex.Y < 0)
						{
							num2 = 0.1f;
						}
					}
					if (rotation == 60f)
					{
						rotation += 5f;
						if (allHex.m_ArrayIndex.Y > 0)
						{
							num2 = 0.1f;
						}
					}
					if (rotation == 120f)
					{
						rotation -= 5f;
						if (allHex.m_ArrayIndex.Y < 0)
						{
							num2 = -0.1f;
						}
					}
					if (rotation == 240f)
					{
						rotation += 5f;
						if (allHex.m_ArrayIndex.Y > 0)
						{
							num2 = -0.1f;
						}
					}
				}
				float num3 = (float)Math.Cos((double)rotation * (Math.PI / 180.0));
				float num4 = (float)Math.Sin((double)rotation * (Math.PI / 180.0));
				float num5 = x * num3 - y * num4;
				float num6 = x * num4 + y * num3;
				MF.ArrayIndexToCartesianCoord(cTile.m_ArrayIndex, 1f, 1f, out var x2, out var y2);
				float num7 = x2 + num5 + num2;
				float val = y2 + num6;
				int num8 = (int)MF.SnapFloat(1f, val);
				int x3 = (((num8 & 1) != 1) ? ((int)MF.SnapFloat(1f, num7)) : ((int)(MF.SnapFloat(1f, num7 - 0.5f) + 0.5f)));
				list3.Add(new Point(x3, num8));
			}
			foreach (Point item in list3)
			{
				try
				{
					CTile cTile2 = ScenarioManager.Tiles[item.X, item.Y];
					if (cTile2 == null)
					{
						continue;
					}
					ScenarioManager.PathFinder.FindPath(actorOriginTile.m_ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out var foundPath);
					if (foundPath)
					{
						if (CActor.HaveLOS(ScenarioManager.Tiles[actorOriginTile.m_ArrayIndex.X, actorOriginTile.m_ArrayIndex.Y], cTile2))
						{
							list.Add(cTile2);
							list2.Add(cTile2);
						}
					}
					else
					{
						ScenarioManager.PathFinder.FindPath(actorOriginTile.m_ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
						if (foundPath && CActor.HaveLOS(ScenarioManager.Tiles[actorOriginTile.m_ArrayIndex.X, actorOriginTile.m_ArrayIndex.Y], cTile2))
						{
							list2.Add(cTile2);
						}
					}
				}
				catch
				{
				}
			}
		}
		if (tilesInRange != null && !list2.Any((CTile item) => tilesInRange.Contains(item)))
		{
			list.Clear();
			list2.Clear();
		}
		list = list.Distinct().ToList();
		list2 = list2.Distinct().ToList();
		if (getBlocked)
		{
			validTilesIncludingBlockedOut = list2;
		}
		return list.Distinct().ToList();
	}

	public static List<CTile> GetRotatedHexTiles(CTile actorOriginTile, CTile positionTile, bool melee, List<CAreaEffectHex> hexList, float rotation)
	{
		CTile cTile = (melee ? actorOriginTile : positionTile);
		float num = rotation;
		float num2 = 0f;
		List<CTile> list = new List<CTile>(hexList.Count);
		if (cTile != null)
		{
			foreach (CAreaEffectHex hex in hexList)
			{
				rotation = num;
				MF.ArrayIndexToCartesianCoord(hex.m_ArrayIndex, 1f, 1f, out var x, out var y);
				if (Math.Abs(hex.m_ArrayIndex.X) > 3)
				{
					if (rotation == 300f)
					{
						rotation -= 5f;
						if (hex.m_ArrayIndex.Y < 0)
						{
							num2 = 0.1f;
						}
					}
					if (rotation == 60f)
					{
						rotation += 5f;
						if (hex.m_ArrayIndex.Y > 0)
						{
							num2 = 0.1f;
						}
					}
					if (rotation == 120f)
					{
						rotation -= 5f;
						if (hex.m_ArrayIndex.Y < 0)
						{
							num2 = -0.1f;
						}
					}
					if (rotation == 240f)
					{
						rotation += 5f;
						if (hex.m_ArrayIndex.Y > 0)
						{
							num2 = -0.1f;
						}
					}
				}
				float num3 = (float)Math.Cos((double)rotation * (Math.PI / 180.0));
				float num4 = (float)Math.Sin((double)rotation * (Math.PI / 180.0));
				float num5 = x * num3 - y * num4;
				float num6 = x * num4 + y * num3;
				MF.ArrayIndexToCartesianCoord(cTile.m_ArrayIndex, 1f, 1f, out var x2, out var y2);
				float num7 = x2 + num5 + num2;
				float val = y2 + num6;
				int num8 = (int)MF.SnapFloat(1f, val);
				int num9 = (((num8 & 1) != 1) ? ((int)MF.SnapFloat(1f, num7)) : ((int)(MF.SnapFloat(1f, num7 - 0.5f) + 0.5f)));
				if (num9 >= 0 && num9 < ScenarioManager.Width && num8 >= 0 && num8 < ScenarioManager.Height)
				{
					CTile cTile2 = ScenarioManager.Tiles[num9, num8];
					if (cTile2 != null)
					{
						list.Add(cTile2);
					}
				}
			}
		}
		return list.Distinct().ToList();
	}

	public static List<CTile> GetValidTilesFromList(CTile actorOriginTile, List<CTile> tilesInRange, List<CTile> tiles, bool getBlocked, ref List<CTile> validTilesIncludingBlockedOut)
	{
		List<CTile> list = new List<CTile>();
		List<CTile> list2 = new List<CTile>();
		foreach (CTile tile in tiles)
		{
			ScenarioManager.PathFinder.FindPath(actorOriginTile.m_ArrayIndex, tile.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out var foundPath);
			if (foundPath)
			{
				if (CActor.HaveLOS(ScenarioManager.Tiles[actorOriginTile.m_ArrayIndex.X, actorOriginTile.m_ArrayIndex.Y], tile))
				{
					list.Add(tile);
					list2.Add(tile);
				}
			}
			else
			{
				ScenarioManager.PathFinder.FindPath(actorOriginTile.m_ArrayIndex, tile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
				if (foundPath && CActor.HaveLOS(ScenarioManager.Tiles[actorOriginTile.m_ArrayIndex.X, actorOriginTile.m_ArrayIndex.Y], tile))
				{
					list2.Add(tile);
				}
			}
		}
		if (tilesInRange != null && !list2.Any((CTile x) => tilesInRange.Contains(x)))
		{
			list.Clear();
			list2.Clear();
		}
		list = list.Distinct().ToList();
		list2 = list2.Distinct().ToList();
		if (getBlocked)
		{
			validTilesIncludingBlockedOut = list2;
		}
		return list.Distinct().ToList();
	}
}
