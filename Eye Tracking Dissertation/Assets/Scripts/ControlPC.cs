using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PostProcessing;

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
    public float appliedYRotationSpeed = 45;
    public float xRotationSpeed = 45;
    public float appliedXRotationSpeed = 45;
    private float yRotation;
    private float xRotation;

    // UI
    [Header("UI")]
    public UIManager uiManager;
    private PostProcessingProfile postProf;

    // Stats
    [Header("Stats")]
    public bool isInvincible;
    public int health = 100;
    public int shields = 100;
    public float shieldCooldownLength = 3;  // time without damage taken before shield recharges
    private float timeSinceDamageTaken = 0;
    public float shieldRechargeLength = 2;
    private float shieldRechargeTimer = 0;

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
    [SerializeField]
    private float maxEffectiveRange = 7.5f;
    [SerializeField]
    private AudioSource[] assaultRifleSFX;

    // Sniper
    [Space(10)]
    [Header("Sniper Rifle")]
    [SerializeField]
    private int weaponDamageSR;
    [SerializeField]
    private float fireRateSR;
    [SerializeField]
    private float unscopedSpreadSR = 10;
    [SerializeField]
    private SmokeTrail smokeTrailPrefabSR;
    [SerializeField]
    private AudioSource[] sniperRifleSFX;
    private bool isScoped;

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
    [SerializeField]
    private AudioSource[] grenadeLauncherSFX;

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
        cam.fieldOfView = 75;
        health = 100;
        yRotation = transform.localEulerAngles.y;
        appliedYRotationSpeed = yRotationSpeed;
        xRotation = cam.transform.localEulerAngles.x;
        appliedXRotationSpeed = xRotationSpeed;
        wasStopped = true;
        appliedGravity = gravity / 2;
        gunAnim.speed = .5f;
        postProf = cam.GetComponent<PostProcessingBehaviour>().profile;
        var vigSetts = postProf.vignette.settings;
        vigSetts.intensity = 0;
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

        timeSinceDamageTaken += Time.deltaTime;

        if (shields < 100 && timeSinceDamageTaken > shieldCooldownLength)
        {
            StartCoroutine("ShieldRegen");
        }

        if (Input.GetKeyDown(KeyCode.Escape))   //show cursor in editor
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameManager.gm.GoToScene(0);
        }
    }

    void FixedUpdate()
    {
        CheckForGround();
    }


    void GetPlayerInput()
    {
        if ((isEyeTracking || (!isEyeTracking && !uiManager.hudMessage)))   // if it's 
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
                yRotation += Input.GetAxis("Mouse X") * appliedYRotationSpeed * Time.deltaTime;
                xRotation -= Input.GetAxis("Mouse Y") * appliedXRotationSpeed * Time.deltaTime;
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

                    if (currentWeapon == WeaponType.Sniper)
                    {
                        if (Input.GetMouseButton(1))
                        {
                            EnterSniperFOV();
                        }
                        else if (Input.GetMouseButtonUp(1))
                        {
                            LeaveSniperFOV();
                        }
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
        gunAnim.SetBool("isFiringAR", isFiring);
        camAnim.SetBool("isFiringAR", isFiring);
        if (Time.time >= fireTime)
        {
            Instantiate(assaultRifleSFX[Random.Range(0, assaultRifleSFX.Length)], barrel.position, Quaternion.identity);
            gunAnim.speed = camAnim.speed = fireRateAR;
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
                        hit.collider.GetComponent<BaddyHitbox>().TakeDamage(IsBeyondEffectiveRange(hit.point) ? weaponDamageAR / 2 : weaponDamageAR * 2);
                        //var part = Instantiate(onHitParticleAR, hit.point, Quaternion.identity);
                        //part.transform.forward = hit.normal;
                    }
                    else
                    {
                        hit.collider.GetComponent<BaddyHitbox>().TakeDamage(IsBeyondEffectiveRange(hit.point) ? weaponDamageAR / 4 : weaponDamageAR);
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

    bool IsBeyondEffectiveRange(Vector3 target)
    {
        float distance = (target = cam.transform.position).sqrMagnitude;

        if (distance > maxEffectiveRange * maxEffectiveRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void WeaponFireSR()
    {
        isFiring = true;
        gunAnim.SetBool("isFiringSR", isFiring);
        camAnim.SetBool("isFiringSR", isFiring);
        if (Time.time >= fireTime)
        {
            Instantiate(sniperRifleSFX[Random.Range(0, sniperRifleSFX.Length)], barrel.position, Quaternion.identity);
            gunAnim.speed = camAnim.speed = fireRateSR;
            fireTime = Time.time + 1 / fireRateSR;

            Vector3 fireDirection = cam.transform.TransformDirection(Vector3.forward);
            if (!isScoped)
            {
                fireDirection = Quaternion.AngleAxis(Random.Range(-unscopedSpreadSR, unscopedSpreadSR), cam.transform.TransformDirection(Vector3.up)) * fireDirection;
                fireDirection = Quaternion.AngleAxis(Random.Range(-unscopedSpreadSR, unscopedSpreadSR), cam.transform.TransformDirection(Vector3.left)) * fireDirection;
            }
            RaycastHit hit;

            if (Physics.Raycast(cam.transform.position, fireDirection, out hit, 500, weaponLayermask))
            {
                var trail = Instantiate(smokeTrailPrefabSR, hit.point, Quaternion.identity);
                trail.gunBarrel = barrel;

                if (hit.collider.gameObject.layer == 10)    // critical shot
                {
                    hit.collider.GetComponent<BaddyHitbox>().TakeDamage(weaponDamageSR * 2);
                    //var part = Instantiate(onHitParticleAR, hit.point, Quaternion.identity);
                    //part.transform.forward = hit.normal;
                }
                else if (hit.collider.gameObject.layer == 9)
                {
                    hit.collider.GetComponent<BaddyHitbox>().TakeDamage(weaponDamageSR);
                    //var part = Instantiate(onHitParticleAR, hit.point, Quaternion.identity);
                    //part.transform.forward = hit.normal;
                }
            }
            else
            {
                var trail = Instantiate(smokeTrailPrefabSR, barrel.position + cam.transform.TransformDirection(Vector3.forward) * 20, Quaternion.identity);
                trail.gunBarrel = barrel;
            }
        }
    }

    void EnterSniperFOV()
    {
        cam.fieldOfView = 20;
        isScoped = true;
        uiManager.sniperReticle.gameObject.SetActive(true);
        appliedXRotationSpeed = xRotationSpeed / 2;
        appliedYRotationSpeed = yRotationSpeed / 2;
    }

    public void LeaveSniperFOV()
    {
        cam.fieldOfView = 75;
        isScoped = false;
        uiManager.sniperReticle.gameObject.SetActive(false);
        appliedXRotationSpeed = xRotationSpeed;
        appliedYRotationSpeed = yRotationSpeed;
    }

    void WeaponFireGL()
    {
        isFiring = true;
        gunAnim.SetBool("isFiringGL", isFiring);
        camAnim.SetBool("isFiringGL", isFiring);
        if (Time.time >= fireTime)
        {
            Instantiate(grenadeLauncherSFX[Random.Range(0, grenadeLauncherSFX.Length)], barrel.position, Quaternion.identity);
            gunAnim.speed = camAnim.speed = fireRateGL;
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
        gunAnim.SetBool("isFiringSR", isFiring);
        gunAnim.speed = 1;
        camAnim.SetBool("isFiringAR", isFiring);
        camAnim.SetBool("isFiringGL", isFiring);
        camAnim.SetBool("isFiringSR", isFiring);
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
            LeaveSniperFOV();
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
        camAnim.speed = 1;
        camAnim.SetTrigger("bigBaddyLanding");
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            StopCoroutine("ShieldRegen");
            timeSinceDamageTaken = 0;
            if (shields > 0)
            {
                shields -= damage;
            }
            else
            {
                health -= damage;
            }

            CheckHealth();
        }
    }

    public bool Heal(int heals)
    {
        if (health < 100)
        {
            health += heals;
            Mathf.Clamp(health, 0, 100);
            uiManager.health.value = health;
            return true;
        }
        return false;
    }

    void CheckHealth()
    {
        if (shields > 0)
        {
            uiManager.shields.value = shields;
        }
        else
        {
            uiManager.shields.value = 0;
            if (health <= 0)
            {
#if UNITY_EDITOR
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
#else
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
#endif
                GameManager.gm.GoToScene(0);
            }
            else
            {
                uiManager.health.value = health;
                var vigSetts = postProf.vignette.settings;
                vigSetts.intensity = Mathf.Lerp(0, .3f, (1 - health / 100));
            }
        }
        Mathf.Clamp(shields, 0, 100);
        Mathf.Clamp(health, 0, 100);
    }

    IEnumerator ShieldRegen()
    {
        shieldRechargeTimer = 0;
        int shieldValue = shields;
        float progress = 0;
        while (shieldRechargeTimer < shieldRechargeLength)
        {
            shieldRechargeTimer += Time.deltaTime;
            progress = shieldRechargeTimer / shieldRechargeLength;
            uiManager.shields.value = Mathf.Lerp(shieldValue, 100, shieldRechargeTimer / shieldRechargeLength);
            shields = (int)uiManager.shields.value;
            yield return null;
        }
    }
}
