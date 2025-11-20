using TMPro;
using UnityEngine;

namespace GraphProgress;

[RequireComponent(typeof(TextMeshProUGUI))]
public class InfoView : MonoBehaviour
{
	private TextMeshProUGUI _tmPro;

	private void Awake()
	{
		_tmPro = GetComponent<TextMeshProUGUI>();
	}

	public void UpdateView(int clickedVertex, float cost)
	{
		_tmPro.text = $"Clicked vertex id - {clickedVertex} \nTotal cost - {cost}";
	}
}
