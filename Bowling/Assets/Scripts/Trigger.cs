using UnityEngine;
using System.Collections;

public class Trigger : MonoBehaviour
{
	public static bool ballDetected
	{
		get;
		set;
	}
	private void OnTriggerEnter(Collider ball)
	{
		if (!ballDetected)
		{
			StartCoroutine(transform.parent.GetComponent<Scriptus>().calculateScore());
			ballDetected = true;
		}
	}
	private void Start ()
	{
		ballDetected = false;
	}
	private void Update ()
	{
	}
}
