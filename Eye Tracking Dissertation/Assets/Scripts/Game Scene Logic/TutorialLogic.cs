using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLogic : MonoBehaviour
{
    public Checkpoint[] allCheckpoints;
    private int currentCheckpoint = 0;
    private bool isComplete;

    void Update()
    {
        if (!isComplete && allCheckpoints[currentCheckpoint].isComplete)
        {
            allCheckpoints[currentCheckpoint].OnCompletion();
            if (currentCheckpoint < allCheckpoints.Length - 1)
            {
                currentCheckpoint++;
                allCheckpoints[currentCheckpoint].OnBegin();
            }
            else isComplete = true;
        }
    }
}
