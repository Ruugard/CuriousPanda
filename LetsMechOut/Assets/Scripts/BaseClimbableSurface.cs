using UnityEngine;
using System.Collections;

public class BaseClimbableSurface : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		CollisionManager.Instance.AddClimbableSurfaceCollision(coll);
	}

	void OnCollisionExit2D(Collision2D coll)
	{
   		CollisionManager.Instance.RemoveClimbableSurfaceCollision(coll);
	}
}
