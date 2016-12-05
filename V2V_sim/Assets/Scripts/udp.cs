using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class udp : MonoBehaviour {

	public class packet{
		public int src = 0;	//simulation uses 2 bytes to represent addresses so there are 65533 addresses, 
							//with address 65535 used for broadcast
		public int dest = 0;

		public int ttl = 0;	//real world time before packet is discarded by node

		public int transId; //combined with src id to designate the packet number
							//method for combining transId and src is transID + src * 100000

		public float xCor,yCor; //coordinates that the packet is attempting to reach.
		public float range;	//acceptable range from the coordinate the car can be

		public string data;
	}

	public Transform car;
	public Rigidbody rigCar;
	public Material[] matList;
	public Renderer meshRend;

	public packet store = new packet(); //packet currently stored
	public packet receive = new packet(); //packet received through broadcast

	public int srcIP;       // IP address specified in unity relative to car

	private int packetSent = 0;
	private float timeCount = 0;
	private Dictionary<int,bool> packRec = new Dictionary<int,bool>(); //keeps track of all packets received

/*
	int destIP = 65535;  // broadcast address
	int srcPort = 80;
	int destPort = 80;
	int speed = 0;
	string direction = "north";
	int congestionLevel = 0;
*/

	// Use this for initialization
	void Start () {
		meshRend.material = matList [0];
	}
	
	// Update is called once per frame
	void Update () {
		if (srcIP == 4) {
			Debug.Log (timeCount);
		}



		//mechanism for timeout
		if (timeCount < store.ttl) {
				timeCount += Time.deltaTime;
				if (timeCount > store.ttl) {
					//erase packet (dest id of 0 ensures packet drop,i.e. node has no packet to broadcast)
						store.src = 0;
						store.dest = 0;
						store.ttl = 0;
						store.transId = 0;
						store.data = "";

					//visually show packet is no longer travling
						Debug.Log ("Disconnected");
						meshRend.material = matList [0];

					//reset timer to zero
						timeCount = 0;
				}
			}
	
	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.name == "Sphere") {					
			//check packet drop criteria

				//dest id is not for broadcast
					if (other.gameObject.GetComponent<udp> ().store.dest != 65535) {
				//Debug.Log (other.gameObject.GetComponent<udp> ().store.dest);
						return; //drop packet
					}

				//transaction id was received before
					if (packRec.ContainsKey (other.gameObject.GetComponent<udp> ().store.transId)) {
				//Debug.Log ("drop2");
						return;	//ignore packet
					}

				//currently not moving towards destination

			//retrieve packet from initating source
				receive = other.gameObject.GetComponent<udp>().store;

			//set received packet in store to initiate broadcast
				store.src = receive.src;
				store.dest = receive.dest;
				store.transId = receive.transId;
				store.ttl = receive.ttl;
				store.data = receive.data;
				store.xCor = receive.xCor;
				store.yCor = receive.yCor;
				store.range = receive.range;

				packRec.Add (store.transId, true); //mark down packet as received
				timeCount = 0;

			//indicate visually that current node is now carrying packet
				Debug.Log ("Connected");
				meshRend.material = matList [1];

		}

		else if (other.gameObject.name == "StartPacket") {
			//mechanism for showing packet is traveling
				Debug.Log ("Connected");
				meshRend.material = matList [1];

			//retrieve packet from initating source
				packet val = other.gameObject.GetComponent<InitiatePacket>().store;

			//make packet own by initializing values
				store.src = srcIP;
				store.dest = 65535;
				store.transId = srcIP * 100000 + packetSent;
				store.ttl = val.ttl;
				store.data = val.data;
				store.xCor = val.xCor;
				store.yCor = val.yCor;
				store.range = val.range;

				packetSent++;
				packRec.Add (store.transId, true); //mark down packet as received
				timeCount = 0;
		}
	}

}
