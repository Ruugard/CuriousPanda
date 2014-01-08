using UnityEngine;
using System.Collections;

public class BaseControlRoom : MonoBehaviour 
{
	public string RoomName
	{
		get;
		set;
	}

	public virtual void Awake()
	{
		RoomName = "";
	}

	private void OnTriggerEnter2D(Collider2D other) 
	{
		if(other.tag.Equals("Player"))
		{
			EnterRoom(other.GetComponent<PlayerControl>());
		}
	}

	private void OnTriggerExit2D(Collider2D other) 
	{
		if(other.tag.Equals("Player"))
		{
			BaseControlRoom room = other.gameObject.GetComponent<PlayerControl>().CurrentRoom;
			if(room != null && room.RoomName.Equals(RoomName))
			{
				ExitRoom(other.gameObject.GetComponent<PlayerControl>());
			}
		}
	}

	public void EnterRoom(PlayerControl pc)
	{
		pc.CurrentRoom = this;
	}

	public void ExitRoom(PlayerControl pc)
	{
		pc.CurrentRoom = null;
	}
}
