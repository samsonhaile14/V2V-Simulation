using UnityEngine;
using System.Collections;

public class udp : MonoBehaviour {

	int srcIP = 0;       // static IP addresses 
	int destIP = 65535;  // broadcast address
	int srcPort = 80;
	int destPort = 80;
	int speed = 0;
	string direction = "north";
	int congestionLevel = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		// select route from available paths

		// detect closest car in route

		// broadcast in angle relative to velocity

		//  

	
	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.name == "Sphere") {
			Debug.Log ("Connected");
		}
	}

	void OnTriggerExit(Collider other){
		if (other.gameObject.name == "Sphere") {
			Debug.Log ("Disconnected");
		}
	}
}
