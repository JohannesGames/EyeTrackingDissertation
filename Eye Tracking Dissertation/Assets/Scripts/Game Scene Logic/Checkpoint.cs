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

    public virtual void OnBegin() { }



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
