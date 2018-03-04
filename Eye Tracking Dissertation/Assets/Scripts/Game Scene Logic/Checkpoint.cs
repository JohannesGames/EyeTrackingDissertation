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

public class Checkpoint : MonoBehaviour
{
    public enum CheckpointTypes
    {
        Trigger,
        Wave
    }
    public CheckpointTypes checkpiontType;

    public bool isComplete;
    public BaddiesToSpawn[] baddiesToSpawn;
    public GameObject[] objectsToDelete;
    public GameObject[] objectsToActivate;

    // Wave
    public float spawnDelay;
    int killsRequired = 0;
    int killCounter = 0;

    void Update()
    {
        if (checkpiontType == CheckpointTypes.Wave)
        {
            if (killCounter > 0 && killCounter >= killsRequired)
            {
                OnCompletion();
            }
        }
    }

    #region OnBegin() functions

    public void OnBegin()
    {
        if (checkpiontType == CheckpointTypes.Wave)
        {
            OnBeginWave();
        }
    }

    private void OnBeginWave()
    {
        Invoke("SpawnBaddies", spawnDelay);

        if (objectsToActivate.Length > 0)
        {
            foreach (GameObject item in objectsToActivate)
            {
                item.SetActive(true);
            }
        }
    }

#endregion

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

    #region Wave functions
    private void SpawnBaddies()
    {
        if (baddiesToSpawn.Length > 0)
        {
            foreach (BaddiesToSpawn baddy in baddiesToSpawn)
            {
                for (int i = 0; i < baddy.amountToSpawn; i++)
                {
                    var _baddy = Instantiate(baddy.prefabToSpawn, baddy.spawnPoints[Random.Range(0, baddy.spawnPoints.Length - 1)].position + Random.insideUnitSphere, Quaternion.identity);
                    killsRequired++;
                    if (baddy.targets.Length > 0)
                    {
                        LilBaddy lb = _baddy.GetComponent<LilBaddy>();
                        if (lb)
                        {
                            foreach (Transform item in baddy.targets)
                            {
                                lb.targetPositions.Add(item);
                            }
                        }
                        else
                        {
                            BigBaddy bb = _baddy.GetComponent<BigBaddy>();
                            foreach (Transform item in baddy.targets)
                            {
                                bb.targetPositions.Add(item);
                            }
                        }
                    }
                }
            }
        }
    }

    public void AddToKillCounter()
    {
        killCounter++;
    }
#endregion

    private void OnTriggerEnter(Collider other)
    {
        if (checkpiontType == CheckpointTypes.Trigger)  // only check for trigger type of checkpoint
        {
            if (other.gameObject.layer == 12)   // is it the PC?
            {
                OnCompletion();
            }
        }
    }
}
