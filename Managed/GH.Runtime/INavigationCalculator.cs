using UnityEngine.UI;

public interface INavigationCalculator
{
	Selectable FindLeft();

	Selectable FindRight();

	Selectable FindUp();

	Selectable FindDown();
}
