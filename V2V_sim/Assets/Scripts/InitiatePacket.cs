using UnityEngine;
using System.Collections;

public class InitiatePacket : MonoBehaviour {

	public udp.packet store = new udp.packet();
	private float timeCount = 0;

	public Collider broadcast;

	public float xDest, zDest, destRange;

	// Use this for initialization
	void Start () {
	
		//initialize starting packet
			store.src = 0;
			store.dest = 65535; //all bits set to 1 to indicate broadcast
			store.ttl = 15;	//packet is initiated and lasts for 5 seconds
			//transaction id not specified here
			store.data = "Congestion Level: 500";

			store.xCor = xDest;	//specify the location the packet is intended for
			store.zCor = zDest;
			store.range = destRange;
	}
	
	// Update is called once per frame
	void Update () {

		timeCount += Time.deltaTime;

		if (broadcast.enabled && timeCount > store.ttl) {
			//Disable broadcast
			broadcast.enabled = false;
			timeCount = 0;
		} else if (!broadcast.enabled && timeCount > 15) {	//wait 60 seconds before sending another packet
			broadcast.enabled = true;
			timeCount = 0;
		}

	}

}
