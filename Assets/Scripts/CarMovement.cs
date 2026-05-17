using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private float speed;
    private int direction;
    private float minX;
    private float maxX;

    public void Setup(float carSpeed, int carDirection, float limitMinX, float limitMaxX)
    {
        speed = carSpeed;
        direction = carDirection;
        minX = limitMinX;
        maxX = limitMaxX;

        // Визуальный разворот
        if (direction == 1)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void Update()
    {
        // Двигаем машину строго по глобальной оси X
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);

        // ЖЕСТКАЯ ПРОВЕРКА ГРАНИЦ:
        // Если ехали направо и выехали за правый край ИЛИ ехали налево и выехали за левый край
        if (direction == 1 && transform.position.x > maxX)
        {
            gameObject.SetActive(false); // Возвращаем в пул
        }
        else if (direction == -1 && transform.position.x < minX)
        {
            gameObject.SetActive(false); // Возвращаем в пул
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        }
    }
}