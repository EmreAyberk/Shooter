using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State{ Idle,Chasing,Attacking }

    private State currentState;
    private NavMeshAgent pathFinder;
    private Transform target;
    private LivingEntity targetEntity;
    private Material skinMaterial;
    private Color originalColour;
    
    private float attackDistanceThreshold = .5f;
    private float timeBetweenAttacks = 1;
    private float damage = 1;

    private float targetCollionRadius;
    private float enemyCollisionRadius;
    private float nextAttackTime;
    private bool hasTarget;

    public override void Start()
    {
        base.Start();
        
        pathFinder = GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer> ().material;
        originalColour = skinMaterial.color;
        
        //When Instantiate enemy after Player died -> NullException
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;
            hasTarget = true;
            
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDead += OnTargetDeath;
        
            enemyCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
       
       

         
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        if(hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + targetCollionRadius + enemyCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
            
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }
    IEnumerator Attack() {

        currentState = State.Attacking;
        pathFinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (targetCollionRadius);

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;
        
        bool hasAppliedDamage = false;
        
        while (percent <= 1)
        {
            if (percent >= .5 && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            // 0,0 - 0.5,1 - 1,0 equation
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColour;
        currentState = State.Chasing;
        pathFinder.enabled = true;
    }
    

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;
        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget *  (enemyCollisionRadius + targetCollionRadius + attackDistanceThreshold/2);
                if (!dead)
                    pathFinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
 