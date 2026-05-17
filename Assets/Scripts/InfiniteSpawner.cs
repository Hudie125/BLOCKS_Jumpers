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

    [Header("Limits")]
    public int maxGrassInRow = 5;
    public int maxRoadInRow = 3;

    [Header("Coin Settings")]
    [Range(0f, 1f)][SerializeField] private float coinSpawnChance = 0.25f; // Шанс спавна (0.25 = 25%)

    private int nextLine = 0;
    private float stepZ = 1.0f;
    private int grassCounter = 0;
    private int roadCounter = 0;

    private Queue<GameObject> activeObstacles = new Queue<GameObject>();
    // Очередь для отслеживания монеток, чтобы тушить их позади игрока
    private Queue<GameObject> activeCoins = new Queue<GameObject>();

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
            ClearOldObjects(); // Чистим и препятствия, и монетки
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

        string lineTag = isGrass ? "Grass" : "Road";
        ObjectPooler.Instance.SpawnFromPool(lineTag, new Vector3(0, 0, currentZ), Quaternion.identity);

        // Список доступных клеток по оси X для спавна на этой линии
        List<int> availableX = new List<int>();
        for (int x = (int)minX; x <= (int)maxX; x++) availableX.Add(x);

        if (isGrass)
        {
            // Спавним препятствия (деревья/камни) и убираем эти координаты из доступных
            SpawnObstaclesOnLine(currentZ, availableX);
        }

        // ПОСЛЕ препятствий пробуем заспавнить монетку на ЛЮБОЙ линии в свободном месте
        TrySpawnCoinOnLine(currentZ, availableX);

        nextLine++;
    }

    // Немного изменили метод препятствий, чтобы он принимал список доступных мест
    void SpawnObstaclesOnLine(float zPos, List<int> availableX)
    {
        int countToSpawn = Random.Range(minObstacles, maxObstacles + 1);
        countToSpawn = Mathf.Min(countToSpawn, availableX.Count);

        for (int i = 0; i < countToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availableX.Count);
            int chosenX = availableX[randomIndex];
            availableX.RemoveAt(randomIndex);

            Vector3 spawnPos = new Vector3(chosenX, 0.2f, zPos);
            GameObject obstacle = ObjectPooler.Instance.SpawnFromPool("Obstacle", spawnPos, Quaternion.identity);

            if (obstacle != null) activeObstacles.Enqueue(obstacle);
        }
    }

    // МЕТОД ДЛЯ СПАВНА МОНЕТКИ
    void TrySpawnCoinOnLine(float zPos, List<int> availableX)
    {
        // Если на линии еще остались свободные клетки и выпал шанс спавна
        if (availableX.Count > 0 && Random.value <= coinSpawnChance)
        {
            // Выбираем случайную свободную координату X, где нет дерева или машины при старте
            int randomIndex = Random.Range(0, availableX.Count);
            int chosenX = availableX[randomIndex];

            // Монетка должна висеть чуть выше земли (например, Y = 0.5)
            Vector3 coinPos = new Vector3(chosenX, 0.5f, zPos);
            Quaternion coinRotation = Quaternion.Euler(0, -90, 90);

            GameObject coin = ObjectPooler.Instance.SpawnFromPool("Coin", coinPos, Quaternion.identity);
            if (coin != null)
            {
                activeCoins.Enqueue(coin);
            }
        }
    }

    void ClearOldObjects()
    {
        float clearZThreshold = player.position.z - 15f;

        // Чистим препятствия
        while (activeObstacles.Count > 0 && activeObstacles.Peek().transform.position.z < clearZThreshold)
        {
            activeObstacles.Dequeue().SetActive(false);
        }

        // Чистим несобранные монетки, которые остались позади, чтобы вернуть их в пул
        while (activeCoins.Count > 0 && activeCoins.Peek().transform.position.z < clearZThreshold)
        {
            activeCoins.Dequeue().SetActive(false);
        }
    }
}