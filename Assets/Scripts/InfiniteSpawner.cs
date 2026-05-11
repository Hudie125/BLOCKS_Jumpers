using System.Collections.Generic; 
using UnityEngine;

public class InfiniteSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject[] grassPrefabs;
    public GameObject[] roadPrefabs;
    
    [Header("Obstacles Settings")]
    public GameObject[] obstaclePrefabs; 
    public int minObstacles = 1;        
    public int maxObstacles = 3;        
    public float minX = -5f;            
    public float maxX = 5f;

    [Header("Grass & Road Settings")]
    public int initialCount = 20;
    [Range(0, 1)] public float RoadPercent = 0.5f;

    private int roadWidth = 0; 
    private int nextLine = 0;
    private float stepZ = 1.0f;
    private int grassCounter = 0;
    private int roadCounter = 0;

    [Header("Limits")]
    public int maxGrassInRow = 5;
    public int maxRoadInRow = 3;

    void Start()
    {
        transform.position = Vector3.zero;
        nextLine = 0;

        for (int i = 0; i < initialCount; i++)
        {
            SpawnLine();
        }
    }

    void Update()
    {
        if (player == null) return;
        while (player.position.z > (nextLine - initialCount) * stepZ)
        {
            SpawnLine();
        }
    }

    void SpawnLine()
    {
        bool isGrass;
        if (grassCounter >= maxGrassInRow) isGrass = false;
        else if (roadCounter >= maxRoadInRow) isGrass = true;
        else isGrass = Random.value > RoadPercent;

        if (isGrass) { grassCounter++; roadCounter = 0; }
        else { roadCounter++; grassCounter = 0; }

        float currentZ = nextLine * stepZ;


        for (int x = -roadWidth; x <= roadWidth; x++)
        {
            GameObject[] selection = isGrass ? grassPrefabs : roadPrefabs;
            GameObject prefab = selection[Random.Range(0, selection.Length)];
            if (prefab != null)
            {
                Instantiate(prefab, new Vector3(x, 0, currentZ), Quaternion.identity, transform);
            }
        }

        if (isGrass && obstaclePrefabs != null && obstaclePrefabs.Length > 0)
        {
            SpawnObstaclesOnLine(currentZ);
        }

        nextLine++;
    }

    void SpawnObstaclesOnLine(float zPos)
    {

        List<int> availableX = new List<int>();
        for (int x = (int)minX; x <= (int)maxX; x++)
        {
            availableX.Add(x);
        }


        int countToSpawn = Random.Range(minObstacles, maxObstacles + 1);
        countToSpawn = Mathf.Min(countToSpawn, availableX.Count);


        for (int i = 0; i < countToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availableX.Count);
            int chosenX = availableX[randomIndex];
            availableX.RemoveAt(randomIndex); 
            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            

            Vector3 spawnPos = new Vector3(chosenX, 0.2f, zPos); 
            Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, transform);
        }
    }
}