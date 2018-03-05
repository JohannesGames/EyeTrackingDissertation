using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct HUDMessage
{
    public HUDMessage(float _delay, string _messageBody)
    {
        delay = _delay;
        messageBody = _messageBody;
    }
    public float delay;
    [TextArea(2, 10)]
    public string messageBody;
}

public class GameManager : MonoBehaviour
{
    public static GameManager gm;
    [HideInInspector]
    public ControlPC pc;

    //UI
    public List<HUDMessage> hudMessagesToBeDisplayed = new List<HUDMessage>();

    // Game logic
    public Checkpoint currentCheckpoint;

    // Baddies
    [Header("Baddies")]
    public BigBaddy bigBaddyPrefab;
    public LilBaddy lilBaddyPrefab;

    void Awake()
    {
        if (!gm)
        {
            gm = this;
        }
        else
        {
            Destroy(gameObject);
        }
        //DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DisplayNextMessage()
    {
        if (hudMessagesToBeDisplayed.Count > 0)
        {
            Invoke("ShowHUDMessage", hudMessagesToBeDisplayed[0].delay);
        }
    }

    void ShowHUDMessage()
    {
        pc.uiManager.OpenHUDMessage(hudMessagesToBeDisplayed[0].messageBody);
        hudMessagesToBeDisplayed.RemoveAt(0);
    }

    public void GoToScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
