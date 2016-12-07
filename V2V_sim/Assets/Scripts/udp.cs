using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class udp : MonoBehaviour {

	public class packet{
		public int src = 0;	//simulation uses 2 bytes to represent addresses so there are 65533 addresses, 
							//with address 65535 used for broadcast
		public int dest = 0;

		public int ccIP = 0; //used for distance vector mechanism. Setting equal to zero initates a general
							 //broadcast with no consideration for distance vector mechanism result

		public int ttl = 0;	//real world time before packet is discarded by node

		public int transId; //combined with src id to designate the packet number
							//method for combining transId and src is transID + src * 100000

		public float xCor,zCor; //coordinates that the packet is attempting to reach.
		public float range;	//acceptable range from the coordinate the car can be

		public string data;
	}
		
	//External information
		public Transform car;
		public Rigidbody rigCar;
		public Material[] matList;
		public Renderer meshRend;

	//Track of total runtime
		private float totalRuntime;

	//Packets for sending and receiving respectively
		public packet store = new packet(); //packet currently stored
		public packet receive = new packet(); //packet received through broadcast

	//vehicle IP address assigned through unity
		public int srcIP;       // IP address specified in unity relative to car

	//Used for creation of transaction id
		private int packetSent = 0;

	//Used for timeout
		private float timeCount = 0;

	//transaction id log
		private Dictionary<int,bool> packRec = new Dictionary<int,bool>(); //keeps track of all packets received

	//distance vector related variables
		private int nodes = 7;
		private int coordSize = 5;
		private Queue< float > visitedX = new Queue< float >(); //queue of points visited by car
		private Queue< float > visitedZ = new Queue< float >();
		private List<float>[] carsXs = new List<float>[7];
		private List<float>[] carsZs = new List<float>[7];
		private float[] coordUpdate = new float[7];
		private bool[] isConnected = {false,false,false,false,false,false,false}; //states whether node is connected to another
		private float coordCount = 0;
		private Dictionary<int,float> dVGraphWeight = new Dictionary<int,float>();	//keeps track of time nodes are within broadcast range of eachother
		private Dictionary<int,float> dVGraphUpdateT = new Dictionary<int,float>(); //logs changes/updates to dVGraphWeight


/*	
	int destIP = 65535;  // broadcast address
	int srcPort = 80;
	int destPort = 80;
	int speed = 0;
	string direction = "north";
	int congestionLevel = 0;
*/

	void distanceVector(){
		//determine which vehicle gets closest to destination coordinate(Verified)
			int closestCar = -1;
			float closestMagnitude = -1;
			for (int a = 0; a < nodes; a++) {
				for (int b = 0; b < carsXs[a].Count; b++) {
					//compute distance/magnitude
					float distX = carsXs[a][b] - store.xCor;
					float distZ = carsZs[a][b] - store.zCor;
					float distCar = Mathf.Sqrt(distX * distX + distZ * distZ);	
					if (closestCar == -1 || distCar < closestMagnitude) {
						closestCar = a+1;
						closestMagnitude = distCar;
					}
				
				}
			}
			
		//perform breadth first search for paths to previously determined vehicle
			store.ccIP = 0;

			//if vehicle picked itself as closest to destination, then set ccip to self
			if (closestCar == -1 || closestCar == srcIP) {
				store.ccIP = srcIP;
			}

		//otherwise, find path to destination vehicle and select first vehicle on path to destination as ccIP
			else {

				//breadth first search approach
					Queue<int> nodesTraveled = new Queue<int>();	//list of nodes marking traversal paths
					Queue<int> firstSelections = new Queue<int> (); //first node on respective path
					nodesTraveled.Enqueue (srcIP - 1);
					for (int a = 0; a < nodes - 1; a++) {	//level in the breadth first search
						Queue<int> nextLevel = new Queue<int>();
						while (nodesTraveled.Count > 0) {
							int node = nodesTraveled.Dequeue ();
							int firstChoice = -1;

							int c = 0;
							int d = 0;

							if(a != 0){
								firstChoice = firstSelections.Dequeue();
							}

							//check for path from node to closestcar
							if (dVGraphWeight.ContainsKey ((1 << node) | (1 << (closestCar - 1)))) {
								if (dVGraphWeight [(1 << node) | (1 << (closestCar - 1))] > 1) {
									//if so, end search and set ccip to node
									a = c = d = nodes;
									nodesTraveled.Clear();

									if( a == 0){
										store.ccIP = node+1;
									}	
									else{
										store.ccIP = firstChoice+1;
									}	
								}
							}

							List<float> sortWeight = new List<float>(); //holds weights of edges connected to node
							List<int> sortIP = new List<int>(); //holds ip addresses of edges connected to node

							for (c = 0; c < nodes; c++) {
								//search for alternate paths through other cars and add to nextLevel
								if(dVGraphWeight[ (1 << node) | (1 << (c))] > 0 ){
									sortWeight.Add( dVGraphWeight[(1 << node) | (1 << (c))] );
									sortIP.Add( c );
								}
							}

							//sort weights
							parallelSort( sortWeight, sortIP);

							//add to next level of breadth search
							for( d = 0; d < sortWeight.Count; d++ ){
								nextLevel.Enqueue( sortIP[d] );
								if(a == 0){
									firstSelections.Enqueue( sortIP[d] );
								}
								else{
									firstSelections.Enqueue( firstChoice );
								}

							}
						}
						while( nextLevel.Count > 0){
							nodesTraveled.Enqueue(nextLevel.Dequeue());
						}
					}

			}
		Debug.Log( store.ccIP );

		//if next ccip is self or path to destination cannot be found, set ccip to -1 to prevent further travel of packet
		if (store.ccIP == srcIP) {
			store.ccIP = -1;
		}
	}

	void parallelSort( List<float> weights, List<int> iPs ){
		//simple bubble sort
		for( int a = 0; a < weights.Count; a++ ){
			for( int b = a + 1; b < weights.Count; b++ ){
				if( weights[a] < weights[b] ){
					float tempW = weights[a];
					int tempI = iPs[a];
					weights[a] = weights[b];
					iPs[a] = iPs[b];
					weights[b] = tempW;
					iPs[b] = tempI;
				}
			}
				
		}
	}

	// Use this for initialization
	void Start () {
		meshRend.material = matList [0];

		//initialize dictionary times to zero
		for (int a = 0; a < nodes; a++) {
			dVGraphWeight.Add( (1 << (srcIP-1)) | (1 << a), 0);
			dVGraphUpdateT.Add( (1 << (srcIP-1)) | (1 << a), 0);
		}

		//initialize carsXs and carsZs to null
		for (int a = 0; a < nodes; a++) {
			carsXs [a] = new List<float>();
			carsZs [a] = new List<float>();
			coordUpdate[a] = 0;
		}
			
	}
	
	// Update is called once per frame
	void Update () {

		//update runtime
			totalRuntime += Time.deltaTime;

		//distance vector related updates

			//visited coordinate point update
				coordCount += Time.deltaTime;

				if (coordCount > coordSize) {
					
					coordCount = 0; // reset timer

					if (visitedX.Count > 5) {
						visitedX.Dequeue ();
						visitedZ.Dequeue ();
					}

					visitedX.Enqueue (car.localPosition.x);
					visitedZ.Enqueue (car.localPosition.z);

					carsXs[srcIP - 1] = new List<float>( visitedX.ToArray());
					carsZs[srcIP - 1] = new List<float>( visitedZ.ToArray());
					coordUpdate[srcIP - 1] = totalRuntime;

				}

			//time connected to other nodes
				for (int a = 0; a < isConnected.Length; a++) {
					if (isConnected [a]) {
						dVGraphWeight [(1 << (srcIP-1)) | (1 << a)] += Time.deltaTime;
						dVGraphUpdateT [(1 << (srcIP - 1)) | (1 << a)] = totalRuntime;
					}
				}

		//if packet reaches destination, show visually
		if( store.dest == 65535 ){
			float xDist = (car.localPosition.x - store.xCor);
			float zDist = (car.localPosition.z - store.zCor);
			float pos = (xDist * xDist + zDist * zDist);

			if (pos <= store.range * store.range) {
				Debug.Log ("Packet has reached destination");
				meshRend.material = matList [2];
			} else {
				meshRend.material = matList [1];
			}
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
			//initiate tracking of connection time
				isConnected[ other.gameObject.GetComponent<udp>().srcIP - 1 ] = true;

			//retrieve visited coordinates from other car(verified)
				for( int a = 0; a < nodes; a ++ ){
					if( coordUpdate[a] < other.gameObject.GetComponent<udp> ().coordUpdate[a] ){
						carsXs[a] = new List<float> (other.gameObject.GetComponent<udp> ().carsXs[a].ToArray());
						carsZs[a] = new List<float> (other.gameObject.GetComponent<udp> ().carsZs[a].ToArray());
						coordUpdate[a] = other.gameObject.GetComponent<udp> ().coordUpdate[a];
					}
				}

			//update distance vector algorithm info
				foreach (KeyValuePair<int,float> entry in other.gameObject.GetComponent<udp>().dVGraphWeight) {
					int valKey = entry.Key;
					//update if outdated
					if(!dVGraphWeight.ContainsKey(valKey)){
						dVGraphWeight.Add( valKey, other.gameObject.GetComponent<udp> ().dVGraphWeight [valKey]);
						dVGraphUpdateT.Add( valKey, other.gameObject.GetComponent<udp> ().dVGraphUpdateT [valKey]);
					}

					else{
						if (dVGraphUpdateT[valKey] < other.gameObject.GetComponent<udp> ().dVGraphUpdateT[valKey]) {
							dVGraphWeight [valKey] = other.gameObject.GetComponent<udp> ().dVGraphWeight [valKey];
							dVGraphUpdateT [valKey] = other.gameObject.GetComponent<udp> ().dVGraphUpdateT [valKey];
						}
					}
				}
				
			//check packet drop criteria

				//dest id is not for broadcast
				if (other.gameObject.GetComponent<udp> ().store.dest != 65535) {
						return; //drop packet
					}

				//receiving car not intended by distance vector algorithm
				if (other.gameObject.GetComponent<udp> ().store.ccIP != srcIP &&
				     other.gameObject.GetComponent<udp> ().store.ccIP != 0) {
					return; //drop packet
				}

				//transaction id was received before
					if (packRec.ContainsKey (other.gameObject.GetComponent<udp> ().store.transId)) {
						return;	//ignore packet
					}

			//retrieve packet from initating source
				receive = other.gameObject.GetComponent<udp>().store;

			//set received packet in store to initiate broadcast
				store.src = receive.src;
				store.dest = receive.dest;
				store.transId = receive.transId;
				store.ttl = receive.ttl;
				store.data = receive.data;
				store.xCor = receive.xCor;
				store.zCor = receive.zCor;
				store.range = receive.range;

				packRec.Add (store.transId, true); //mark down packet as received
				timeCount = 0;

			//calculate next ccip via distance vector algorithm
				distanceVector();

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
				store.zCor = val.zCor;
				store.range = val.range;

				packetSent++;
				packRec.Add (store.transId, true); //mark down packet as received
				timeCount = 0;

			//calculate next ccip via distance vector algorithm
				distanceVector();

		}
	}

	void OnTriggerExit(Collider other){

		if (other.gameObject.name == "Sphere") {

			//halt connection timer
				isConnected[ other.gameObject.GetComponent<udp>().srcIP - 1] = false;

		}
	}

}