using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
   public Transform weaponHolder;
   public Gun startingGun;
   private Gun _equippedGun;

   private void Start()
   {
      if(startingGun!=null)
         EquipGun(startingGun);
   }

   public void EquipGun(Gun gunToEquip)
   {
      if (_equippedGun!=null)
      {
         Destroy(_equippedGun.gameObject);
      }
      _equippedGun = Instantiate(gunToEquip,weaponHolder.position,weaponHolder.rotation) as Gun;
      _equippedGun.transform.parent = weaponHolder;
   }

   public void Shoot()
   {
      if(_equippedGun!=null)
         _equippedGun.Shoot();
   }
}
