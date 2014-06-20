using UnityEngine;
using System.Collections;

public class Operator : MonoBehaviour
{
	public GameObject ball
	{
		get;
		set;
	}
	private bool shoot = false;
	private Vector3 initialPosition;
	public void prepareForShooting()
	{
		transform.position = initialPosition;
	}
	public void startShooting()
	{
		shoot = true;
	}
	public void finishShooting()
	{
		shoot = false;
	}
	private void Start ()
	{
		initialPosition = transform.position;
	}
	private void Update ()
	{
		if (shoot)
		{
			float pos1 = Scriptus.foulLineZ + Scriptus.distanceToPins - 10.0f, pos2 = ball.transform.position.z - 5.0f, pos = pos2;
			if (pos1 < pos2)
			{
				pos = pos1;
				finishShooting ();
			}
			transform.position = new Vector3(ball.transform.position.x,transform.position.y,pos);
		}
	}
}
