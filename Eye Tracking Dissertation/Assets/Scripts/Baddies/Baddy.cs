using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Baddy : MonoBehaviour
{
    [HideInInspector]
    public bool isDestroyed;
    public int health;
    private int startHealth;
    private float particleProgress = 0.0f;
    public Rigidbody criticalPoint;
    public CapsuleCollider baddyBody;
    public CapsuleCollider baddyBase;
    public ParticleSystem initialDamageVFX;
    public int initialDamageRateOverTime;
    public float initialDamageSizeMin;
    public float initialDamageSizeMax;
    private ParticleSystem.MainModule initialMain;
    private ParticleSystem.EmissionModule initialEmission;
    public ParticleSystem severeDamageVFX;
    public int severeDamageRateOverTime;
    public float severeDamageSizeMin;
    public float severeDamageSizeMax;
    public float severeDamageSpeedMin;
    public float severeDamageSpeedMax;
    private ParticleSystem.MainModule severeMain;
    private ParticleSystem.EmissionModule severeEmission;
    public ParticleSystem deathExplosionVFX;
    public DestroyIn destroyIn;
    public string destructionAnimationName;

    void Start()
    {
        startHealth = health;
        initialMain = initialDamageVFX.main;
        initialEmission = initialDamageVFX.emission;
        severeMain = severeDamageVFX.main;
        severeEmission = severeDamageVFX.emission;
    }


    void Update()
    {

    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        CheckHealth();
    }

    private void CheckHealth()
    {
        if (health <= 0 && !isDestroyed)
        {
            isDestroyed = true;
            Explode();
        }
        else
        {
            DamageVFX();
        }
    }

    private void DamageVFX()
    {
        if (health > startHealth / 2)   // while above 50% health
        {
            particleProgress = 1 - ((health - (startHealth / 2.0f)) / (startHealth / 2.0f));
            initialMain.startColor = Color.Lerp(Color.clear, Color.black, particleProgress);
            var startSize = initialMain.startSize;
            startSize.constantMin = Mathf.Lerp(0, .5f, particleProgress);
            startSize.constantMax = Mathf.Lerp(0, 10, particleProgress);
            initialEmission.rateOverTime = Mathf.Lerp(0, 20, particleProgress);
        }
        else
        {
            particleProgress = 1 - (health / (startHealth / 2.0f));
            severeMain.startColor = Color.Lerp(Color.clear, Color.black, particleProgress);
            var startSize = severeMain.startSize;
            startSize.constantMin = Mathf.Lerp(0, .5f, particleProgress);
            startSize.constantMax = Mathf.Lerp(0, 10, particleProgress);
            var startSpeed = severeMain.startSpeed;
            startSpeed.constantMin = Mathf.Lerp(0, 4, particleProgress);
            startSpeed.constantMax = Mathf.Lerp(0, 10, particleProgress);
            severeEmission.rateOverTime = Mathf.Lerp(0, 10, particleProgress);
        }
    }

    private void Explode()
    {
        Instantiate(deathExplosionVFX, transform.position, Quaternion.identity);
        Invoke("StopVFX", 1);
        GameManager.gm.pc.camAnim.SetTrigger(destructionAnimationName);
        GameManager.gm.pc.camAnim.speed = 1;
        //criticalPoint.GetComponent<SphereCollider>().enabled = false;
        //baddyBody.enabled = false;
        //baddyBase.enabled = false;
        criticalPoint.transform.parent = null;
        criticalPoint.GetComponent<DestroyIn>().enabled = true;
        criticalPoint.isKinematic = false;
        criticalPoint.AddForce(Vector3.up * 200 + (Vector3.left / Random.Range(1, 50)) + (Vector3.forward / Random.Range(1, 50)), ForceMode.VelocityChange);
        criticalPoint.transform.parent = null;
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(Vector3.up * 10, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.left * Random.Range(1, 5), ForceMode.VelocityChange);
        NavMeshAgent tempNMA = GetComponent<NavMeshAgent>();
        if (tempNMA) Destroy(tempNMA);
        destroyIn.enabled = true;
    }

    void StopVFX()
    {
        initialDamageVFX.Stop();
        severeDamageVFX.Stop();
    }
}
