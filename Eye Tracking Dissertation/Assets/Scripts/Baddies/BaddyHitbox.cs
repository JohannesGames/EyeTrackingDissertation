using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddyHitbox : MonoBehaviour
{
    public Baddy baddy;
    

    void Start()
    {
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
