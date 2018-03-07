using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public GameObject cross;
    public int heals;
    private bool isInside;

    private void Update()
    {
        if (cross && isInside)
        {
            if (GameManager.gm.pc.Heal(heals)) Destroy(cross);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 12)   // is it PC?
        {
            isInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 12)   // is it PC?
        {
            isInside = false;
        }
    }
}
