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
    public float checkForPCAfter = 1.5f;
    private float pcCheckTime;

    // Navigation
    private Vector3 pcPosition;

    // Combat
    private bool isAttacking;


    void Start()
    {
        nma = GetComponent<NavMeshAgent>();
        cc = GetComponent<CharacterController>();
        baddy = GetComponent<Baddy>();
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
        if (Time.time >= pcCheckTime)
        {
            pcCheckTime = Time.time + checkForPCAfter;
            if (pcPosition != GameManager.gm.pc.transform.position)
            {
                pcPosition = GameManager.gm.pc.transform.position + Random.insideUnitSphere * 4;
                nma.SetDestination(pcPosition);
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
