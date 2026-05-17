using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private float speed;
    private int direction; // 1 — вправо, -1 — влево

    public void Setup(float carSpeed, int carDirection)
    {
        speed = carSpeed;
        direction = carDirection;

        // Настройка визуального разворота модели
        if (direction == 1)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    void Update()
    {
        // Жесткое глобальное движение вдоль дороги (по оси X).
        // Машина гарантированно смещается только влево/вправо, игнорируя кривые оси модели.
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);
    }

    // МЕТОД ДЛЯ ФИКСАЦИИ СТОЛКНОВЕНИЯ
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что объект, в который врезалась машина, имеет тег "Player"
        if (other.CompareTag("Player"))
        {
            // Находим на сцене GameManager и вызываем метод завершения игры
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        }
    }
}