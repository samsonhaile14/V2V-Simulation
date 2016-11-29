using UnityEngine;
using System.Collections;

public class SwitchTraffic : MonoBehaviour {

	public float timeToSwitch;
	private float accumTime = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		accumTime += Time.deltaTime;
		if (accumTime > timeToSwitch) {
			transform.Rotate (new Vector3 (0, 90, 0));
			accumTime = 0;
		}
	}
}
