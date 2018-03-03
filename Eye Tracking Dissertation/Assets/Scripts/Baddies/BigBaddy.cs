using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBaddy : MonoBehaviour
{
    [HideInInspector]
    public Vector3 landingSpot;
    public Transform lilBaddySpawnPoint;
    private Vector3 startPosition;
    private Vector3 positionLastFrame;
    public float timeToLand = 3;
    public float startingAltitude = 100;
    public AnimationCurve landingVelocity;
    public ParticleSystem landingVFX;

    void Start()
    {
        positionLastFrame = transform.position + Vector3.up;
        StartCoroutine("Descent");
    }


    void Update()
    {

    }

    private IEnumerator Descent()
    {
        float timer = 0;
        float progress = 0;

        landingSpot = transform.position;
        startPosition = transform.position = landingSpot + Vector3.up * startingAltitude + Vector3.left * Random.Range(0, 100) + Vector3.forward * Random.Range(0, 100);

        while (progress < 1)
        {
            timer += Time.deltaTime;
            progress = timer / timeToLand;
            transform.position = Vector3.Lerp(startPosition, landingSpot, landingVelocity.Evaluate(progress));
            transform.up = positionLastFrame - transform.position;
            positionLastFrame = transform.position;
            yield return null;
        }
        landingVFX.Play();
        Destroy(landingVFX.gameObject, 3);
        GameManager.gm.pc.BigBaddyLanding();
    }
}
