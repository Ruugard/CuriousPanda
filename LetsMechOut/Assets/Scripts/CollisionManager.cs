using UnityEngine; 
using System.Collections; 
using System.Collections.Generic;

public class CollisionManager
{
	private List<Collision2D> mClimbableSurfaceCollisions = new List<Collision2D>();

	public List<Collision2D> ClimbableSurfaceCollisionList
	{
		get {return mClimbableSurfaceCollisions; }
	}

	private static readonly CollisionManager _instance = new CollisionManager();
	public static CollisionManager Instance { get { return _instance; } }
	
	static CollisionManager(){}
	
	private CollisionManager() {}

	public void AddClimbableSurfaceCollision(Collision2D col)
	{
		mClimbableSurfaceCollisions.Add(col);
	}
	
	public void RemoveClimbableSurfaceCollision(Collision2D col)
	{
		mClimbableSurfaceCollisions.Remove(col);
	}
}