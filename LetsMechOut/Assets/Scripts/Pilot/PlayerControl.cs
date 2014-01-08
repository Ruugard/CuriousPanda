using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class PlayerControl : Photon.MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the player should jump.


	public float moveForce = 5f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 1f;				// The fastest the player can travel in the x axis.
	public float jumpForce = 50;			// Amount of force added when the player jumps.

	public GameObject ObjectWeCanFallThrough
	{
		get;
		set;
	}


	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private Transform wallClimbCheck;
	private Transform mRoofClimbCheck;
	private bool grounded = false;			// Whether or not the player is grounded.
	private Animator anim;					// Reference to the player's animator component.
	private bool mIsClimbing = false;
	private bool mIsRoofClimbing = false;
	private bool mCanDoubleJump = true;

	// contains player info like the users name, skills, and stats
	private PilotInfo mPilotInfo;

	// current room occupied data
	public BaseControlRoom CurrentRoom
	{
		get;
		set;
	}

	// networking
	private Vector3 mLatestCorrectPos;
	private Vector3 mOnUpdatePos;
	private float mFraction;

	public bool IsLocalPlayer
	{
		get;
		set;
	}

	
	/// <summary>
	/// While script is observed (in a PhotonView), this is called by PUN with a stream to write or read.
	/// </summary>
	/// <remarks>
	/// The property stream.isWriting is true for the owner of a PhotonView. This is the only client that
	/// should write into the stream. Others will receive the content written by the owner and can read it.
	/// 
	/// Note: Send only what you actually want to consume/use, too!
	/// Note: If the owner doesn't write something into the stream, PUN won't send anything.
	/// </remarks>
	/// <param name="stream">Read or write stream to pass state of this GameObject (or whatever else).</param>
	/// <param name="info">Some info about the sender of this stream, who is the owner of this PhotonView (and GameObject).</param>
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 pos = transform.localPosition;
			Quaternion rot = transform.localRotation;
			float speed = anim.GetFloat("Speed");
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			stream.Serialize(ref speed);
			stream.Serialize(ref facingRight);
		}
		else
		{
			// Receive latest state information
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			float speed = 0;

			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			stream.Serialize(ref speed);
			stream.Serialize(ref facingRight);
			
			mLatestCorrectPos = pos;                 // save this to move towards it in FixedUpdate()
			mOnUpdatePos = transform.localPosition;  // we interpolate from here to latestCorrectPos
			mFraction = 0;                           // reset the fraction we alreay moved. see Update()
			
			transform.localRotation = rot;          // this sample doesn't smooth rotation

			anim.SetFloat("Speed", speed);
			SetFacingDirection(facingRight);
		}
	}
	
	void Awake()
	{
		CurrentRoom = null;

		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		wallClimbCheck = transform.Find("climbWallCheck");
		mRoofClimbCheck = transform.Find("climbRoofCheck");
		anim = GetComponent<Animator>();

		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Climbable"), LayerMask.NameToLayer("Player"));
	
		mPilotInfo = new PilotInfo();
		mPilotInfo.PlayerName = "";

		mLatestCorrectPos = transform.position;
		mOnUpdatePos = transform.position;
	}


	void Update()
	{
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")); 

		if(grounded)
		{
			mCanDoubleJump = true;
		}

		anim.SetBool("Jump", !grounded);

		if (photonView.isMine)
		{
			if(Input.GetButtonDown("Use"))
			{
				// check for elevators, if we find one then teleport to it
				// todo, add in an animation for elevators moving
				Collider2D [] collisions = Physics2D.OverlapPointAll(new Vector2(this.transform.position.x, this.transform.position.y));
				foreach(Collider2D col in collisions)
				{
					if(col.gameObject.tag == "Elevator")
					{
						Elevator elevator = (Elevator)col.gameObject.GetComponent("Elevator");
						GameObject [] objects = GameObject.FindGameObjectsWithTag("Elevator");
						bool foundDestination = false;
						foreach(GameObject obj in objects)
						{
							Elevator elevatorDest = (Elevator)obj.GetComponent("Elevator");
							if(elevatorDest != null && elevatorDest.elevatorName.Equals(elevator.elevatorDestinationName))
							{
								foundDestination = true;
								this.transform.position = obj.transform.position;

								// TODO, when we have elevators with animations and not just teleporting I suspect this will break since we would still get a 
								// onTriggerExit2D call from the room. In which case, this call could most likely be removed
								if(CurrentRoom != null)
								{
									CurrentRoom.ExitRoom(this);
								}

								break;
							}
						}

						if(foundDestination == false)
						{
							Debug.LogError("Could not find elevator with a name " + elevator.elevatorDestinationName);
						}

						break;
					}
				}
			}

			// If the jump button is pressed and the player is grounded then the player should jump.
			if(Input.GetButtonDown("Jump"))
			{
				bool fallingThrough = false;
				if(Input.GetButtonDown("Down") && ObjectWeCanFallThrough != null)
				{
					fallingThrough = true;
					//Physics2D.IgnoreCollision(collider, ObjectWeCanFallThrough.collider);
				}

				if(fallingThrough == false && (grounded == true || mCanDoubleJump == true))
				{
					if(grounded == false)
					{
						mCanDoubleJump = false;
					}
					jump = true;
				}
			}

			// attempt to grapple to the wall
			if((grounded == false && mIsClimbing == false && mIsRoofClimbing == false) && Input.GetButtonDown("WallGrab"))
			{
				if(Physics2D.Linecast(transform.position, wallClimbCheck.position, 1 << LayerMask.NameToLayer("Climbable")))
				{
					anim.SetBool("Climb", true);
					mIsClimbing = true;
					rigidbody2D.velocity = Vector2.zero;
					rigidbody2D.gravityScale = 0;
				}
				else if (Physics2D.Linecast(transform.position, mRoofClimbCheck.position, 1 << LayerMask.NameToLayer("Climbable")))
				{
					mIsRoofClimbing = true;
					mIsClimbing = false;
					rigidbody2D.velocity = Vector2.zero;
					rigidbody2D.gravityScale = 0;
					anim.SetBool("RoofClimb", true);
					anim.SetBool("Climb", false);
				}
			}
			else if(( mIsClimbing || mIsRoofClimbing) && Input.GetButtonDown("WallGrab"))
			{
				_EnableGravityAndFall();
			}
		}
		else
		{
			// We get 10 updates per sec. sometimes a few less or one or two more, depending on variation of lag.
			// Due to that we want to reach the correct position in a little over 100ms. This way, we usually avoid a stop.
			// Lerp() gets a fraction value between 0 and 1. This is how far we went from A to B.
			//
			// Our fraction variable would reach 1 in 100ms if we multiply deltaTime by 10.
			// We want it to take a bit longer, so we multiply with 9 instead.
			
			mFraction = mFraction + Time.deltaTime * 9;
			transform.position = Vector3.Lerp(mOnUpdatePos, mLatestCorrectPos, mFraction);    // set our pos between A and B
		}
	}

	private void _EnableGravityAndFall()
	{
		anim.SetBool("Climb", false);
		anim.SetBool("RoofClimb", false);
		mIsClimbing = false;
		mIsRoofClimbing = false;
		rigidbody2D.gravityScale = 1;
	}

	private bool _AttemptToWallClimb()
	{
		if (Physics2D.Linecast(transform.position, wallClimbCheck.position, 1 << LayerMask.NameToLayer("Climbable")))
		{
			mIsRoofClimbing = false;
			mIsClimbing = true;
			rigidbody2D.velocity = Vector2.zero;
			rigidbody2D.gravityScale = 0;
			anim.SetBool("Climb", true);
			anim.SetBool("RoofClimb", false);
			return true;
		}

		return false;
	}
	
	void FixedUpdate ()
	{
		if(photonView.isMine)
		{
			// Cache the horizontal input.
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat("Speed", Mathf.Abs(h));

			if(Mathf.Abs(h) < 0.1f)
			{
				rigidbody2D.velocity = new Vector2(0,rigidbody2D.velocity.y);
			}

			if(mIsClimbing || mIsRoofClimbing)
			{
				anim.SetFloat("WallSpeed", Mathf.Abs(v));

				if(Mathf.Abs(h) > 0.5f && mIsClimbing)
				{
	                if (Physics2D.Linecast(transform.position, mRoofClimbCheck.position, 1 << LayerMask.NameToLayer("Climbable")))
	                {
	                    mIsRoofClimbing = true;
	                    mIsClimbing = false;
	                    rigidbody2D.velocity = Vector2.zero;
	                    rigidbody2D.gravityScale = 0;
	                    anim.SetBool("RoofClimb", true);
	                    anim.SetBool("Climb", false);
	                }
				}
	            else if (Mathf.Abs(v) > 0.5f && mIsRoofClimbing)
	            {
					_AttemptToWallClimb();
	            }

	            if (mIsClimbing)
	            {
					if (Physics2D.Linecast(transform.position, wallClimbCheck.position, 1 << LayerMask.NameToLayer("Climbable")) == false)
					{
						_EnableGravityAndFall();
					}
					else if (Mathf.Abs(v) > 0.5f)
	                {
	                    if (v * rigidbody2D.velocity.y < maxSpeed)
	                        // ... add a force to the player.
	                        rigidbody2D.AddForce(Vector2.up * v * moveForce * 0.5f);

	                    // If the player's horizontal velocity is greater than the maxSpeed...
	                    if (Mathf.Abs(rigidbody2D.velocity.y) > maxSpeed)
	                        // ... set the player's velocity to the maxSpeed in the x axis.
	                        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, Mathf.Sign(rigidbody2D.velocity.y * maxSpeed));
	                }
	                else
	                {
	                    rigidbody2D.velocity = Vector2.zero;
	                }
	            }
				else if(mIsRoofClimbing)
				{
					if (Physics2D.Linecast(transform.position, mRoofClimbCheck.position, 1 << LayerMask.NameToLayer("Climbable")) == false)
					{
						if(_AttemptToWallClimb() == false)
						{
							_EnableGravityAndFall();
						}
					}
				}
			}
			else
			{
				anim.SetFloat("WallSpeed", 0);
			}

			if(mIsClimbing == false)
			{
				// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
				if(h * rigidbody2D.velocity.x < maxSpeed)
					// ... add a force to the player.
					rigidbody2D.AddForce(Vector2.right * h * moveForce);

				// If the player's horizontal velocity is greater than the maxSpeed...
				if(Mathf.Abs(rigidbody2D.velocity.x) > maxSpeed)
					// ... set the player's velocity to the maxSpeed in the x axis.
					rigidbody2D.velocity = new Vector2(Mathf.Sign(rigidbody2D.velocity.x) * maxSpeed, rigidbody2D.velocity.y);

				// If the input is moving the player right and the player is facing left...
				if(h > 0 && !facingRight && !mIsClimbing)
					// ... flip the player.
					Flip();
				// Otherwise if the input is moving the player left and the player is facing right...
				else if(h < 0 && facingRight && !mIsClimbing)
					// ... flip the player.
					Flip();
			}

			// If the player should jump...
			if(jump)
			{
				rigidbody2D.velocity = new Vector2(0,0);

				// Add a vertical force to the player.
				rigidbody2D.AddForce(new Vector2(0f, jumpForce));

				// Make sure the player can't jump again until the jump conditions from Update are satisfied.
				jump = false;
			}
		}
	}
	
	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		SetFacingDirection(facingRight);
	}

	private void SetFacingDirection(bool right)
	{
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;

		if(right)
		{
			theScale.x = Mathf.Abs(theScale.x);
		}
		else
		{
			theScale.x = Mathf.Abs(theScale.x) * -1;
		}

		transform.localScale = theScale;
	}

	[RPC]
	public void SetUserInfo(string userName)
	{
		mPilotInfo.PlayerName = userName;
	}

	public void OnGUI()
	{
		GUIStyle format = new GUIStyle();
		format.alignment = TextAnchor.LowerCenter;
		format.normal.textColor = Color.white;

		Vector3 point = Camera.main.WorldToScreenPoint(transform.position);
		float x = point.x - 5;
		float y = Camera.main.pixelHeight - point.y - 75;
		GUI.Label(new Rect(x-400*0.5f, y, 400, 20), mPilotInfo.PlayerName, format);

		// TODO, remove this once we have in the proper UI for it
		if(photonView.isMine && CurrentRoom != null)
		{
			format = new GUIStyle();
			format.normal.textColor = Color.black;
			format.fontSize = 18;
			GUI.Label(new Rect(22, 22, 400, 20), CurrentRoom.RoomName, format);
			format.normal.textColor = Color.white;
			GUI.Label(new Rect(20, 20, 400, 20), CurrentRoom.RoomName, format);
		}
	}
}
