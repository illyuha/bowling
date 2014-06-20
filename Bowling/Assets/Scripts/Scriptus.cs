using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scriptus : MonoBehaviour
{
	// TODO: increase the radius of gutters or decrease it when it falls in a gutter
	private class Player
	{
		public class Frame
		{
			public int[] balls;
			public uint score;
			public Frame(uint numOfballs) // TODO: get rid of the parameter
			{
				balls = new int[numOfballs];
				for (uint i = 0; i < numOfballs; ++i)
					balls[i] = -1;
				score = 0;
			}
		}
		private string _nickname;
		public string nickname // is this necessary?
		{
			get
			{
				return _nickname;
			}
			set
			{
				if (_nickname == null)
					_nickname = value;
			}
		}
		public uint score;
		public Frame[] history;
		public Player(string nick)
		{
			nickname = nick;
			history = new Frame[10];
			score = 0;
		}
		public void recalculateScore(uint points, uint frame, uint ball)
		{
			if (points > 0)
			{
				history[frame].score += points;
				
				if (frame < 9 || ball == 0)
				{
					int fr = (int)frame - 2;
					if (fr >= 0 && history[fr].balls[0] == 10) // strike in the frame before previous
						if (history[fr+1].balls.Length == 1) // strike in the previous frame
							history[fr].score += points;
					fr = (int)frame - 1;
					if (fr >= 0)
					{
						if (history[fr].balls.Length == 1) // strike in the previous frame
							history[fr].score += points;
						else if ((history[fr].balls[0] + history[fr].balls[1] == 10) && (ball == 0)) // spare in the previous frame
							history[fr].score += points;
					}
				}
				else if (ball == 1)
					if (history[8].balls.Length == 1)
						history[8].score += points;
					
			}
			score = 0;
			for (uint i = 0; i <= frame; ++i) // try to get rid of these iterations
				score += history[i].score;
		}
	}
	private Player[] players;
	private uint currentPlayerIndex;
	private uint currentFrame = 0, currentBall = 0;
	private bool ballThrown = false; // TODO: rename
	private bool finish = false;
	private GameObject[] pins;
	public GameObject pinPrefab;
	public const float k = 2.0f; // coefficient of conversion of Blender objects' scale to Unity's ones
	public const float foulLineZ = 4.572f * k, indentZ = k * 3.658f; //0.914f;
	public const float distanceToPins = 18.29f * k; // distance between the foul line and the first pin
	private float pinRadius = 0.06f * k;
	private float distanceBetweenPinsX, distanceBetweenPinsZ;
	private uint knockedDown = 13;
	private LinkedList<GameObject> knockedDownPins; // or uint?
	private float height;
	public const float width = k * 1.05f;
	public GameObject ballPrefab;
	public BallController ballController;
	public Operator cameraman;
	public GameObject textPrefab;
	private GameObject text;//TextMesh
	private Quaternion initialPinRotation; // is it OK?
	private void placePins()
	{
		float initDist = foulLineZ + distanceToPins;
		pins[0] = (GameObject)Instantiate(pinPrefab,new Vector3(0,height,initDist+pinRadius),transform.rotation);
		pins[1] = (GameObject)Instantiate(pinPrefab,
			new Vector3(-2.0f*pinRadius-distanceBetweenPinsX,height,initDist+3.0f*pinRadius+distanceBetweenPinsZ),transform.rotation);
		pins[2] = (GameObject)Instantiate(pinPrefab,
			new Vector3(2.0f*pinRadius+distanceBetweenPinsX,height,initDist+3.0f*pinRadius+distanceBetweenPinsZ),transform.rotation);
		pins[3] = (GameObject)Instantiate(pinPrefab,new Vector3
			(-4.0f*pinRadius-2.0f*distanceBetweenPinsX,height,initDist+5.0f*pinRadius+2.0f*distanceBetweenPinsZ),transform.rotation);
		pins[4] = (GameObject)Instantiate(pinPrefab,new Vector3
			(0,height,initDist+5.0f*pinRadius+2.0f*distanceBetweenPinsZ),transform.rotation);
		pins[5] = (GameObject)Instantiate(pinPrefab,new Vector3
			(4.0f*pinRadius+2.0f*distanceBetweenPinsX,height,initDist+5.0f*pinRadius+2.0f*distanceBetweenPinsZ),transform.rotation);
		pins[6] = (GameObject)Instantiate(pinPrefab,new Vector3
			(-6.0f*pinRadius-3.0f*distanceBetweenPinsX,height,initDist+7.0f*pinRadius+3.0f*distanceBetweenPinsZ),transform.rotation);
		pins[7] = (GameObject)Instantiate(pinPrefab,new Vector3
			(-2.0f*pinRadius-distanceBetweenPinsX,height,initDist+7.0f*pinRadius+3.0f*distanceBetweenPinsZ),transform.rotation);
		pins[8] = (GameObject)Instantiate(pinPrefab,new Vector3
			(2.0f*pinRadius+distanceBetweenPinsX,height,initDist+7.0f*pinRadius+3.0f*distanceBetweenPinsZ),transform.rotation);
		pins[9] = (GameObject)Instantiate(pinPrefab,new Vector3
			(6.0f*pinRadius+3.0f*distanceBetweenPinsX,height,initDist+7.0f*pinRadius+3.0f*distanceBetweenPinsZ),transform.rotation);
	}
	private void inform()
	{
		string message = "";
		if (currentBall == 0)
		{
			if (knockedDown == 10)
				message = "STRIKE!";
		}
		else
		{
			int ball0 = players[currentPlayerIndex].history[currentFrame].balls[0];
			int ball1 = players[currentPlayerIndex].history[currentFrame].balls[1];
			if (currentFrame < 9)
				if ((currentBall == 1) && (ball0 + knockedDown == 10))
					message = "SPARE!";
			else
			{
				if (currentBall == 1)
				{
					if (ball0 < 10 && ball0 + knockedDown == 10)
						message = "SPARE!";
					else if (ball0 == 10 && knockedDown == 10)
						message = "STRIKE!";
				}
				else if (currentBall == 2)
				{
					if (ball0 == 10 && ball1 < 10 && ball1 + knockedDown == 10)
						message = "SPARE!";
					else if (knockedDown == 10)
					{
						if (ball0 == 10 && ball1 == 10)
							message = "STRIKE!";
						else if (ball0 < 10 && ball0 + ball1 == 10)
							message = "STRIKE!";
					}
				}
			}
		}
		if (message == "")
			message = knockedDown.ToString () + " scored.";
		//print(message);
		text.GetComponent<TextMesh>().text = message;
	}
	public IEnumerator calculateScore()
	{
		print ("Current player is "+players[currentPlayerIndex].nickname);
		yield return new WaitForSeconds(5);
		knockedDown = 0;
		knockedDownPins = new LinkedList<GameObject>();
		foreach (GameObject pin in pins)
			if (pin != null)
				if (Mathf.Abs(270-pin.transform.rotation.eulerAngles.x) > 20 || Mathf.Abs(pin.transform.rotation.eulerAngles.z) > 20)
			{
				++knockedDown;
				knockedDownPins.AddLast(pin);
			}
	}
	private IEnumerator prepareNextTurn(bool nextFrame)
	{
		ballThrown = false;
		yield return new WaitForSeconds(1);
		cameraman.prepareForShooting();
		yield return new WaitForSeconds(1);
		if (nextFrame)
		{
			foreach (GameObject pin in pins) // get rid of all the pins
				Destroy (pin);
			print ("All the pins are destroyed");
		}
		else
		{
			foreach (GameObject pin in knockedDownPins) // get rid of knocked down pins
				Destroy (pin);
			print("Knocked down pins are destroyed");
			foreach (GameObject pin in pins)
				if (pin != null)
					pin.transform.rotation = initialPinRotation;
		}
		knockedDownPins = null;
		yield return new WaitForSeconds(2);
		if (nextFrame)
		{
			placePins();
			print ("All the pins are ready");
		}
		ballController.allowThrow();
		text.GetComponent<TextMesh>().text = "Total score is " + players[currentPlayerIndex].score.ToString();
		Trigger.ballDetected = false;
	}
	private void createBall()
	{
		//return ((GameObject)Instantiate(ballPrefab,position,rotation)).GetComponent<Ball>();
		float radius = k * 0.5f * (ballPrefab.renderer.bounds.max.y - ballPrefab.renderer.bounds.min.y);
		ballController.ball = (GameObject)
			Instantiate(ballPrefab,new Vector3(0,height+radius,indentZ),Quaternion.identity);
		// transform.rotation
	}
	private void Start ()
	{
		players = new Player[2];
		//firstPlayer
		players[0] = new Player("Erste");
		//secondPlayer
		players[1] = new Player("Zweite");
		currentPlayerIndex = 0;
		//currentPlayer = firstPlayer;
		pins = new GameObject[10];
		distanceBetweenPinsX = 0.032f * k;
		distanceBetweenPinsZ = 0.221f * k;
		height = renderer.bounds.max.y;
		//width = 
		placePins();
		initialPinRotation = pins[0].transform.rotation;
		//ball = 
		//createBall(new Vector3(0,height,indentZ),transform.rotation);
		createBall();
		ballController.allowThrow ();
		cameraman.ball = ballController.ball;
		text = (GameObject)Instantiate (textPrefab);
		text.GetComponent<TextMesh>().text = "Total score is 0";
		//print (renderer.bounds.max.y);
		//print (renderer.bounds.min.y);
		//print(GameObject.Find ("BackLane").renderer.bounds.max.y);
	}
	private void Update ()
	{
		if (ballController.ballThrown)
		{
			cameraman.startShooting();
			ballController.ballThrown = false;
		}
		if (Trigger.ballDetected)
			cameraman.finishShooting();
		if (knockedDown != 13)
		{
			inform();
			ballThrown = true;
		}
		if (ballThrown && !finish) // make it a separate method?
		{
			print ("Frame: "+currentFrame);
			print ("Ball: "+currentBall);
			Player currentPlayer = players[currentPlayerIndex];
			bool changePlayer = false;
			switch (currentBall)
			{
			case 0:
				if (knockedDown == 10)
				{
					uint balls = (uint)((currentFrame < 9) ? 1 : 3);						
					currentPlayer.history[currentFrame] = new Player.Frame(balls);
					if (balls == 1)
						changePlayer = true;
				}
				else
				{
					currentPlayer.history[currentFrame] = new Player.Frame(2);
					currentBall = 1;
				}
				currentPlayer.history[currentFrame].balls[0] = (int)knockedDown;
				break;
			case 1:
				currentPlayer.history[currentFrame].balls[1] = (int)knockedDown;
				if (currentFrame < 9)
				{
					//currentPlayer.history[currentFrame].balls[1] = (int)knockedDown;
					changePlayer = true;
				}
				else
				{
					if (currentPlayer.history[9].balls[0] + currentPlayer.history[9].balls[1] == 10)
					{
						// TODO: improve!
						int ball0 = currentPlayer.history[9].balls[0], ball1 = currentPlayer.history[9].balls[1];
						currentPlayer.history[9].balls = new int[3];
						currentPlayer.history[9].balls[0] = ball0;
						currentPlayer.history[9].balls[1] = ball1;
					}
					if (currentPlayer.history[9].balls.Length < 3)
						changePlayer = true;
				}
			break;
			case 2:
				currentPlayer.history[currentFrame].balls[2] = (int)knockedDown;
				changePlayer = true;
			break;
			}
			currentPlayer.recalculateScore(knockedDown,currentFrame,currentBall);
			print ("Total score is "+currentPlayer.score);
			ballThrown = false;
			knockedDown = 13;
			if (changePlayer)
			{
				currentBall = 0;
				if (currentPlayerIndex < players.Length - 1)
				{
					++currentPlayerIndex;
					StartCoroutine (prepareNextTurn (true));
				}
				else
				{
					currentPlayerIndex = 0;
					++currentFrame;
					if (currentFrame == 10)
					{
						finish = true;
						print ("Game finished");
					}
					else
						StartCoroutine (prepareNextTurn (true));
				}
			}
			else
				StartCoroutine (prepareNextTurn (false));
		}
	}
}
