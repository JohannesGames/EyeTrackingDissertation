using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaddiesToSpawn
{
    public Baddy prefabToSpawn;
    public int amountToSpawn;
    public Transform[] spawnPoints;
    public Transform[] targets;
}

public abstract class Checkpoint : MonoBehaviour
{
    public bool isComplete;
    public BaddiesToSpawn[] baddiesToSpawn;
    public GameObject[] objectsToDelete;
    public GameObject[] objectsToActivate;

    public virtual void OnBegin()
    {
        //if (baddiesToSpawn.Length > 0)
        //{
        //    foreach (BaddiesToSpawn baddy in baddiesToSpawn)
        //    {
        //        for (int i = 0; i < baddy.amountToSpawn; i++)
        //        {
        //            var _baddy = Instantiate(baddy.prefabToSpawn, baddy.spawnPoints[Random.Range(0, baddy.spawnPoints.Length - 1)].position, Quaternion.identity);
        //            if (baddy.targets.Length > 0)
        //            {
        //                LilBaddy lb = _baddy.GetComponent<LilBaddy>();
        //                for (int j = 0; j < baddy.targets.Length; j++)
        //                {
        //                    lb.targetPositions[j] = baddy.targets[j];
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
    }

    public virtual void OnCompletion()
    {
        isComplete = true;

        if (objectsToDelete.Length > 0)
        {
            foreach (GameObject item in objectsToDelete)
            {
                Destroy(item.gameObject);
            }
        }

        //Destroy(gameObject);
    }
}
