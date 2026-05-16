using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 7f;

    [Header("Spawn Time Settings")]
    [SerializeField] private float minSpawnDelay = 2f;
    [SerializeField] private float maxSpawnDelay = 4f;

    [Header("Road Limits (X axis)")]
    [SerializeField] private float spawnX = -10f; // Откуда машина начинает ехать
    [SerializeField] private float destroyX = 10f; // Где машина должна исчезнуть

    private float currentRoadSpeed;
    private float currentSpawnDelay;
    private int direction; // 1 = вправо (от minX к maxX), -1 = влево (от maxX к minX)
    
    private Coroutine spawnCoroutine;

    // Этот метод срабатывает КАЖДЫЙ РАЗ, когда дорога берется из пула и активируется
    void OnEnable()
    {
        // 1. Выбираем случайную скорость для ЭТОЙ дороги из диапазона
        currentRoadSpeed = Random.Range(minSpeed, maxSpeed);

        // 2. Выбираем фиксированный интервал времени между машинами для этой дороги
        currentSpawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);

        // 3. Выбираем случайное направление (50% влево, 50% вправо)
        direction = Random.value > 0.5f ? 1 : -1;

        // Запускаем бесконечный спавн машин
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnCarsLoop());
    }

    void OnDisable()
    {
        // Если дорога выключается (уходит назад в пул), останавливаем спавн
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    IEnumerator SpawnCarsLoop()
    {
        // Небольшая случайная задержка перед первой машиной, чтобы они не спавнились одновременно при старте
        yield return new WaitForSeconds(Random.Range(0f, currentSpawnDelay));

        while (true)
        {
            // Определяем точку старта в зависимости от направления
            float startX = (direction == 1) ? spawnX : destroyX;
            Vector3 spawnPosition = new Vector3(startX, 0.2f, transform.position.z);

            // Берем СЛУЧАЙНУЮ машинку из пула (ObjectPooler сам выберет случайный префаб из списка)
            GameObject car = ObjectPooler.Instance.SpawnFromPool("Car", spawnPosition, Quaternion.identity);

            if (car != null)
            {
                // Настраиваем скрипт движения машинки
                CarMovement movement = car.GetComponent<CarMovement>();
                if (movement == null) movement = car.AddComponent<CarMovement>();
                
                movement.Setup(currentRoadSpeed, direction);

                // Запускаем отслеживание, чтобы выключить машинку, когда она уедет за пределы экрана
                StartCoroutine(CheckCarBounds(car));
            }

            // Ждем строго фиксированное время перед следующей машиной
            yield return new WaitForSeconds(currentSpawnDelay);
        }
    }

    IEnumerator CheckCarBounds(GameObject car)
    {
        // Пока машина жива и дорога активна
        while (car != null && car.activeSelf)
        {
            // Если ехала вправо и пересекла правую границу ИЛИ ехала влево и пересекла левую
            if ((direction == 1 && car.transform.position.x > destroyX) || 
                (direction == -1 && car.transform.position.x < spawnX))
            {
                car.SetActive(false); // Выключаем её (возвращаем в пул)
                yield break;
            }
            yield return new WaitForSeconds(0.5f); // Проверяем границы дважды в секунду для оптимизации
        }
    }
}