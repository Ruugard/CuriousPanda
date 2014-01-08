using UnityEngine;
using System.Collections;

public class EngineRoom : BaseControlRoom 
{
	public override void Awake()
	{
		base.Awake();

		RoomName = "Engine Room";
	}
}
