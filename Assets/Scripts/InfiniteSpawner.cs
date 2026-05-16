using System.Collections.Generic; 
using UnityEngine;

public class InfiniteSpawner : MonoBehaviour
{
    public Transform player;
    
    [Header("Obstacles Settings")]
    public int minObstacles = 1;        
    public int maxObstacles = 3;        
    public float minX = -5f;            
    public float maxX = 5f;

    [Header("Grass & Road Settings")]
    public int initialCount = 20;
    [Range(0, 1)] public float RoadPercent = 0.5f;

    private int roadWidth = 0; // Для цельных полос 11м ширина в цикле больше не нужна
    private int nextLine = 0;
    private float stepZ = 1.0f;
    private int grassCounter = 0;
    private int roadCounter = 0;

    [Header("Limits")]
    public int maxGrassInRow = 5;
    public int maxRoadInRow = 3;

    // Список для отслеживания объектов препятствий, чтобы вовремя их тушить
    private Queue<GameObject> activeObstacles = new Queue<GameObject>();

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

        // Пока игрок идет вперед — спавним новые линии
        while (player.position.z > (nextLine - initialCount) * stepZ)
        {
            SpawnLine();
            ClearOldObstacles();
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

        // ВМЕСТО ЦИКЛА: Спавним ОДНУ цельную полосу 11м из ПУЛА
        string lineTag = isGrass ? "Grass" : "Road";
        ObjectPooler.Instance.SpawnFromPool(lineTag, new Vector3(0, 0, currentZ), Quaternion.identity);

        // Спавним препятствия, только если это трава
        if (isGrass)
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

            Vector3 spawnPos = new Vector3(chosenX, 0.2f, zPos); 
            
            // Спавним препятствие из пула
            GameObject obstacle = ObjectPooler.Instance.SpawnFromPool("Obstacle", spawnPos, Quaternion.identity);
            
            if (obstacle != null)
            {
                activeObstacles.Enqueue(obstacle);
            }
        }
    }

    // Метод выключает препятствия, которые остались далеко позади игрока
    void ClearOldObstacles()
    {
        // Если препятствие отстало от игрока более чем на 15 метров
        while (activeObstacles.Count > 0 && activeObstacles.Peek().transform.position.z < player.position.z - 15f)
        {
            GameObject oldObstacle = activeObstacles.Dequeue();
            oldObstacle.SetActive(false); // Выключаем, возвращая в пул
        }
    }
}