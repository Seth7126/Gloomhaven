using UnityEngine;

namespace commanastationwww.multistorydungeons;

public class DisableMeshAtRuntime : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Renderer>().enabled = false;
	}
}
