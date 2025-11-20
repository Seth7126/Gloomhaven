using UnityEngine;

namespace commanastationwww.multistorydungeons;

public class Move : MonoBehaviour
{
	private float speed = 7f;

	private float gravity = 1000f;

	private float horizontalMovement;

	private float verticalMovement;

	private CharacterController character;

	private Vector3 destination = Vector3.zero;

	private void Start()
	{
		character = GetComponent<CharacterController>();
	}

	private void Update()
	{
		horizontalMovement = Input.GetAxis("Horizontal");
		verticalMovement = Input.GetAxis("Vertical");
		destination.Set(horizontalMovement, 0f, verticalMovement);
		destination = base.transform.TransformDirection(destination);
		destination *= speed;
		destination.y -= gravity * Time.deltaTime;
		character.Move(destination * Time.deltaTime);
	}
}
