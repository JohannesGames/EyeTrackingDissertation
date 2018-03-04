using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointWave : Checkpoint
{
    private int killsRequired = 0;
    private int killCounter = 0;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (killCounter > 0 && killCounter >= killsRequired)
        {
            OnCompletion();
        }
    }

    //public override void OnBegin()
    //{
    //    if (baddiesToSpawn.Length > 0)
    //    {
    //        foreach (BaddiesToSpawn baddy in baddiesToSpawn)
    //        {
    //            for (int i = 0; i < baddy.amountToSpawn; i++)
    //            {
    //                var _baddy = Instantiate(baddy.prefabToSpawn, baddy.spawnPoints[Random.Range(0, baddy.spawnPoints.Length - 1)].position + Random.insideUnitSphere, Quaternion.identity);
    //                _baddy.deathCount = AddToKillCounter;
    //                killsRequired++;
    //                if (baddy.targets.Length > 0)
    //                {
    //                    LilBaddy lb = _baddy.GetComponent<LilBaddy>();
    //                    for (int j = 0; j < baddy.targets.Length; j++)
    //                    {
    //                        lb.targetPositions[j] = baddy.targets[j];
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    if (objectsToActivate.Length > 0)
    //    {
    //        foreach (GameObject item in objectsToActivate)
    //        {
    //            item.SetActive(true);
    //        }
    //    }
    //}

    //public void AddToKillCounter()
    //{
    //    killCounter++;
    //}
}
