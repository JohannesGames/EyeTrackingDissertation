using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gm;
    [HideInInspector]
    public ControlPC pc;

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
}
