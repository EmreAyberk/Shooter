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
        health -= damage;
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    private void Die()
    {
        dead = true;
        OnDead?.Invoke();
        GameObject.Destroy(gameObject);
    }
}
