using UnityEngine;
using System.Collections;

public class PreventCrash : MonoBehaviour {

	public Rigidbody car;
	public Transform carT;

	private Vector3 zero = new Vector3 (0, 0, 0);
	private Vector3 vel;
	private bool stopped = false;
	private double timeCount = 0;
	private double timeBeforeTurn = -1;
	private double timeBeforeContinue = -1;
	private string turnDirection = "straight";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		//responsible for turning car at intersections
		if (timeBeforeTurn != -1) {
			timeCount += Time.deltaTime;
			if (timeCount >= timeBeforeTurn) {
				if (turnDirection != "straight") {
					carT.Rotate (new Vector3 (0, 90, 0));

					float mult;

					if (turnDirection == "right") {
						mult = 1;
					} else {
						mult = -1;
					}

					if (car.velocity == new Vector3 (0, 0, 10) ||
					    car.velocity == new Vector3 (0, 0, -10)) {
						car.velocity = new Vector3 (car.velocity.z * mult, 0, 0);
					} else {
						car.velocity = new Vector3 (0, 0, car.velocity.x * mult * -1f);
					}
				}

				timeBeforeTurn = -1;
				timeCount = 0;
			}
		} else if (timeBeforeContinue != -1) {
			timeCount += Time.deltaTime;
			if (timeCount >= timeBeforeContinue) {
				car.velocity = vel;
				timeCount = 0;
				timeBeforeContinue = -1;
			}
		}
	
	}

	void OnTriggerEnter(Collider other){


		if (other.gameObject.name.Length < 5) {
			return;
		}
			
		//stop if approaching danger
		if ((other.gameObject.name == "Shield" && (car.velocity != zero)) ||
			(other.gameObject.name.Substring(0,5) == "Cross" && (timeBeforeTurn == -1))) {
			stopped = true;
			vel = car.velocity;
			car.velocity = new Vector3 (0, 0, 0);
		}

		//Continue moving when invisible traffic light allows it
		else if (other.gameObject.name == "TrafficRelease" && (timeBeforeTurn == -1) &&
	 			 stopped) {
			car.velocity = vel;
			timeBeforeTurn = 2.5;
			turnDirection = "right";// <= This is where directions should be delivered
			stopped = false;
		}
	}

	//continue movement when car leaves shield space
	void OnTriggerExit(Collider other){
		if (other.gameObject.name == "Shield" && stopped) {
			timeBeforeContinue = 2;
		}
	}

}
