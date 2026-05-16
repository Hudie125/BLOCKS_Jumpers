using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private float speed;
    private int direction; // 1 — вправо, -1 — влево

    public void Setup(float carSpeed, int carDirection)
    {
        speed = carSpeed;
        direction = carDirection;

        // Поворачиваем модельки так, чтобы они смотрели вдоль дороги (по оси X)
        if (direction == 1)
        {
            // Машина едет направо. 
            // Поворачиваем её носом в сторону увеличения X.
            transform.rotation = Quaternion.Euler(0, 90, 0); 
        }
        else
        {
            // Машина едет налево.
            // Поворачиваем её носом в сторону уменьшения X.
            transform.rotation = Quaternion.Euler(0, -90, 0); 
        }
    }

    void Update()
    {
        // ВМЕСТО Vector3.forward используем Vector3.right (ось X) или корректируем локальное движение.
        // Если твоя моделька изначально (при нулевых поворотах) смотрит носом в бок, 
        // то Translate(Vector3.forward) двигал её боком. 
        // Теперь мы двигаем её вперед ПО ЕЁ СОБСТВЕННОМУ локальному направлению "вперед":
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }
}