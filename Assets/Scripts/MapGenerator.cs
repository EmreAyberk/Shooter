using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrafab;
    public Transform navMeshMaskPrefab;
    public Transform navMeshFloor;
    public Vector2 maxMapSize;

    [Range(0, 1)] public float outlinePercent;
    public float tileSize;

    private List<Coord> allTileCoords;
    private Queue<Coord> shuffledTileCoords;
    private Queue<Coord> shuffledOpenTileCoords;
    private Transform[,] tileMap;
    private Map currentMap;

    void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
        
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x,currentMap.mapSize.y];
        Random randNumGen = new Random(currentMap.seed);

        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize,.5f, currentMap.mapSize.y * tileSize);
        
        //GenerateCoords
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        //Create Map Holder
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //Spawning Tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }

        //Spawning Obstacles
        bool[,] obstacleMap = new bool[(int) currentMap.mapSize.x, (int) currentMap.mapSize.y];

        int obstacleCount = Convert.ToInt32(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currectObstacleCount = 0;
        
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords); 
        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currectObstacleCount++;
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currectObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight,
                    (float) randNumGen.NextDouble());

                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrafab, obstaclePosition + Vector3.up * obstacleHeight / 2,
                    Quaternion.identity);
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight,
                    (1 - outlinePercent) * tileSize);

                newObstacle.parent = mapHolder;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color =
                    Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
                
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currectObstacleCount--;
            }
        }
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        
        //Creating NavMeshMasks
        Transform maskLeft = Instantiate(navMeshMaskPrefab,
            Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize,
            Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale =
            new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navMeshMaskPrefab,
            Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize,
            Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale =
            new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navMeshMaskPrefab,
            Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize,
            Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskDown = Instantiate(navMeshMaskPrefab,
            Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize,
            Quaternion.identity);
        maskDown.parent = mapHolder;
        maskDown.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currectObstacleCount)
    {
        bool[,] mapFlag = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlag[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) &&
                            neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1) &&
                            !mapFlag[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                        {
                            mapFlag[neighbourX, neighbourY] = true;
                            queue.Enqueue(new Coord(neighbourX, neighbourY));
                            accessibleTileCount++;
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount =
            Convert.ToInt32(currentMap.mapSize.x * currentMap.mapSize.y - currectObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + .5f + x, 0, -currentMap.mapSize.y / 2f + .5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp (x, 0, tileMap.GetLength (0) -1);
        y = Mathf.Clamp (y, 0, tileMap.GetLength (1) -1);
        return tileMap [x, y];
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x,randomCoord.y];
    }

    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Vector2 mapSizeV2;
        public Coord mapSize => new Coord((int) mapSizeV2.x, (int) mapSizeV2.y);
        [Range(0, 1)] public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter => new Coord(mapSize.x / 2, mapSize.y / 2);
    }
}