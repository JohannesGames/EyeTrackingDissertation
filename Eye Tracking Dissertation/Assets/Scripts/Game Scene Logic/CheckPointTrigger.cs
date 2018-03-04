using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTrigger : Checkpoint
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 12)   // is it the PC?
        {
            OnCompletion();
        }
    }
}
