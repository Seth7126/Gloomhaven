using BeautifyEffect;
using UnityEngine;

public class VignetteFadeScript : MonoBehaviour
{
	private Color vignetteColour;

	public bool getColourLowFromComponent = true;

	public Color vignetteColourLow = new Color(0f, 0f, 0f, 0.05f);

	public Color vignetteColourHigh = new Color(0f, 0f, 0f, 0.14f);

	public float yPosLow = 6.23f;

	public float yPosHigh = 16.23f;

	private float yPos;

	private int frames;

	private void Start()
	{
		vignetteColour = base.gameObject.GetComponent<Beautify>().vignettingColor;
		if (getColourLowFromComponent)
		{
			vignetteColourLow = vignetteColour;
		}
		yPos = base.gameObject.transform.position.y;
	}

	private void Update()
	{
		frames++;
		if (base.gameObject.transform.position.y != yPos || frames % 30 == 0)
		{
			frames = 0;
			yPos = base.gameObject.transform.position.y;
			float t = (yPos - yPosLow) / (yPosHigh - yPosLow);
			vignetteColour = Color.Lerp(vignetteColourLow, vignetteColourHigh, t);
			base.gameObject.GetComponent<Beautify>().vignettingColor = vignetteColour;
		}
	}
}
