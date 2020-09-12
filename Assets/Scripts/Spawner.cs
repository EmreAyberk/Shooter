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

   private float timeBetweenCampingChecks = 2;
   private float campThresholdDistance = 1.5f;
   private float nextCampCheckTime;
   private Vector3 oldCampPosition;
   private bool isCamping;
   private bool isPlayerDead;

   private LivingEntity playerEntity;
   private Transform playerTransform;
   private MapGenerator map;
   
   public event System.Action<int> OnNewWave;
   
   private void Start()
   {
      map = FindObjectOfType<MapGenerator>();
      playerEntity = FindObjectOfType<Player>();
      playerTransform = playerEntity.transform;

      nextCampCheckTime = timeBetweenCampingChecks + Time.time;
      oldCampPosition = playerTransform.position;
      
      NextWave();
   }

   private void Update()
   {
      if (!isPlayerDead)
      {
         if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
         {
            enemiesRemainingToSpawn--;
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

            StartCoroutine(SpawnEnemy());
         }

         if (Time.time > nextCampCheckTime)
         {
            nextCampCheckTime = Time.time + timeBetweenCampingChecks;

            isCamping = (Vector3.Distance(playerTransform.position, oldCampPosition) < campThresholdDistance);
            oldCampPosition = playerTransform.position;
         }
      }
   }

   IEnumerator SpawnEnemy()
   {
      float spawnDelay = 1;
      float tileFlashSpeed = 4;
      
      Transform spawnTile = map.GetRandomOpenTile();
      if (isCamping)
      {
         spawnTile = map.GetTileFromPosition(playerTransform.position);
      }
      
      Material tileMat = spawnTile.GetComponent<Renderer>().material;
      Color initialColor = tileMat.color;
      Color flashColor = Color.red;
      float spawnTimer = 0;

      while (spawnTimer < spawnDelay)
      {
         tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed,1));
         spawnTimer += Time.deltaTime;
         yield return null;
      }
      Enemy spawnedEnemy = Instantiate(enemy,spawnTile.position + Vector3.up ,Quaternion.identity) as Enemy;
      spawnedEnemy.OnDead += OnEnemyDead;
   }

   void OnPlayerDead()
   {
      isPlayerDead = true;
   }
   void OnEnemyDead()
   {
      enemiesRemainingAlive -= 1;
      if(enemiesRemainingAlive == 0)
         NextWave();
   }
   void ResetPlayerPosition() {
      playerTransform.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up;
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
         if (OnNewWave != null) {
            OnNewWave(currentWaveNumber);
         }
         ResetPlayerPosition();
      }
   }
   [Serializable] public class Wave
   {
      public int enemyCount;
      public float timeBetweenSpawns;
   }
}
