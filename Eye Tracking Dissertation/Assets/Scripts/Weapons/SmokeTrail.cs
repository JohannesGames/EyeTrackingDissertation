using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeTrail : MonoBehaviour
{
    LineRenderer lr;
    Color startColour;
    public Color endColour;
    public float fadeLength = 2;
    private float fadeTimer = 0;
    private float fadeProgress;
    [HideInInspector]
    public Transform gunBarrel;
    public bool isFollowing;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        startColour = lr.material.color;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, gunBarrel.position);
    }


    void Update()
    {
        fadeTimer += Time.deltaTime;
        lr.material.color = Color.Lerp(startColour, endColour, fadeTimer / fadeLength);

        if (isFollowing)
        {
            lr.SetPosition(1, gunBarrel.position);
        }
    }
}
