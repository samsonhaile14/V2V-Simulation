using UnityEngine;
using System.Collections;

public class moveCar : MonoBehaviour {

	//note: intersections along z at 65, -30, -125, -220, and -315
	//		intersections along x at -95, 0, 95, 190, 285
	//take cross product of z set and x set to get all intersections
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.velocity = new Vector3 (0, 0, 10);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	}
}
