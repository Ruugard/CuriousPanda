using UnityEngine;
using System.Collections;

public class FallthroughableObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.tag == "Player")
		{
			PlayerControl pc = (PlayerControl) coll.gameObject.GetComponent("PlayerControl");
			pc.ObjectWeCanFallThrough = this.gameObject;
		}
	}
	
	void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject.tag == "Player")
		{
			PlayerControl pc = (PlayerControl) coll.gameObject.GetComponent("PlayerControl");
			pc.ObjectWeCanFallThrough = null;
			//Physics2D.IgnoreCollision(coll, this.collider2D, false);
		}
	}
}
