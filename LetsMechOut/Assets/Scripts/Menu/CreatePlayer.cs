using UnityEngine;
using System.Collections;

public class CreatePlayer : MonoBehaviour 
{
	private string mUserName = "";
	private bool mSubmitPressed = false;

	private UserInfo userInfo;

	public void Awake()
	{
		if(GameObject.FindGameObjectWithTag("GlobalManagers") == null)
		{
			Instantiate(Resources.Load(@"CodeObjects/Managers"), Vector3.zero, Quaternion.identity);
		}

		userInfo = GameObject.FindGameObjectWithTag("GlobalManagers").GetComponent<UserInfo>();
	}

	public void OnGUI()
	{
		GUILayout.Space(10);
		GUI.Label(new Rect(10, 10, 200, 20), "Name :");

		string tempName = mUserName;
		tempName = GUI.TextField(new Rect(10, 30, 200, 20), tempName, 25);
		if(mSubmitPressed == false)
		{
			mUserName = tempName;
		}

		if(GUI.Button(new Rect(10, 60, 200, 20), "Join Game"))
		{
			userInfo.PlayerName = mUserName;
			userInfo.IsOnline = true;
			mSubmitPressed = true;
			Application.LoadLevel("Level");
		}

		if(GUI.Button(new Rect(10, 90, 200, 20), "Start Offline"))
		{
			userInfo.PlayerName = mUserName;
			mSubmitPressed = true;
			userInfo.IsOnline = false;
			Application.LoadLevel("Level");
		}
	}
}
