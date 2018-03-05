using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MovementMod
{
    public MovementMod(Vector3 direction, float length, bool fade, bool groundClear, bool gravReset)
    {
        modDirection = currentVector = direction;
        modLength = length;
        modFadesOut = fade;
        resetGravityWhileActive = gravReset;
        removeWhenGrounded = groundClear;
    }

    public Vector3 modDirection;
    public Vector3 currentVector;
    public float modLength;
    public bool modFadesOut;
    public bool removeWhenGrounded;
    public bool resetGravityWhileActive;
    public float modTimer = 0;
}

public class ControlPC : MonoBehaviour
{
    // Animation
    public Animator camAnim;
    [SerializeField]
    private Animator gunAnim;

    // Basic Movement
    [HideInInspector]
    public CharacterController cc;
    private Vector3 moveDirection;
    [Header("Basic Movement")]
    public float baseSpeed;
    public float sprintMultiplier = 1;
    public float airBaseSpeed;
    public bool isGrounded;
    private bool isFalling;
    public LayerMask walkableLayers;
    [HideInInspector]
    public List<MovementMod> movementModifiers = new List<MovementMod>();

    // Jumping
    [Header("Jumping")]
    public float jumpTimeLength = 1;
    public float jumpHeight = 2;
    private bool isJumping;
    private float jumpTimer = 0;

    // Movement abilities
    [HideInInspector]
    public bool hasDoubleJumped;

    // Rigidbody & Physics
    private bool wasStopped;
    public float gravity = 1;
    [HideInInspector]
    public float appliedGravity;

    // Camera
    [Header("Camera")]
    public Transform cameraContainer;
    [HideInInspector]
    public Camera cam;
    public float yRotationSpeed = 45;
    public float xRotationSpeed = 45;
    private float yRotation;
    private float xRotation;

    // UI
    [Header("UI")]
    public UIManager uiManager;

    // Stats
    public int health = 100;

    ///
    ////////// Weapons 
    [Header("Weapons")]
    [SerializeField]
    private Transform barrel;
    [SerializeField]
    private LayerMask weaponLayermask;
    [SerializeField]
    private float timeToSwitchWeapons;
    private float fireTime;
    private float fireDuration;
    private bool isFiring;
    private bool isSwitching;
    private bool isReloading;

    public enum WeaponType
    {
        AssaultRifle,
        Sniper,
        Launcher
    }
    public WeaponType currentWeapon;

    // Assault Rifle
    [Space(10)]
    [Header("Assault Rifle")]
    [SerializeField]
    private int weaponDamageAR;
    [SerializeField]
    private float fireRateAR;
    [SerializeField]
    private float spreadDegreeAR;
    [SerializeField]
    private float timeToMaxSpreadAR = 1;
    private float spreadMultiplierAR;
    [SerializeField]
    private SmokeTrail smokeTrailPrefabAR;
    [SerializeField]
    private ParticleSystem onHitParticleAR;

    // Sniper
    [Space(10)]
    [Header("Sniper Rifle")]
    [SerializeField]
    private int weaponDamageSR;
    [SerializeField]
    private float fireRateSR;

    // Grenade Launcher
    [Space(10)]
    [Header("Grenade Launcher")]
    [SerializeField]
    private int weaponDamageGL;
    [SerializeField]
    private float fireRateGL;
    [SerializeField]
    private GrenadeLauncherAmmo ammoPrefabGL;
    private GrenadeLauncherAmmo ammoGL;
    [SerializeField]
    private float knockbackForceGL = 5;
    [SerializeField]
    private float fireForceGL;

    //////////
    ///

    // Eye Tracking
    public bool isEyeTracking;

    void Start()
    {
        Application.runInBackground = true;
        cc = GetComponent<CharacterController>();
        cameraContainer.GetChild(0).gameObject.SetActive(true);
        cam = cameraContainer.GetComponentInChildren<Camera>();
        health = 100;
        yRotation = transform.localEulerAngles.y;
        xRotation = cam.transform.localEulerAngles.x;
        wasStopped = true;
        appliedGravity = gravity / 2;
        gunAnim.speed = .5f;
        GameManager.gm.pc = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        GetPlayerInput();
        MovePC();

        if (isFiring)
        {
            fireDuration += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Escape))   //show cursor in editor
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void FixedUpdate()
    {
        CheckForGround();
    }


    void GetPlayerInput()
    {
        if (!uiManager.hudMessage)
        {
            // Keyboard input
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection.Normalize();
            moveDirection = transform.TransformDirection(moveDirection);
            if (Mathf.Abs(moveDirection.x) != 0 || Mathf.Abs(moveDirection.z) != 0)
            {
                if (Mathf.Abs(moveDirection.x) == 1 || Mathf.Abs(moveDirection.z) == 1)
                {
                    wasStopped = false;
                }
            }

            // Aerial
            if (isGrounded && !isJumping && Input.GetButtonDown("Jump"))
            {
                isJumping = true;
            }


            // Mouse input
            if (isEyeTracking || (!isEyeTracking && !uiManager.isHUDActive))
            {
                yRotation += Input.GetAxis("Mouse X") * yRotationSpeed * Time.deltaTime;
                xRotation -= Input.GetAxis("Mouse Y") * xRotationSpeed * Time.deltaTime;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                if (xRotation != cam.transform.eulerAngles.x || yRotation != transform.eulerAngles.y)
                {
                    cam.transform.localEulerAngles = new Vector3(xRotation, 0, 0);
                    transform.localEulerAngles = new Vector3(0, yRotation, 0);
                }

                if (!isSwitching)   // can only fire if not switching weapons
                {
                    if (Time.time > fireTime && Input.GetButton("Fire1"))
                    {
                        WeaponFire();
                    }
                    else if (Input.GetButtonUp("Fire1"))    // if player lets go of fire button
                    {
                        StoppedFiring();
                    }
                }
            }
        }
    }

    void ResetGravity()
    {
        appliedGravity = 0;
    }

    void MovePC()
    {
        if (isGrounded)
        {
            if (Mathf.Abs(moveDirection.x) != 0 || Mathf.Abs(moveDirection.z) != 0) // if there's some input
            {
                moveDirection *= baseSpeed;
            }
            else
            {
                // is idle
            }
        }
        else
        {
            if (Mathf.Abs(moveDirection.x) != 0 || Mathf.Abs(moveDirection.z) != 0) // if there's some input
            {
                moveDirection *= airBaseSpeed;
            }
        }

        ApplyJump();
        ApplyGravity();
        ApplyMovementModifiers();
        cc.Move(moveDirection * Time.deltaTime);
        moveDirection = Vector3.zero;
        if (cc.velocity == Vector3.zero) wasStopped = true;

        ResetGravityFromModifier();
    }

    void ApplyJump()
    {
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            moveDirection += Vector3.up * jumpHeight * (1 - (jumpTimer / jumpTimeLength));

            if (jumpTimer >= jumpTimeLength)
            {
                isJumping = false;
                appliedGravity = jumpTimer = 0;
            }
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            if (!isFalling)
            {
                isFalling = true;
                movementModifiers.Add(new MovementMod(cc.velocity / 2, 1, true, true, false));
            }
        }
        if (!isJumping)
        {
            moveDirection += Vector3.down * appliedGravity;
            appliedGravity += gravity * Time.deltaTime;
        }

    }

    #region Movement Mods

    void ApplyMovementModifiers()   // applies movement modifiers (e.g. motion retained when walking over an edge, or from an explosion)
    {
        for (int i = movementModifiers.Count - 1; i > -1; i--)
        {
            movementModifiers[i].modTimer += Time.deltaTime;

            if (movementModifiers[i].modTimer >= movementModifiers[i].modLength)    // if the movement modifier has timed out
            {
                movementModifiers.RemoveAt(i);
            }
            else
            {
                if (movementModifiers[i].modFadesOut)   // if the mod force fades out over time reduce it's force
                {
                    movementModifiers[i].currentVector = movementModifiers[i].modDirection * (1 - movementModifiers[i].modTimer / movementModifiers[i].modLength);
                }

                moveDirection += movementModifiers[i].currentVector;
            }
        }
    }

    void ResetGravityFromModifier()
    {
        for (int i = movementModifiers.Count - 1; i > -1; i--)
        {
            if (movementModifiers[i].resetGravityWhileActive)
            {
                appliedGravity = 0;
                return;
            }
        }
    }

    void GroundClearMoveMods()
    {
        for (int i = movementModifiers.Count - 1; i > -1; i--)
        {
            if (movementModifiers[i].removeWhenGrounded)
            {
                movementModifiers.RemoveAt(i);
            }
        }
    }

    #endregion

    bool CheckForGround()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, .4f, Vector3.down, out hit, .7f, walkableLayers))
        {
            appliedGravity = gravity / 3;
            isFalling = false;
            GroundClearMoveMods();
            return isGrounded = true;
        }
        else
        {
            return isGrounded = false;
        }
    }

    void WeaponFire()
    {
        switch (currentWeapon)
        {
            case WeaponType.AssaultRifle:
                WeaponFireAR();
                break;
            case WeaponType.Sniper:
                WeaponFireSR();
                break;
            case WeaponType.Launcher:
                WeaponFireGL();
                break;
            default:
                WeaponFireAR();
                break;
        }
    }

    void WeaponFireAR()
    {
        isFiring = true;
        gunAnim.speed = camAnim.speed = fireRateAR;
        gunAnim.SetBool("isFiringAR", isFiring);
        camAnim.SetBool("isFiringAR", isFiring);
        if (Time.time >= fireTime)
        {
            fireTime = Time.time + 1 / fireRateAR;

            Vector3 fireDirection = cam.transform.TransformDirection(Vector3.forward);

            // Spread
            spreadMultiplierAR = Mathf.Lerp(0, 1, fireDuration / timeToMaxSpreadAR);
            fireDirection = Quaternion.AngleAxis(Random.Range(-spreadDegreeAR, spreadDegreeAR) * spreadMultiplierAR, cam.transform.TransformDirection(Vector3.up)) * fireDirection;
            fireDirection = Quaternion.AngleAxis(Random.Range(-spreadDegreeAR, spreadDegreeAR) * spreadMultiplierAR, cam.transform.TransformDirection(Vector3.left)) * fireDirection;
            //

            RaycastHit hit;

            if (Physics.Raycast(cam.transform.position + cam.transform.TransformDirection(Vector3.forward) / 2, fireDirection, out hit, 200, weaponLayermask))
            {
                var trail = Instantiate(smokeTrailPrefabAR, hit.point, Quaternion.identity);
                trail.gunBarrel = barrel;
                if (hit.collider.gameObject.layer == 8) // if it's terrain
                {
                    //var part = Instantiate(onHitParticleAR, hit.point, Quaternion.identity);
                    //part.transform.forward = hit.normal;
                }
                else    // if it's a baddy
                {

                    if (hit.collider.gameObject.layer == 10)    // critical shot
                    {
                        hit.collider.GetComponent<BaddyHitbox>().TakeDamage(weaponDamageAR * 2);
                        //var part = Instantiate(onHitParticleAR, hit.point, Quaternion.identity);
                        //part.transform.forward = hit.normal;
                    }
                    else
                    {
                        hit.collider.GetComponent<BaddyHitbox>().TakeDamage(weaponDamageAR);
                        //var part = Instantiate(onHitParticleAR, hit.point, Quaternion.identity);
                        //part.transform.forward = hit.normal;
                    }

                }
            }
            else
            {
                var trail = Instantiate(smokeTrailPrefabAR, barrel.position + cam.transform.TransformDirection(Vector3.forward) * 20, Quaternion.identity);
                trail.gunBarrel = barrel;
            }
        }
    }

    void WeaponFireSR()
    {

    }

    void WeaponFireGL()
    {
        isFiring = true;
        gunAnim.speed = camAnim.speed = fireRateGL;
        gunAnim.SetBool("isFiringGL", isFiring);
        camAnim.SetBool("isFiringGL", isFiring);
        if (Time.time >= fireTime)
        {
            fireTime = Time.time + 1 / fireRateGL;
            Vector3 fireTraj = (Quaternion.AngleAxis(5, cam.transform.TransformDirection(Vector3.left)) 
                * cam.transform.TransformDirection(Vector3.forward)) * fireForceGL;

            ammoGL = Instantiate(ammoPrefabGL, barrel.position, Quaternion.identity);
            ammoGL.damage = weaponDamageGL;
            ammoGL.explosionForce = knockbackForceGL;
            ammoGL.rb.velocity = fireTraj;
        }
    }

    public void StoppedFiring()
    {
        isFiring = false;
        gunAnim.SetBool("isFiringAR", isFiring);
        gunAnim.SetBool("isFiringGL", isFiring);
        gunAnim.speed = 1;
        camAnim.SetBool("isFiringAR", isFiring);
        camAnim.SetBool("isFiringGL", isFiring);
        fireDuration = 0;
    }

    public void SwitchWeapon(WeaponType nextWeapon)
    {
        StartCoroutine(StartSwitchWeapon(nextWeapon));
    }

    private IEnumerator StartSwitchWeapon(WeaponType nextWeapon)
    {
        if (!isSwitching && nextWeapon != currentWeapon)
        {
            isSwitching = true;
            gunAnim.SetTrigger("switchWeapon");
            gunAnim.speed = timeToSwitchWeapons;
            yield return new WaitForSeconds(timeToSwitchWeapons);
            currentWeapon = nextWeapon;
            isSwitching = false;
        }
    }

    public void BigBaddyLanding()
    {
        camAnim.SetTrigger("bigBaddyLanding");
    }

    void CheckHealth()
    {
        if (health <= 0)
        {
            // Respawn
        }
        // Update UI
    }
}
