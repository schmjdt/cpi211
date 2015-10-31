using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour {

	public float speed;

	Vector3 initPos;
	float initXpose;
	Vector3 newPos;
	Vector3 lastPos;
	Vector3 currentPos;

	public GameObject cube;

	public Camera cam;

	bool isSpinning;

	// Use this for initialization
	void Start () {
		isSpinning = false;
	}
	
	// Update is called once per frame
	void Update () {
		
			//current position of mouse
			currentPos = Input.mousePosition;
			 
			//get all position along with mouse pointer movement
			newPos = cam.ScreenToWorldPoint (new Vector3(currentPos.x,currentPos.y,Mathf.Clamp(currentPos.y/10,10,50)));
			 

			if (isSpinning && transform.position.y < .26f) {
				isSpinning = false;
				GetDiceCount();
			}
			if (transform.position.y > .26f) 
				isSpinning = true;
 	}

 	void OnMouseDown() {

			//initial click to roll a dice
			initPos = Input.mousePosition;
			 
			//return x component of dice from screen to view point
			initXpose = cam.ScreenToViewportPoint (Input.mousePosition).x;
 	}

 	void OnMouseUp() {

			//translate from screen to world coordinates  
			newPos = cam.ScreenToWorldPoint (currentPos);

			initPos = cam.ScreenToWorldPoint (initPos);
			 
			//Method use to roll the dice
			if (!isSpinning)
				RollTheDice(newPos);
			//use identify face value on dice
			//StartCoroutine(GetDiceCount());
 	}

	//Method Roll the Dice
	void RollTheDice(Vector3 lastPos)
	{
		//Debug.Log("lastPos: " + lastPos + " initPos: " + initPos);
		Vector3 test2 = new Vector3(0,0,0);
		Vector3 test1 = new Vector3(0,0,0);
		while (Vector3.Cross(test1, test2) == new Vector3(0.0f, 0.0f, 0.0f)) {
			test1 = new Vector3(Random.Range(-1,1), Random.Range(-1,1), Random.Range(-1,1));
			test2 = new Vector3(Random.Range(-1,1), Random.Range(-1,1), Random.Range(-1,1));
		}
		lastPos = test2;
		initPos = test1;
		//Debug.Log("test1: " + test1 + " test2: " + test2 + " Cross: "+ Vector3.Cross(test1, test2));
	    gameObject.GetComponent<Rigidbody>().AddTorque(Vector3.Cross(test1, test2) * 1000, ForceMode.Impulse);
		lastPos.y += 12;
		gameObject.GetComponent<Rigidbody>().AddForce (((lastPos - initPos).normalized) * (Vector3.Distance (lastPos, initPos)) * 25 * gameObject.GetComponent<Rigidbody>().mass);

	}



	//Coroutine to get dice count
	void GetDiceCount()
	{
		int diceCount = 0;
		if (Vector3.Dot (transform.forward, Vector3.up) > .99f)
		diceCount = 5;
		if (Vector3.Dot (-transform.forward, Vector3.up) > .99f)
		diceCount = 2;
		if (Vector3.Dot (transform.up, Vector3.up) > .99f)
		diceCount = 3;
		if (Vector3.Dot (-transform.up, Vector3.up) > .99f)
		diceCount = 4;
		if (Vector3.Dot (transform.right, Vector3.up) > .99f)
		diceCount = 6;
		if (Vector3.Dot (-transform.right, Vector3.up) > .99f)
		diceCount = 1;
		if (diceCount != 0) Debug.Log ("diceCount :" + diceCount);
	}
}
