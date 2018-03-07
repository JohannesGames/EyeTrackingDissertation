using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBaddy : MonoBehaviour
{
    [HideInInspector]
    public Vector3 landingSpot;
    public Transform lilBaddySpawnPoint;
    private Vector3 startPosition;
    private Vector3 positionLastFrame;
    public float timeToLand = 3;
    public float startingAltitude = 100;
    public AnimationCurve landingVelocity;
    public ParticleSystem landingVFX;
    private Baddy baddy;
    public float spawnFrequency = 4;
    public float spawnAmount = 5;
    [HideInInspector]
    public List<Transform> targetPositions = new List<Transform>();

    // Combat
    private bool isCharging;
    public int damage = 45;
    private float distancePC;
    private float checkTimePC;
    public float attackCooldown = 3;
    private float nextAttackTime;
    public float chargeLength;   // how long must the PC be in view before bigBaddy fires
    private float chargeTimer;
    public LineRenderer attackBeam;
    public ParticleSystem weaponCharger;
    private ParticleSystem weaponChargerChild;
    public ParticleSystem attackVFX;

    void Start()
    {
        positionLastFrame = transform.position + Vector3.up;
        baddy = GetComponent<Baddy>();
        StartCoroutine("Descent");
        checkTimePC = Time.time + .5f;
        weaponChargerChild = weaponCharger.GetComponentInChildren<ParticleSystem>();
        nextAttackTime = Time.time + timeToLand + 1;
        attackBeam.SetPosition(0, attackBeam.transform.position);
    }


    void LateUpdate()
    {
        if (baddy.isDestroyed)
        {
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (!baddy.isDestroyed && Time.time > nextAttackTime && Time.time >= checkTimePC)
        {
            checkTimePC = isCharging ? 0 : Time.time + .5f;
            distancePC = Vector3.Distance(GameManager.gm.pc.transform.position, transform.position);
            if (!Physics.Linecast(baddy.criticalPoint.transform.position, GameManager.gm.pc.transform.position, GameManager.gm.terrainLayer))
            {
                // if no terrain between baddy and PC charge up weapon
                if (!isCharging)
                {
                    weaponChargerChild.Play();
                    var emission = weaponCharger.emission;
                    emission.rateOverTime = 40;
                    StartCoroutine("ChargeWeapon");
                }
            }
            else if (isCharging)
            {
                isCharging = false;
                weaponChargerChild.Stop();
                var emission = weaponCharger.emission;
                emission.rateOverTime = 0;
                attackBeam.gameObject.SetActive(false);
                StopCoroutine("ChargeWeapon");
            }
        }
    }

    private IEnumerator Descent()
    {
        float timer = 0;
        float progress = 0;

        landingSpot = transform.position;
        startPosition = transform.position = landingSpot + Vector3.up * startingAltitude + Vector3.left * Random.Range(0, 100) + Vector3.forward * Random.Range(0, 100);

        while (progress < 1)
        {
            timer += Time.deltaTime;
            progress = timer / timeToLand;
            transform.position = Vector3.Lerp(startPosition, landingSpot, landingVelocity.Evaluate(progress));
            transform.up = positionLastFrame - transform.position;
            positionLastFrame = transform.position;
            yield return null;
        }
        landingVFX.Play();
        Destroy(landingVFX.gameObject, 3);
        GameManager.gm.pc.BigBaddyLanding();
        nextAttackTime = Time.time + 1;
        StartCoroutine("SpawnThem");
        StartCoroutine("SpawnLilBaddies");
    }

    IEnumerator SpawnLilBaddies()
    {
        while (!baddy.isDestroyed)
        {
            yield return new WaitForSeconds(spawnFrequency - 2);
            // Signal spawn coming 2 seconds before

            yield return new WaitForSeconds(2);
            StartCoroutine("SpawnThem");
        }
    }

    IEnumerator SpawnThem()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            Instantiate(GameManager.gm.lilBaddyPrefab, lilBaddySpawnPoint.position + Random.insideUnitSphere, Quaternion.identity);
            yield return null;
        }
    }

    IEnumerator ChargeWeapon()
    {
        isCharging = true;
        chargeTimer = 0;
        float progress = 0;
        var main = weaponCharger.main;
        attackBeam.gameObject.SetActive(true);
        while (chargeTimer < chargeLength)
        {
            progress = chargeTimer / chargeLength;
            main.startSize = Mathf.Lerp(0, 12, progress);
            chargeTimer += Time.deltaTime;
            UpdateAttackBeam(progress);

            yield return null;
        }

        // Resolve attack
        Vector3 attackPos = GameManager.gm.pc.transform.position + Random.insideUnitSphere * 2;
        Instantiate(attackVFX, attackPos, Quaternion.identity);
        GameManager.gm.pc.BigBaddyLanding();
        Collider[] hits = Physics.OverlapSphere(attackPos, 1, GameManager.gm.playerLayer);
        if (hits.Length > 0)
        {
            GameManager.gm.pc.TakeDamage(damage);
        }
        //

        attackBeam.gameObject.SetActive(false);
        isCharging = false;
        weaponChargerChild.Stop();
        var emission = weaponCharger.emission;
        emission.rateOverTime = 0;
        nextAttackTime = Time.time + attackCooldown;
    }

    void UpdateAttackBeam(float progress)
    {
        attackBeam.startWidth = attackBeam.endWidth = Mathf.Lerp(.001f, .1f, progress);
        attackBeam.SetPosition(0, baddy.criticalPoint.transform.position);
        attackBeam.SetPosition(1, GameManager.gm.pc.transform.position + Random.insideUnitSphere * .5f * progress);
    }
}
