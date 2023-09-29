using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// left to do
// state doesnt change from grounded if you walk off a platform instead of jump
//  add swinging
// dashing crashes the game

public class PlayerControl : MonoBehaviour
{
    public enum Trilean
    {
        Left,
        Right,
        None
    }
    public enum PlayerState
    {
        ground,
        wallrun,
        airborn,
        dash,
        freeze
    }

    public Mesh shrek1, shrek2;

    [Header("Movement")]
    public GameObject player; // game object of player
    Rigidbody playerRB; // rigid body of player
    CapsuleCollider playerCollider; // collider of player; should never be anything but capsule collider
    public PlayerState state = PlayerState.ground; // what action state the player is in
    public float playerSpeed = 10; // dude really? its what it says on the tin
    public CameraControl cameraControl; // camera control script in player
    float horzInput, vertInput; // player inputs

    [Header("Grounding")]
    public float playerHeight = 1; // height of playerRB
    public LayerMask layerIdentGround; // what is ground
    public bool isGrounded = true; // is player touching ground
    public float groundCheckTolerance = 0.02f; // extra height to check below player for ground
    public float groundDrag = 5f; // amount of drag to apply when player is grounded
    public Vector3 flatV; // player velocity disregarding fall/rise

    [Header("Air Control")]
    [Range(0f, 1f)] public float airNerf = 0.7f; // the amount to multiply player speed by when in the air
    public bool jumpReady = true; // true when jumpCooldown has ended
    public float jumpCooldown = 0.25f; // amount of time between jumps in seconds
    public float jumpForce = 5; // how much force to apply when jumping
    public float jumpHoldFloat = 3; // amount of counter force to add when holding the jump key
    public float airDrag = 0; // how much drag to have while airborn

    [Header("Wallrun")]
    public float wallrunMinV = 5f; // minimum speed to start wallrunning
    public float wallrunCheckRadius = 1; // length of ray to check for a wall
    public Trilean wallIsWhere; 
    public float wallrunForce = 9.8f; // how much lateral force to apply so the player sticks to the wall  
    public LayerMask layerIdentWall; // what is wall
    public float wallrunDrag = 2.5f; // how much drag to apply whilst wallruning

    [Header("Dash Control")]
    public float dashForce = 5; // how much force to apply when dashing
    public float dashDrag = 4; // how much drag to apply while in dash state
    public bool dashReady = true;
    public float dashCooldown = 0.5f;

    [Header("Power Meter")]
    public int maxPower = 50;
    public int currentPower;
    public int powerToDash;
    public int powerToJump;
    public int powerToWallrun;
    public int powerToShoot;

    [Header("Death Handling")]
    public GameObject deathUI;
    public AudioSource musicSource;
    public AudioClip deathClip, normalClip;

    private void Start()
    {
        playerRB = player.GetComponent<Rigidbody>();
        playerCollider = player.GetComponent<CapsuleCollider>();
        currentPower = maxPower;
        musicSource.clip = normalClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void Update() 
    {
        // input get in here so input is received regardless of framerate
        horzInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {       
        isGrounded = Physics.Raycast(player.transform.position, Vector3.down, playerHeight / 2 + groundCheckTolerance, layerIdentGround);  // casts ray at player's feet of distance playerheight/2+tolerance to search for gameobject in layer "layerIdentGround"
        Debug.DrawRay(player.transform.position, Vector3.down * (playerHeight / 2 + groundCheckTolerance), Color.white, 0.0f, false);
        if (isGrounded) state = PlayerState.ground;
        SpeedLimit();
        HandleDrag();
        if(state != PlayerState.freeze)
        {
            if (state == PlayerState.ground)
            {
                playerRB.useGravity = true; // if player grounded then use gravity
            }
            else
            {
                if (CheckForWallrun() == false)
                {
                    state = PlayerState.airborn; // if not grounded and not wallrunning, player airborn and should use gravity ** will need to be updated after finishing wallrun
                    playerRB.useGravity = true;
                }
            }
            MovePlayer(); // in here so no speed variation at different framerates
        }
        else
        {
            Freeze();
        }
    }

    public void DeathFunc()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        state = PlayerState.freeze;
        deathUI.SetActive(true);
        musicSource.clip = deathClip;
        musicSource.Play();
        musicSource.time = 5.5f;
    }

    private void MovePlayer()
    {
        Vector3 moveDirection = cameraControl.playerOrientation.forward * vertInput + cameraControl.playerOrientation.right * horzInput; // sets movement direction based on camera facing and input
        if (state == PlayerState.ground || state == PlayerState.wallrun)
        {
            playerRB.AddForce(moveDirection.normalized * playerSpeed * 10, ForceMode.Force); // adds force using moveDirection and player speed
            if (Input.GetKey(KeyCode.Space) && jumpReady) // if jump is pressed and no jump cooldown active
            {
                jumpReady = false; // disable immediate jumping on same frame
                Jump(); // jump
                Invoke(nameof(ResetJump), jumpCooldown); // start jump cooldown timer
            }
        }
        else if (state != PlayerState.ground && state != PlayerState.wallrun)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                playerRB.AddForce(transform.up * jumpHoldFloat);
            }
            playerRB.AddForce(moveDirection.normalized * playerSpeed * airNerf, ForceMode.Force); // adds force using moveDirection and player speed and reduced by multiplying by airnerf
        }
        if(Input.GetKey(KeyCode.LeftControl) && dashReady == true && currentPower > 0) // change to current power > power to dash
        {
            Dash();
        }


        return;
    }

    private void HandleDrag() // if player is on the ground, apply drag, else no drag
    {
        if (state == PlayerState.ground)
        {
            playerRB.drag = groundDrag;
            return;
        }
        else if ( state == PlayerState.wallrun)
        {
            playerRB.drag = wallrunDrag;
        }
        else if (state == PlayerState.dash)
        {
            playerRB.drag = dashDrag;
        } else if (state == PlayerState.airborn)
        {
            playerRB.drag = airDrag;
            return;
        } 
        else if (state == PlayerState.freeze)
        {
            playerRB.drag = 100f;
        }else
        {
            playerRB.drag = 0;
            Debug.LogError("State Error: State not found :: HandleDrag()");
        }
    }

    private void SpeedLimit() // controls player speed
    {
        flatV = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z); // player velocity excluding fall/rise

        if (flatV.magnitude > playerSpeed) // if playerRB speed is greater than playerSpeed
        {
            Vector3 limitedV = flatV.normalized * playerSpeed; // set velocity to playerspeed
            playerRB.velocity = new Vector3(limitedV.x, playerRB.velocity.y, limitedV.z); // the actual setting of velocity
        }
        return;
    }

    private void Jump() // jump
    {
        state = PlayerState.airborn;
        isGrounded = false;
        playerRB.velocity = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z); // set player fall speed to 0
        playerRB.AddForce(transform.up * jumpForce, ForceMode.Impulse); // apply jumpforce to playerrb upwards
        return;
    }

    private void ResetJump() // used to apply timer before allowing jump
    {
        jumpReady = true;
        return;
    }

    private bool CheckForWallrun()
    {
        flatV = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z); // player velocity excluding fall/rise

        if (flatV.magnitude >= wallrunMinV)
        {
            if (Physics.Raycast(player.transform.position, transform.right, wallrunCheckRadius, layerIdentWall))
            {
                Debug.Log("wall is right");
                wallIsWhere = Trilean.Right;
                Wallrun();
                return true;
            }
            else if (Physics.Raycast(player.transform.position, -transform.right, wallrunCheckRadius, layerIdentWall)){
                Debug.Log("wall is left");
                wallIsWhere = Trilean.Left;
                Wallrun();
                return true;
            }
            else
            {
                Debug.Log("no wall");
                if (state == PlayerState.wallrun)
                {
                    EndWallrun();
                }
                wallIsWhere = Trilean.None;
            }
        }
        EndWallrun();
        return false;
    }

    private void Wallrun()
    {
            state = PlayerState.wallrun;
            playerRB.useGravity = false;
            if (wallIsWhere == Trilean.Right)
            {
                playerRB.AddForce(player.transform.right * wallrunForce * Time.deltaTime);
            }
            else if (wallIsWhere == Trilean.Left)
            {
                playerRB.AddForce(-player.transform.right * wallrunForce * Time.deltaTime);
            }
            else
            {
                EndWallrun();
            }
    }

    private void EndWallrun()
    {
        playerRB.useGravity = true;
        return;
    }

    private void Dash()
    {
        state = PlayerState.dash;
        float _t;
        if (playerRB.velocity.y > 0)
        {
             _t = playerRB.velocity.y;
        }
        else
        {
             _t = 0;
        }
        playerRB.drag = 0;
        //float _t = playerRB.velocity.y > 0 ? playerRB.velocity.y : 0; // keeps upwards velocity but not downward velocity
        playerRB.velocity = new Vector3(playerRB.velocity.x, _t, playerRB.velocity.z); // set player fall speed to 0
        playerRB.AddForce(transform.forward * dashForce, ForceMode.Impulse); // apply dashforce to playerrb forwards
        
        dashReady = false; // disable immediate jumping on same frame
        InvokeRepeating(nameof(ResetDash), dashCooldown, 1); // start jump cooldown timer
        return;
    }

    private void ResetDash()
    {
        if (dashReady == false) // keep looping until state changes 
        {
            if (state != PlayerState.airborn) // if player touches something they can jump off of; need to add swinging state
            {
                dashReady = true;
            }
        }
    }

    private void Freeze()
    {
        playerRB.useGravity = false;
        cameraControl.canLook = false; // disables camera movement
    }

    private void Thaw()
    {
        playerRB.useGravity = true;
        cameraControl.canLook = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawMesh(shrek1, -1, new Vector3(player.transform.position.x, player.transform.position.y - 1, player.transform.position.z), player.transform.rotation, new Vector3(1.75f, 1.75f, 1.75f));
        Gizmos.DrawMesh(shrek2, -1, new Vector3(player.transform.position.x, player.transform.position.y - 1, player.transform.position.z), player.transform.rotation, new Vector3(1.75f, 1.75f, 1.75f));
    }

}

