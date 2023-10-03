using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// much of this script was thanks to the ideas of Dave from YT: https://www.youtube.com/@davegamedevelopment
// I still claim credit for the execution tho

// left to do:
// probs put the gun as a ui element
// power features
// detecting enemy type not working

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
        wallskid,
        airborn,
        dash,
        freeze,
        swing
    }

    public Mesh shrek1, shrek2;
    public LayerMask wallType;
    [Header("Movement")]
    public GameObject player; // game object of player
    Rigidbody playerRB; // rigid body of player
    CapsuleCollider playerCollider; // collider of player; should never be anything but capsule collider
    public PlayerState state = PlayerState.ground; // what action state the player is in
    public float playerSpeed = 10; // current max speed, changes by state
    public CameraControl cameraControl; // camera control script in player
    float horzInput, vertInput; // player inputs
    public Camera mainCam;

    [Header("Grounding")]
    public float playerHeight = 1; // height of playerRB
    public LayerMask layerIdentGround; // what is ground
    public bool isGrounded = true; // is player touching ground
    public float groundCheckTolerance = 0.02f; // extra height to check below player for ground
    public float groundDrag = 5f; // amount of drag to apply when player is grounded
    public Vector3 flatV; // player velocity disregarding fall/rise
    public float groundSpeed = 10; // max player speed while on the ground

    [Header("Air Control")]
    [Range(0f, 1f)] public float airNerf = 0.7f; // the amount to multiply player speed by when in the air
    public bool jumpReady = true; // true when jumpCooldown has ended
    public float jumpCooldown = 0.25f; // amount of time between jumps in seconds
    public float jumpForce = 5; // how much force to apply when jumping
    public float jumpHoldFloat = 3; // amount of counter force to add when holding the jump key
    public float airDrag = 0; // how much drag to have while airborn
    public float airSpeed = 15; // max player speed while airborn

    [Header("Wallrun")]
    public float wallrunMinV = 5f; // minimum speed to start wallrunning
    public float wallrunCheckRadius = 1; // length of ray to check for a wall
    public Trilean wallIsWhere; 
    public float wallrunForce = 9.8f; // how much lateral force to apply so the player sticks to the wall  
    public LayerMask layerIdentWall; // what is wall
    public float wallrunDrag = 2.5f; // how much drag to apply whilst wallruning
    public float wallSpeed = 10; // max player movement speed while wallrunning
    public float skidDrag = 3.5f;
    public LayerMask layerIdentWallSkid;

    [Header("Dash Control")]
    public float dashForce = 5; // how much force to apply when dashing
    public float dashDrag = 4; // how much drag to apply while in dash state
    public bool dashReady = true; // if dash is ready
    public float dashCooldown = 0.5f; // cooldown between dashes

    [Header("Power Meter")]
    public int maxPower = 50; // max power a player can have
    public int currentPower; // current amount of power
    public int powerToDash; // how much to dash
    public int powerToJump; // how much to jump
    public int powerToWallrun; // how much to wallrun
    public int powerToShoot; // how much to shoot

    [Header("Death Handling")]
    public GameObject deathUI; // screen to show on death
    public GameObject normalHUD; // the HUD normally shown during gameplay

    [Header("Grapple")]
    public float hookRange; // how far the hook can shoot
    public LayerMask layerIdentHookable; // layer mask for hookable objects
    Vector3 hookPoint; // location of the hook
    SpringJoint hook; // the hook component
    public float maxSwingDist = 0.8f; // multiplier to how long the rope can be
    public float minSwingDist = 0.25f; // multiplier to how long the rope can be
    public float hookVarSpring = 4.5f; // how springy the rope is
    public float hookVarDamp = 7f; // how much to dampen the springiness
    public float hookVarMassScale = 4.5f; // some physics variable idk
    public KeyCode swingKey; // button to swing
    public LineRenderer hookRope; // the line render component for the rope
    public Transform hookSpawnPoint; // the location the rope shoots from
    Vector3 currentHookPos; // where the hook is displayed as being, not where the hook attach point is
    public float ropeSpeed = 8; // how fast the rope shoots
    public float swingSpeed = 15; // how fast you move when swinging
    public float swingDrag = 0; // how much drag to apply when swinging

    [Header("Sho- I Mean Dispatcher")]
    public GameObject guIMeanDispatchImplement;
    public Transform dispatcherSpawnPoint;
    public LayerMask layerIdentEnemy;
    public float gunRange = 25;
    public SpawnHandler spawnHandle;
    public ScoreCounter scoreCount;
    public int easyPoints = 15, midPoints = 30;

    [Header("Sounds")]
    public AudioClip[] shootSounds;
    public AudioClip[] stepSounds;
    public AudioClip[] jumpSounds;
    public AudioClip[] landSounds;
    public AudioClip[] dashSounds;
    public AudioClip[] hookshotSounds;
    public AudioClip normalMusic, deathMusic;
    public AudioSource musicSource;
    public AudioSource soundSource;

    private void Start()
    {
        // just a bunch of variable decls
        playerRB = player.GetComponent<Rigidbody>();
        playerCollider = player.GetComponent<CapsuleCollider>();
        // make sure player starts level at max power
        currentPower = maxPower;
        // start music playback
        musicSource.clip = normalMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    bool currentGround, lastGround;
    private void Update() 
    {
        // input get in here so input is received regardless of framerate
        horzInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(swingKey))
            ShootHook(); // if swing key pressed, attempt grapple
        if (Input.GetKeyUp(swingKey))
            DetachHook(); // if swing key released, stop grapple
        DrawRope();// draw rope
    }

    private void FixedUpdate()
    {
        RaycastHit groundCheck;
        isGrounded = Physics.SphereCast(player.transform.position, playerCollider.radius / 2, Vector3.down, out groundCheck, playerHeight / 2 + groundCheckTolerance, layerIdentGround);  // casts ray at player's feet of distance playerheight/2+tolerance to search for gameobject in layer "layerIdentGround"
        currentGround = isGrounded;
        Debug.DrawRay(player.transform.position, Vector3.down * (playerHeight / 2 + groundCheckTolerance), Color.white, 0.0f, false);
        if (isGrounded)
        {
            state = PlayerState.ground; // if player is grounded, set grounded state
        }
        else if (!isGrounded && state == PlayerState.ground)
        {
            state = PlayerState.airborn; // dirty fix for this issue: "state doesnt change from grounded if you walk off a platform instead of jump"
        }
        SpeedLimit(); // apply speed limit on ground
        HandleDrag(); // apply drag based on state
        if(state != PlayerState.freeze) // if player is not frozen
        {
            if (state == PlayerState.ground)
            {
                playerRB.useGravity = true; // if player grounded then use gravity
            }
            else
            {
                if (CheckForWallrun() == false) // if no wall or not wall running
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

        if (currentGround == true && lastGround == false)
        {
            soundSource.PlayOneShot(landSounds[Mathf.RoundToInt(Random.Range(0, landSounds.Length - 1))]); // play random land sound
        }

        if (Input.GetKey(KeyCode.LeftControl) && dashReady == true && currentPower > 0) // change to current power > power to dash
        {
            Dash();
        }
        if (Input.GetMouseButtonDown(0))
        {
            FireDispatcher();
        }
        lastGround = currentGround;
    }

    public void DeathFunc() // if player dies
    {
        // show and unlock cursor
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
        // freeze player and camera controls
        state = PlayerState.freeze;
        // activate death screen
        deathUI.SetActive(true);
        // hides normal hud
        normalHUD.SetActive(false);
        // switch to death music(lol)
        musicSource.clip = deathMusic;
        musicSource.Play();
        musicSource.time = 5.5f;
    }

    private void MovePlayer()
    {
        Vector3 moveDirection = cameraControl.playerOrientation.forward * vertInput + cameraControl.playerOrientation.right * horzInput; // sets movement direction based on camera facing and input
        if (state == PlayerState.ground || state == PlayerState.wallrun) // only allow grounded movement if wallrunning or grounded
        {
            playerRB.AddForce(moveDirection.normalized * playerSpeed * 10, ForceMode.Force); // adds force using moveDirection and player speed
            if (Input.GetKey(KeyCode.Space) && jumpReady) // if jump is pressed and no jump cooldown active
            {
                jumpReady = false; // disable immediate jumping on same frame
                Jump(); // jump
                Invoke(nameof(ResetJump), jumpCooldown); // start jump cooldown timer
            }
        }
        else if (state == PlayerState.airborn) // if airborn, apply air movement
        {
            if (Input.GetKey(KeyCode.Space)) // and holding space
            {
                playerRB.AddForce(transform.up * jumpHoldFloat); // add extra air time
            }
            playerRB.AddForce(moveDirection.normalized * playerSpeed * airNerf, ForceMode.Force); // adds force using moveDirection and player speed and reduced by multiplying by airnerf
        }
        return;
    }

    private void HandleDrag() // if player is on the ground, apply drag, else no drag
    {
        if (state == PlayerState.ground)
        {
            playerRB.drag = groundDrag; // if grounded, apply ground drag
            playerSpeed = groundSpeed;
            return;
        }
        else if ( state == PlayerState.wallrun)
        {
            playerRB.drag = wallrunDrag; // if wallrunning, apply wallrun drag
            playerSpeed = wallSpeed;
        }
        else if (state == PlayerState.dash)
        {
            playerRB.drag = dashDrag; // if dashing, apply dashing drag
        } 
        else if (state == PlayerState.airborn)
        {
            playerRB.drag = airDrag; // if airborn, apply air drag
            playerSpeed = airSpeed;
        } 
        else if (state == PlayerState.freeze)
        {
            playerRB.drag = 100f; // if frozen, apply hella drag in case something breaks
            playerSpeed = 0f;
        }
        else if (state == PlayerState.swing)
        {
            playerRB.drag = swingDrag; // if swinging, apply swing drag
            playerSpeed = swingSpeed;
        }
        else if (state == PlayerState.wallskid)
        {
            playerRB.drag = skidDrag; // if wallrunning on not-glass, apply ground drag
            playerSpeed = wallSpeed;
        }
        else
        {
            playerRB.drag = 0; // if state not found, throw error
            playerSpeed = 0f;
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
        state = PlayerState.airborn; // set airborn state on jump
        isGrounded = false; // i don't know why this is here but i'm scared to remove it
        playerRB.velocity = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z); // set player fall speed to 0
        playerRB.AddForce(transform.up * jumpForce, ForceMode.Impulse); // apply jumpforce to playerrb upwards
        soundSource.PlayOneShot(jumpSounds[Mathf.RoundToInt(Random.Range(0, jumpSounds.Length - 1))]); // play random jump sound
        return;
    }

    private void ResetJump() // used to apply timer before allowing jump
    {
        jumpReady = true;
        return;
    }

    private bool CheckForWallrun() // check for wall, if wall, set wallIsWhere and call Wallrun()
    {
        flatV = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z); // player velocity excluding fall/rise

        if (flatV.magnitude >= wallrunMinV) // if player horizontal velocity is fast enough to wallrun
        {
            RaycastHit wallData;
            if (Physics.Raycast(player.transform.position, transform.right, out wallData, wallrunCheckRadius)) // if wall is found on the right
            {
            //    Debug.Log("wall is right");
                wallIsWhere = Trilean.Right; // used in Wallrun()
                wallType = wallData.transform.gameObject.layer;
                Wallrun();
                return true;
            }
            else if (Physics.Raycast(player.transform.position, -transform.right, out wallData, wallrunCheckRadius)){ // if wall is found on the left
            //    Debug.Log("wall is left");
                wallIsWhere = Trilean.Left; // used in Wallrun()
                wallType = wallData.transform.gameObject.layer;
                Wallrun();
                return true;
            }
            else // if no wall found
            {
            //    Debug.Log("no wall");
                if (state == PlayerState.wallrun) // if player was wallrunning up until this point
                {
                    EndWallrun(); // stop it
                }
                wallIsWhere = Trilean.None; // not actually sure if this is necessary
            }
        }
        if (state == PlayerState.wallrun)
            EndWallrun(); // if player is not fast enough and was wallrunning, call EndWallrun();
        return false;
    }

    private void Wallrun() // apply wallrun forces in the Trilean direction relative to the player
    {
        if((layerIdentWall & 1 << wallType) == 1 << wallType) // if wall is glass wall
        {
            state = PlayerState.wallrun; // set state to wallrun
            //Debug.Log("wall");
        }
        else if((layerIdentWallSkid & 1 << wallType) == 1 << wallType) // if something else
        {
            state = PlayerState.wallskid; // set state to wallskid 
            //Debug.Log("nonwall");
        }
        else
        {
            Debug.LogError("LayerMask Error : Wall of Layer \'" + wallType + "\' when expecting type \'Wall\' or \'Ground\'");
            return;
        }
        playerRB.useGravity = false; // disable gravity on player
        if (wallIsWhere == Trilean.Right) // if wall is right
        {
            playerRB.AddForce(player.transform.right * wallrunForce * Time.deltaTime);
        }
        else if (wallIsWhere == Trilean.Left) // if wall is left
        {
            playerRB.AddForce(-player.transform.right * wallrunForce * Time.deltaTime);
        }
        else // don't think it's possible to get here
        {
            Debug.LogError("Unknown Error: Entered Wallrun() in Trilean.None state"); 
            EndWallrun();
        }
    }

    private void EndWallrun() // probably not necessary as a separate function
    {
        playerRB.useGravity = true;
        return;
    }

    private void Dash() // apply dash force forward relative to player
    {
        state = PlayerState.dash;
        float _t;
        if (playerRB.velocity.y > 0) // if player is going up, keep upward velocity
        {
             _t = playerRB.velocity.y;
        }
        else // if player is falling, cancel fall velocity
        {
             _t = 0;
        }
        playerRB.drag = 0;
        playerRB.velocity = new Vector3(playerRB.velocity.x, _t, playerRB.velocity.z); // set player fall speed to _t
        playerRB.AddForce(transform.forward * dashForce, ForceMode.Impulse); // apply dashforce to playerrb forwards
        soundSource.PlayOneShot(dashSounds[Mathf.RoundToInt(Random.Range(0, dashSounds.Length - 1))]); // play random dash sound

        dashReady = false; // disable immediate jumping on same frame
        InvokeRepeating(nameof(ResetDash), dashCooldown, 1); // after dashcooldown invoke repeating until reset dash is successful
        return;
    }

    private void ResetDash() // used to apply cooldown 
    {
        if (dashReady == false) // keep looping until state changes 
        {
            if (state != PlayerState.airborn) // if player touches something they can jump off of; need to add swinging state
            {
                dashReady = true;
                CancelInvoke(nameof(ResetDash)); // stop InvokeRepeating from Dash()
            }
        }
    }

    private void Freeze() // freeze all player actions
    {
        playerRB.useGravity = false; // disable gravity
        playerRB.velocity = Vector3.zero; // cancel all velocity
        cameraControl.canLook = false; // disables camera movement
        playerRB.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void Thaw() // return player agency
    {
        playerRB.useGravity = true; // reenable gravity
        cameraControl.canLook = true; // reenable camera movement
    }

    private void ShootHook() // shoot the grappling hook
    {
        RaycastHit hookHit; // hit info from raycast in next line
        if(Physics.Raycast(mainCam.transform.position, cameraControl.cameraOrientation.forward, out hookHit, hookRange))
        { // if raycast hits a target within hook range with tag layerIdentHook based on raycast from camera position pointing to camera forward
            hookPoint = hookHit.point; // the Vector3 that the raycast hit
            hook = player.AddComponent<SpringJoint>(); // add joint to player
            hook.autoConfigureConnectedAnchor = false; // i dont want your default settings unity
            hook.connectedAnchor = hookPoint; // where the springjoint is attached
            float distToHook = Vector3.Distance(player.transform.position, hookPoint); // distance to the attach point
            hook.maxDistance = maxSwingDist * distToHook; // don't fully understand this one tbh
            hook.minDistance = minSwingDist * distToHook;
            hook.spring = hookVarSpring; // how springy the rope is
            hook.damper = hookVarDamp; // how much movement dampening in springiness
            hook.massScale = hookVarMassScale; // some physics bs idk
            soundSource.PlayOneShot(hookshotSounds[Mathf.RoundToInt(Random.Range(0, hookshotSounds.Length - 1))]); // play random hookshot sound

            hookRope.positionCount = 2; // two points in the line renderer for the rope
            currentHookPos = hookSpawnPoint.position; // set the hook position to the attach point
        }
    }

    private void DetachHook() // stop swing/grapple
    {
        hookRope.positionCount = 0; // remove line points for line renderer
        Destroy(hook); // delete the spring joint component from player
    }

    private void DrawRope() // draw the rope for swinging/grappling
    {
        if (!hook) return; // if there is no spring joint cancel this function
        currentHookPos = Vector3.Lerp(currentHookPos, hookPoint, Time.deltaTime * ropeSpeed); // math based animation for hook moving from gun to anchor
        hookRope.SetPosition(0, hookSpawnPoint.position); // set pos 0 of line renderer to the gun
        hookRope.SetPosition(1, currentHookPos); // set pos 1 of the line renderer to the hook position
    }

    private void FireDispatcher() // fire the gun
    {
        soundSource.PlayOneShot(shootSounds[Mathf.RoundToInt(Random.Range(0, shootSounds.Length - 1))]); // play random dash sound
        RaycastHit gunHit;
        if(Physics.Raycast(mainCam.transform.position, cameraControl.cameraOrientation.forward, out gunHit, gunRange, layerIdentEnemy))
        {
            Debug.Log("hit!");
            if(gunHit.transform.gameObject.CompareTag("Easy"))
            {
                scoreCount.AddScore(easyPoints);
            } 
            else if (gunHit.transform.gameObject.CompareTag("Mid"))
            {
                scoreCount.AddScore(midPoints);
            }
            else
            {
                scoreCount.AddScore(1000);
                Debug.LogWarning("Layer Error : Destroyed enemy of unknown type; Have 1k");
            }

            spawnHandle.enemies.Remove(gunHit.transform.gameObject);
            Destroy(gunHit.transform.gameObject.transform.parent.gameObject);
        }
    }

    private void OnDrawGizmos() // this ugly ass function is just to add that cursed shrek model in the editor window
    {
        Gizmos.DrawMesh(shrek1, -1, new Vector3(player.transform.position.x, player.transform.position.y - 1, player.transform.position.z), player.transform.rotation, new Vector3(1.75f, 1.75f, 1.75f));
        Gizmos.DrawMesh(shrek2, -1, new Vector3(player.transform.position.x, player.transform.position.y - 1, player.transform.position.z), player.transform.rotation, new Vector3(1.75f, 1.75f, 1.75f));
        //Gizmos.DrawWireSphere(new Vector3(player.transform.position.x, player.transform.position.y - (playerHeight / 2) + groundCheckTolerance, player.transform.position.z), playerCollider.radius / 2);
    }

}

