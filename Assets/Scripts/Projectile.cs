using System;
using System.Collections;
using System.Collections.Generic;
using Packages.Rider.Editor.Util;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    private float speed = 10;
    private float damage = 1;
    private float lifeTime = 3;
    private float skinWidth = .1f;
    void Start()
    {
        Destroy(gameObject,lifeTime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position,.1f,collisionMask);
        if (initialCollisions.Length > 0)
        { 
            OnHitObject(initialCollisions[0]);  
        }
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    private void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        // Add skinWidth cuz bullet can pass through enemy between frames    
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit)
    {
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage,hit);
        }
        GameObject.Destroy(gameObject);
    }

    void OnHitObject(Collider col)
    {
        IDamageable damageableObject = col.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeDamage(damage);
        }
        GameObject.Destroy(gameObject);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
