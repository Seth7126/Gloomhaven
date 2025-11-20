namespace Apparance.Net;

public interface IDebugDisplay
{
	void DisplayControl(int geometry_id, bool enable);

	void DisplayHide(int geometry_id, bool hide);

	void DisplayColour(int geometry_id, bool enable, Colour c);
}
