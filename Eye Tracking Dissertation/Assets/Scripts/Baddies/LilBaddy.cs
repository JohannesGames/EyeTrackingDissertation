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
    public List<Transform> targetPositions = new List<Transform>();
    private int currentTarget = 0;

    // Combat
    private bool isAttacking;
    [HideInInspector]
    public bool isHuntingPC;
    public int damage = 15;
    public float attackCooldown = 3;
    private float nextAttackTime;
    public LineRenderer electricAttack;


    void Start()
    {
        nma = GetComponent<NavMeshAgent>();
        cc = GetComponent<CharacterController>();
        baddy = GetComponent<Baddy>();
        Invoke("FindPC", 0);
    }

    void FindPC()
    {
        if (targetPositions.Count == 0)
        {
            targetPositions.Add(GameManager.gm.pc.transform);
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

        if (isAttacking && Time.time > nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            AttackPC();
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
                    currentTarget = Random.Range(0, targetPositions.Count - 1);
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
        electricAttack.gameObject.SetActive(true);
        electricAttack.SetPosition(0, transform.position);
        electricAttack.SetPosition(1, GameManager.gm.pc.transform.position);
        GameManager.gm.pc.TakeDamage(damage);
        Invoke("HideElectric", .15f);
    }

    void HideElectric()
    {
        electricAttack.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 12)   // is it the PC?
        {
            isAttacking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 12)   // is it the PC?
        {
            isAttacking = false;
        }
    }
}
