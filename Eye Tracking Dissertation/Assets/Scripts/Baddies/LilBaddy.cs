using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LilBaddy : MonoBehaviour
{
    NavMeshAgent nma;
    CharacterController cc;
    Baddy baddy;

    public float gravity = 5;
    public float checkForNewTargetAfter = 1.5f;
    private float checkTime;

    // Navigation
    public Transform[] targetPositions = new Transform[10];
    private int currentTarget = 0;

    // Combat
    private bool isAttacking;
    [HideInInspector]
    public bool isHuntingPC;


    void Start()
    {
        nma = GetComponent<NavMeshAgent>();
        cc = GetComponent<CharacterController>();
        baddy = GetComponent<Baddy>();
        if (targetPositions.Length == 0)
        {
            targetPositions[0].position = GameManager.gm.pc.transform.position;
        }
    }


    void Update()
    {
        if (!isAttacking && !baddy.isDestroyed)
        {
            if (!cc.isGrounded)
            {
                cc.Move(Vector3.down * gravity * Time.deltaTime);
            }
            else if (nma.isOnNavMesh)
            {
                MoveToPC();
            }
        }
    }

    void MoveToPC()
    {
        if (Time.time >= checkTime)
        {
            checkTime = Time.time + checkForNewTargetAfter;
            if (!isHuntingPC && !isAttacking)
            {
                for (int i = 0; i < 1; i++)
                {
                    currentTarget = Random.Range(0, targetPositions.Length - 1);
                    if (!targetPositions[currentTarget]) i--;
                }
                nma.SetDestination(targetPositions[currentTarget].position + Random.insideUnitSphere * 4);
            }
            else if (isHuntingPC && targetPositions[0].position != GameManager.gm.pc.transform.position)
            {
                targetPositions[0].position = GameManager.gm.pc.transform.position;
                nma.SetDestination(targetPositions[0].position + Random.insideUnitSphere * 4);
            }
        }
    }

    void AttackPC()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 12)   // is it the PC?
        {
            AttackPC();
        }
    }
}
