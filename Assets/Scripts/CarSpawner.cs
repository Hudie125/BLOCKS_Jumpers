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
    public float spawnX = -12f;
    public float destroyX = 12f;

    private float currentRoadSpeed;
    private float currentSpawnDelay;
    private int direction;

    private Coroutine spawnCoroutine;

    void OnEnable()
    {
        currentRoadSpeed = Random.Range(minSpeed, maxSpeed);
        currentSpawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
        direction = Random.value > 0.5f ? 1 : -1;

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnCarsLoop());
    }

    void OnDisable()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }

    IEnumerator SpawnCarsLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, currentSpawnDelay));

        while (true)
        {
            // Если едем направо (1), спавнимся слева (spawnX). Если налево (-1), спавнимся справа (destroyX)
            float startX = (direction == 1) ? spawnX : destroyX;
            Vector3 spawnPosition = new Vector3(startX, 0.3f, transform.position.z);

            GameObject car = ObjectPooler.Instance.SpawnFromPool("Car", spawnPosition, Quaternion.identity);

            if (car != null)
            {
                CarMovement movement = car.GetComponent<CarMovement>();
                if (movement == null) movement = car.AddComponent<CarMovement>();

                // Передаем машине её скорость, направление и границы именно ЭТОЙ дороги
                movement.Setup(currentRoadSpeed, direction, spawnX, destroyX);
            }

            yield return new WaitForSeconds(currentSpawnDelay);
        }
    }
}