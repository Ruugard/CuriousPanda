using ExitGames.Client.Photon;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviour
{
	public GameObject PlayerPrefab;
	public List<GameObject> players = new List<GameObject>();
	
	#region CONNECTION HANDLING
	
	public void Awake()
	{
		// TODO, remove or move this. This is a hack for when we start the game up mid stage so we don't have to start from the main menu every time
		if(GameObject.FindGameObjectWithTag("GlobalManagers") == null)
		{
			Instantiate(Resources.Load(@"CodeObjects/Managers"), Vector3.zero, Quaternion.identity);
		}

		UserInfo userInfo = (UserInfo)GameObject.FindGameObjectWithTag("GlobalManagers").GetComponent("UserInfo");

		if(userInfo == null || userInfo.IsOnline == false)
		{
			Debug.Log("Started an offline game");

			PhotonNetwork.offlineMode = true;
			PhotonNetwork.JoinRoom("offline");
		}
		else if (!PhotonNetwork.connected)
		{
			PhotonNetwork.autoJoinLobby = false;
			PhotonNetwork.ConnectUsingSettings("1");
		}
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	public void OnConnectedToMaster()
	{
		PhotonNetwork.JoinRandomRoom();
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	public void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.CreateRoom(null, true, true, 4);
	}

	public void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		Debug.Log("OnPhotonPlayerConnected: " + player);
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	public void OnJoinedRoom()
	{
		Debug.Log("Joined server");
		GameObject pc = (GameObject)PhotonNetwork.Instantiate("mechPilot", GameObject.FindGameObjectWithTag("SpawnPoint").transform.position, Quaternion.identity, 0);
		pc.transform.parent = GameObject.FindGameObjectWithTag("Mech").transform;
		players.Add(pc);

		GameObject obj = GameObject.FindGameObjectWithTag("MainCamera");
		CameraFollow cam = (CameraFollow)obj.GetComponent("CameraFollow");
		cam.player = players[players.Count-1].transform;

		PlayerControl playerControl = (PlayerControl)pc.GetComponent("PlayerControl");
		playerControl.IsLocalPlayer = true;

		PhotonView photonView = PhotonView.Get(pc);
		photonView.RPC("SetUserInfo", PhotonTargets.AllBuffered, GameObject.FindGameObjectWithTag("GlobalManagers").GetComponent<UserInfo>().PlayerName); 
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	public void OnCreatedRoom()
	{
		Debug.Log ("Created a new server");
	}
	
	#endregion
	
	public void Update()
	{
		if (PhotonNetwork.isMasterClient)
		{
		}
	}
}
