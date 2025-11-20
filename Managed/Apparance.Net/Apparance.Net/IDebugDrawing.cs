namespace Apparance.Net;

public interface IDebugDrawing
{
	void ResetDrawing();

	void DrawLine(Vector3 start, Vector3 end, Colour colour);

	void DrawBox(Frame frame, Colour colour);

	void DrawSphere(Vector3 centre, float radius, Colour colour);
}
