using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour , IDamageable
{
    public float startingHealth;
    protected float health;
    protected bool dead;

    public event System.Action OnDead;

    public virtual void Start()
    {
        health = startingHealth;
    }

    public void TakeHit(float damage, RaycastHit hit)
    {
        //do sth with hit var
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    private void Die()
    {
        dead = true;
        OnDead?.Invoke();
        GameObject.Destroy(gameObject);
    }
}
