using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLauncherAmmo : MonoBehaviour
{
    public int damage;
    public Rigidbody rb;
    public float explodeAfter = 2;
    private float explodeTimer;
    public float explosionRange = 3;
    public float explosionForce;
    public ParticleSystem explosionVFX;
    public LayerMask enemyLayer;
    public LayerMask playerLayer;
    private bool hasExploded;
    [SerializeField]
    private AudioSource[] impactSFX;

    private void Start()
    {
        explodeTimer = Time.time + explodeAfter;
    }

    private void Update()
    {
        if (!hasExploded && Time.time >= explodeTimer)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!hasExploded)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (!hasExploded)
        {
            hasExploded = true;
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRange, enemyLayer);

            if (hits.Length > 0)
            {
                foreach (Collider item in hits)
                {
                    BaddyHitbox _bad = item.GetComponent<BaddyHitbox>();
                    _bad.baddy.TakeDamage(damage);
                    _bad.baddy.rb.AddExplosionForce(explosionForce, transform.position, explosionRange, 0, ForceMode.VelocityChange);
                }
            }
            hits = Physics.OverlapSphere(transform.position, 10, playerLayer);
            if (hits.Length > 0)
            {
                GameManager.gm.pc.camAnim.speed = 1;
                GameManager.gm.pc.camAnim.SetTrigger("lilExplosion");
            }
            Instantiate(impactSFX[Random.Range(0, impactSFX.Length)], transform.position, Quaternion.identity);
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
