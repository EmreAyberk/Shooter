using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 5;
    private PlayerController _playerController;
    private GunController _gunController;
    private Camera _camera;
    
    public override void Start()
    {
        base.Start();

        _playerController = GetComponent<PlayerController>();
        _gunController = GetComponent<GunController>();
        _camera = Camera.main;
    }

    void Update()
    {
        #region MovementInput

        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        _playerController.Move(moveVelocity);

        #endregion

        #region LookInput

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out var rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin,point,Color.red);
            _playerController.LookAt(point);
        }

        #endregion

        #region WeaponInput

        if (Input.GetMouseButton(0))
            _gunController.Shoot();

        #endregion
    }
}
