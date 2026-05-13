using UnityEngine;
using System.Collections; 
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float checkRadius = 0.4f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Game objects")]
    [SerializeField] private Transform character;

    [Header("Game parameters")]
    [SerializeField] private float moveDuration = 0.2f;

    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 50f;
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;

    [SerializeField] private Animator animator;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private float jumpHeight = 0.5f;

    enum GameState {
        Ready,
        Moving,
        Dead
    }
    private GameState gameState;
    private Vector2Int characterPos;

    void Awake() {
        NewLevel();
    }

    private void NewLevel() {
        gameState = GameState.Ready;
        characterPos = new Vector2Int(0, -1);
        character.position = new Vector3(0, 0.575f, -1);
    }

    // Z

    // Update
    void Update()
    {
        if (gameState == GameState.Ready)
        {
            Vector2Int moveDirection = Vector2Int.zero;

            // --- ЛОГИКА КЛАВИАТУРЫ ---
            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                moveDirection = new Vector2Int(0, -1);
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                moveDirection = new Vector2Int(0, 1);
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                moveDirection = new Vector2Int(1, 0);
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                moveDirection = new Vector2Int(-1, 0);

            // --- ЛОГИКА СВАЙПОВ (Touch & Mouse) ---
            if (Pointer.current != null)
            {
                if (Pointer.current.press.wasPressedThisFrame)
                {
                    touchStartPos = Pointer.current.position.ReadValue();
                }
                if (Pointer.current.press.wasReleasedThisFrame)
                {
                    touchEndPos = Pointer.current.position.ReadValue();
                    moveDirection = GetSwipeDirection();
                }
            }

            // --- ПРИМЕНЕНИЕ ДВИЖЕНИЯ ---
            if (moveDirection != Vector2Int.zero)
            {
                // Установка ротации персонажа в зависимости от направления
                if (moveDirection.y == -1) character.localRotation = Quaternion.Euler(0, 180, 0);
                else if (moveDirection.y == 1) character.localRotation = Quaternion.Euler(0, 0, 0);
                else if (moveDirection.x == 1) character.localRotation = Quaternion.Euler(0, 90, 0);
                else if (moveDirection.x == -1) character.localRotation = Quaternion.Euler(0, -90, 0);

                Vector2Int destination = characterPos + moveDirection;
                Vector3 targetWorldPos = new Vector3(destination.x, 0.575f, destination.y);
                bool isBlocked = Physics.CheckSphere(targetWorldPos, checkRadius, obstacleLayer);

                if (!isBlocked && inStartArea(destination))
                {
                    characterPos = destination;
                    StartCoroutine(MoveCharacter());
                }
                else
                {
                    Debug.Log("Путь заблокирован!");
                }
            }
        }
    }

    private IEnumerator MoveCharacter()
    {
        gameState = GameState.Moving;

        if (animator != null)
        {
            animator.SetTrigger("DoJump");
        }

        float elapsedTime = 0f;
        float yHeight = 0.575f;

        Vector3 startPos = character.position;
        Vector3 endPos = new Vector3(characterPos.x, yHeight, characterPos.y);

        while (elapsedTime < moveDuration) {
            float percent = elapsedTime / moveDuration;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, percent);
            float hill = jumpCurve.Evaluate(percent) * jumpHeight;
            newPos.y += hill;
            character.position = newPos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        character.position = endPos;

        if (gameState == GameState.Moving) {
            gameState = GameState.Ready;
        }
    }

    private Vector2Int GetSwipeDirection()
    {
        Vector2 delta = touchEndPos - touchStartPos;

        // Проверка на минимальную длину свайпа
        if (delta.magnitude < minSwipeDistance) return Vector2Int.zero;

        // ИСПРАВЛЕНО: Добавлен знак '>' для сравнения абсолютных значений
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Горизонтальный свайп
            return delta.x < 0 ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
        }
        else
        {
            // Вертикальный свайп
            return delta.y > 0 ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
        }
    }

    private bool inStartArea(Vector2Int pos) {
        return true; 
    }

} 