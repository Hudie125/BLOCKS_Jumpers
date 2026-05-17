using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;

    void Update()
    {
        // Плавно вращаем монетку вокруг своей оси Y каждый кадр
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что монетку подобрал именно Игрок
        if (other.CompareTag("Player"))
        {
            // Находим GameManager и просим его прибавить монетку
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.AddCoin();
            }

            // Выключаем монетку, возвращая её обратно в пул объектов
            gameObject.SetActive(false);
        }
    }
}