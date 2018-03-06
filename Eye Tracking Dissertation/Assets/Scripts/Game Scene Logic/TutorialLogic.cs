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
        else if (AllComplete())
        {
#if UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
#else
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
#endif
            GameManager.gm.GoToScene(0);
        }
    }

    bool AllComplete()
    {
        for (int i = 0; i < allCheckpoints.Length; i++)
        {
            if (!allCheckpoints[i].isComplete)
            {
                return false;
            }
        }
        return true;
    }
}
