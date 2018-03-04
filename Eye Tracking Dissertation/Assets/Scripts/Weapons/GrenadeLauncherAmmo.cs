using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLauncherAmmo : MonoBehaviour
{
    public int damage;
    public Rigidbody rb;

    private void OnCollisionEnter(Collision collision)
    {
        Baddy _bad = collision.collider.GetComponent<Baddy>();
        if (_bad)
        {
            _bad.TakeDamage(damage);
        }
    }
}
