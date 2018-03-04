using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLogic : MonoBehaviour
{
    public Checkpoint[] allCheckpoints;
    private int currentCheckpoint = 0;
    private bool isComplete;

    private void Start()
    {
        GameManager.gm.currentCheckpoint = allCheckpoints[currentCheckpoint];
    }

    void Update()
    {
        if (!isComplete && allCheckpoints[currentCheckpoint].isComplete)
        {
            allCheckpoints[currentCheckpoint].OnCompletion();
            if (currentCheckpoint < allCheckpoints.Length - 1)
            {
                currentCheckpoint++;
                GameManager.gm.currentCheckpoint = allCheckpoints[currentCheckpoint];
                allCheckpoints[currentCheckpoint].OnBegin();
            }
            else isComplete = true;
        }
    }
}
