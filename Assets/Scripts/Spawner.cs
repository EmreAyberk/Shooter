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
   private float nextSpawnTime;
   private void Update()
   {
      
   }

   void NextWave()
   {
      currentWaveNumber++;
      currentWave = waves[currentWaveNumber-1];
   }
   [Serializable] public class Wave
   {
      public int enemyCount;
      public float timeBetweenSpawns;
   }
}
