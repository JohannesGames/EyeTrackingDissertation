using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddyHitbox : MonoBehaviour
{
    private Baddy baddy;
    

    void Start()
    {
        baddy = GetComponentInParent<Baddy>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(int damage)
    {
        baddy.TakeDamage(damage);
    }
}
