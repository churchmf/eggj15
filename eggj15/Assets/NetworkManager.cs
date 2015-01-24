using UnityEngine;
using System.Collections;
[ExecuteInEditMode]

//http://createathingunity.blogspot.co.uk/
public class NetworkManager : MonoBehaviour
{
		public Transform player;
		public Transform cpuPlayer;
		string registeredName = "somekindofuniquename";
		float refreshRequestLength = 3.0f;
		HostData[] hostData;
		public string chosenGameName = "";
		public NetworkPlayer myPlayer;
		public int DEFENDER_ATTACKER_RATIO = 30;

		public enum PlayerType
		{
				ATTACKER = 0,
				DEFENDER
		}
	
		private void StartServer ()
		{
				Network.InitializeServer (16, Random.Range (2000, 2500), !Network.HavePublicAddress ());
				MasterServer.RegisterHost (registeredName, chosenGameName);
		}
	
		void OnServerInitialized ()
		{ 
				if (Network.isServer) {
						myPlayer = Network.player;
						makePlayer (myPlayer);
						
						for (int i = 0; i < 4; ++i) {
								makeCPUPlayer ();
						}
				}
		}
	
		void OnConnectedToServer ()
		{
				myPlayer = Network.player;
				networkView.RPC ("makePlayer", RPCMode.Server, myPlayer);
		}

		[RPC]
		void makeCPUPlayer ()
		{
				Transform newPlayer = Network.Instantiate (cpuPlayer, transform.position, transform.rotation, 0) as Transform;
		}
	
		[RPC]
		void makePlayer (NetworkPlayer thisPlayer)
		{
				Transform newPlayer = Network.Instantiate (player, transform.position, transform.rotation, 0) as Transform;
				
				if (Network.isServer) {
						var rigidBody = newPlayer.gameObject.AddComponent<Rigidbody> ();
						rigidBody.freezeRotation = true;

				}

				if (thisPlayer != myPlayer) {
						networkView.RPC ("enableCamera", thisPlayer, newPlayer.networkView.viewID);
						//newPlayer.gameObject.AddComponent ("PlayerRemote");
				} else {
						enableCamera (newPlayer.networkView.viewID);
						//newPlayer.gameObject.AddComponent ("PlayerLocal");
				}

				PlayerType neededType = determineNeededRole ();
				networkView.RPC ("setPlayerRole", RPCMode.AllBuffered, (int)neededType, newPlayer.networkView.viewID);
		

		}
	
		[RPC]
		public void setPlayerRole (int neededType, NetworkViewID playerID)
		{
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				foreach (GameObject thisPlayer in players) {
						if (thisPlayer.networkView.viewID == playerID) {
								if ((PlayerType)neededType == PlayerType.ATTACKER) {
										var component = thisPlayer.AddComponent<Attacker> ();
										component.enabled = thisPlayer.GetComponent<Movement> ().haveControl;
								} else {
										var component = thisPlayer.AddComponent<Defender> ();
										component.enabled = thisPlayer.GetComponent<Movement> ().haveControl;
										
								}
								break;
						}
				}
		}
			
		public PlayerType determineNeededRole ()
		{
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");

				int defenderCount = 0;
				foreach (GameObject thisPlayer in players) {
						if (isDefender (thisPlayer)) {
								defenderCount++;
						}
				}

				if (((float)defenderCount / (float)players.Length) * 100 < DEFENDER_ATTACKER_RATIO) {
						return PlayerType.DEFENDER;
				} else {
						return PlayerType.ATTACKER;
						
				}
		}

		public bool isDefender (GameObject go)
		{
				return go.GetComponent<Defender> () != null;
		}

		public bool isAttacker (GameObject go)
		{
				return go.GetComponent<Attacker> () != null;
		}
	
		[RPC]
		void enableCamera (NetworkViewID playerID)
		{
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				foreach (GameObject thisPlayer in players) {
						if (thisPlayer.networkView.viewID == playerID) {
								thisPlayer.GetComponent<Movement> ().haveControl = true;
								Transform myCamera = thisPlayer.transform.Find ("Camera");
								myCamera.camera.enabled = true;
								myCamera.camera.GetComponent<AudioListener> ().enabled = true;
								break;
						}
				}
		}
	
		public IEnumerator RefreshHostList ()
		{
				MasterServer.RequestHostList (registeredName);
				float timeEnd = Time.time + refreshRequestLength;
				while (Time.time < timeEnd) {
						hostData = MasterServer.PollHostList ();
						yield return new WaitForEndOfFrame ();
				}
		}
	
		public void OnGUI ()
		{
				if (Network.isClient || Network.isServer) {
						return;
				}
				if (chosenGameName == "") {
						GUI.Label (new Rect (Screen.width / 2 - Screen.width / 10, Screen.height / 2 - Screen.height / 20, Screen.width / 5, Screen.height / 20), "Game Name");
				}
				chosenGameName = GUI.TextField (new Rect (Screen.width / 2 - Screen.width / 10, Screen.height / 2 - Screen.height / 20, Screen.width / 5, Screen.height / 20), chosenGameName, 25);
				if (GUI.Button (new Rect (Screen.width / 2 - Screen.width / 10, Screen.height / 2, Screen.width / 5, Screen.height / 10), "Start New Server")) {
						StartServer ();
				}
				if (GUI.Button (new Rect (Screen.width / 2 - Screen.width / 10, Screen.height / 2 + Screen.height / 10, Screen.width / 5, Screen.height / 10), "Find Servers")) {
						StartCoroutine (RefreshHostList ());
				}
				if (hostData != null) {
						for (int i = 0; i < hostData.Length; i++) {
								if (GUI.Button (new Rect (Screen.width / 2 - Screen.width / 10, Screen.height / 2 + ((Screen.height / 20) * (i + 4)), Screen.width / 5, Screen.height / 20), hostData [i].gameName)) {
										Network.Connect (hostData [i]);
								}
						}
				}
		}
}
