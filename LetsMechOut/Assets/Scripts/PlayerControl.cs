using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the player should jump.


	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
	public float jumpForce = 500;			// Amount of force added when the player jumps.


	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private Transform wallClimbCheck;
	private Transform mRoofClimbCheck;
	private bool grounded = false;			// Whether or not the player is grounded.
	private Animator anim;					// Reference to the player's animator component.
	private bool mIsClimbing = false;
	private bool mIsRoofClimbing = false;

	void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		wallClimbCheck = transform.Find("climbWallCheck");
		mRoofClimbCheck = transform.Find("climbRoofCheck");
		anim = GetComponent<Animator>();
	}


	void Update()
	{
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

		anim.SetBool("Jump", !grounded);

		// If the jump button is pressed and the player is grounded then the player should jump.
		if(Input.GetButtonDown("Jump") && grounded)
			jump = true;

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

	private void _EnableGravityAndFall()
	{
		anim.SetBool("Climb", false);
		anim.SetBool("RoofClimb", false);
		mIsClimbing = false;
		mIsRoofClimbing = false;
		rigidbody2D.gravityScale = 1;
	}

	void FixedUpdate ()
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
                if (Physics2D.Linecast(transform.position, wallClimbCheck.position, 1 << LayerMask.NameToLayer("Climbable")))
                {
                    mIsRoofClimbing = false;
                    mIsClimbing = true;
                    rigidbody2D.velocity = Vector2.zero;
                    rigidbody2D.gravityScale = 0;
                    anim.SetBool("Climb", true);
                    anim.SetBool("RoofClimb", false);
                }
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
                        rigidbody2D.AddForce(Vector2.up * v * moveForce);

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
					_EnableGravityAndFall();
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
			// Add a vertical force to the player.
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));

			// Make sure the player can't jump again until the jump conditions from Update are satisfied.
			jump = false;
		}
	}
	
	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
