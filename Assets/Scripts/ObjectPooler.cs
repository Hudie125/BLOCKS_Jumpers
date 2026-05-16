using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject[] prefabs; // Массив префабов для разнообразия
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        InitializePools();
    }

    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                // Выбираем случайный префаб из доступных для этого тега
                GameObject prefab = pool.prefabs[Random.Range(0, pool.prefabs.Length)];
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // Метод для взятия объекта из пула
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Пул с тегом " + tag + " не существует!");
            return null;
        }

        // Берем объект из начала очереди
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // Перемещаем и активируем
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // Возвращаем объект в конец очереди, чтобы использовать его по кругу
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}