using System;
using UnityEngine;
using System.Collections;

public class UserInfo : MonoBehaviour
{
	public string PlayerName
	{
		get;
		set;
	}

	public bool IsOnline
	{
		get;
		set;
	}
	
	public UserInfo ()
	{
		IsOnline = false;
	}

	public void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}


