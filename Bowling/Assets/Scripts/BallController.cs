using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
	public GameObject ball
	{
		get;
		set;
	}
	private float step = 0.05f;
	private float dir = 0.0f;
	private bool turn = false;
	private Vector3 initialPosition;
	public bool ballThrown
	{
		get;
		set;
	}
	public void allowThrow()
	{
		turn = true;
		ball.transform.position = initialPosition;
		ball.rigidbody.velocity = Vector3.zero;
		ball.rigidbody.useGravity = false;
		ball.rigidbody.isKinematic = true;
		ball.transform.rotation = Quaternion.AngleAxis(0,new Vector3(0,0,0));
	}
	private void Start ()
	{
		initialPosition = transform.position;
		ballThrown = false;
	}
	private void Update () // should the rigidbody code be in FixedUpdate()?
	{
		if (turn)
		{
			if (Input.GetKeyDown (KeyCode.RightArrow))
				dir += 1;
			if (Input.GetKeyDown (KeyCode.LeftArrow))
				dir -= 1;
			if (Input.GetKeyUp (KeyCode.RightArrow))
				dir -= 1;
			if (Input.GetKeyUp (KeyCode.LeftArrow))
				dir += 1;
			if ((dir != 0) && (Mathf.Abs(dir * step + ball.transform.position.x) <= Scriptus.width * 0.5f))
				ball.transform.Translate(new Vector3(dir*step,0,0));
			float mouseY = Input.GetAxis("Mouse Y");
			float mouseX = Input.GetAxis("Mouse X");
			if (Input.GetMouseButton(0)) // holding left mouse
			{
				/*RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray,out hit))
				{
					float z = hit.point.z;
					if (z >= indentZ && z <= foulLineZ)
						transform.position = new Vector3(transform.position.x,transform.position.y,z);
				}*/
				float z = ball.transform.position.z + 0.4f * mouseY;
				if (z >= Scriptus.indentZ && z <= Scriptus.foulLineZ)
					ball.transform.position = new Vector3(ball.transform.position.x, ball.transform.position.y, z);
			}
			if (Input.GetMouseButtonUp(0) && mouseY >= 0) // left mouse unpressed
			{
				// TODO: check for VERY slow velocity
				ball.rigidbody.isKinematic = false;
				if (mouseY < 4.0f)
					mouseY = 4.0f;
				print ("Mouse Y: "+mouseY);
				ball.rigidbody.velocity = new Vector3(0.7f*mouseX,0,2.0f*mouseY);
				ball.rigidbody.useGravity = true;
				turn = false;
				ballThrown = true;
			}
		}
	}
}
