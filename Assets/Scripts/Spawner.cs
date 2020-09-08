using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
   public Enemy enemy;
   public Wave[] waves;

   private Wave currentWave;
   private int currentWaveNumber;
   
   private int enemiesRemainingToSpawn;
   private int enemiesRemainingAlive;
   private float nextSpawnTime;

   private void Start()
   {
      NextWave();
   }

   private void Update()
   {
      if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
      {
         enemiesRemainingToSpawn--;
         nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
         
         Enemy spawnedEnemy = Instantiate(enemy,Vector3.zero,Quaternion.identity) as Enemy;
         spawnedEnemy.OnDead += OnEnemyDead;
      }
   }

   void OnEnemyDead()
   {
      enemiesRemainingAlive -= 1;
      if(enemiesRemainingAlive == 0)
         NextWave();
   }
   void NextWave()
   {
      currentWaveNumber++;
      print("Wave:" + currentWaveNumber);
      if (currentWaveNumber - 1 < waves.Length)
      {
         currentWave = waves[currentWaveNumber-1];

         enemiesRemainingToSpawn = currentWave.enemyCount;
         enemiesRemainingAlive = enemiesRemainingToSpawn;
      }
   }
   [Serializable] public class Wave
   {
      public int enemyCount;
      public float timeBetweenSpawns;
   }
}
