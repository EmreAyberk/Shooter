using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Vector3 _velocity;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void Move(Vector3 velocity)
    {
        _velocity = velocity;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + _velocity * Time.fixedDeltaTime); 
    }

    public void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x,transform.position.y,lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }
}
