using UnityEngine;
using UnityEngine.UI;

public static class GridLayoutGroupExtensions
{
	public static Vector2 GetCellCountPerAxis(this GridLayoutGroup gridLayout, int cells)
	{
		int num = 1;
		int num2 = 1;
		Vector2 size = gridLayout.GetComponent<RectTransform>().rect.size;
		if (gridLayout.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
		{
			num = gridLayout.constraintCount;
			num2 = Mathf.CeilToInt((float)cells / (float)num - 0.001f);
		}
		else if (gridLayout.constraint == GridLayoutGroup.Constraint.FixedRowCount)
		{
			num2 = gridLayout.constraintCount;
			num = Mathf.CeilToInt((float)cells / (float)num2 - 0.001f);
		}
		else
		{
			num = Mathf.Max(1, Mathf.FloorToInt((size.x - (float)gridLayout.padding.horizontal + gridLayout.spacing.x + 0.001f) / (gridLayout.cellSize.x + gridLayout.spacing.x)));
			num2 = Mathf.Max(1, Mathf.FloorToInt((size.y - (float)gridLayout.padding.vertical + gridLayout.spacing.y + 0.001f) / (gridLayout.cellSize.y + gridLayout.spacing.y)));
		}
		int num3;
		int num4;
		if (gridLayout.startAxis == GridLayoutGroup.Axis.Horizontal)
		{
			num3 = Mathf.Clamp(num, 1, cells);
			num4 = Mathf.Clamp(num2, 1, Mathf.CeilToInt((float)cells / (float)num));
		}
		else
		{
			num4 = Mathf.Clamp(num2, 1, cells);
			num3 = Mathf.Clamp(num, 1, Mathf.CeilToInt((float)cells / (float)num4));
		}
		return new Vector2(num3, num4);
	}
}
