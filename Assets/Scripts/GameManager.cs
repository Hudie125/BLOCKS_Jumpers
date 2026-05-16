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

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 pcCameraOffset = new Vector3(0, 10, -10);
    [SerializeField] private Vector3 mobileCameraOffset = new Vector3(0, 15, -15);
    [SerializeField] private float mobileFieldOfView = 75f;
    [SerializeField] private float pcFieldOfView = 60f;

    // Эталонные соотношения сторон для интерполяции FOV
    private float horizontalAspect = 16f / 9f; // ~1.77 (Широкий монитор)
    private float verticalAspect = 9f / 16f;   // ~0.56 (Вертикальный экран смартфона)

    private float lastWidth;
    private float lastHeight;

    [Header("Score Settings")]
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    private int score = 0;
    private int maxZReached = 0;

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
        AdjustCamera();

        // Запоминаем стартовое разрешение экрана
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    private void NewLevel() {
        gameState = GameState.Ready;
        characterPos = new Vector2Int(0, -1);
        character.position = new Vector3(0, 0.575f, -1);
    }

    private void AdjustCamera()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Рассчитываем текущее соотношение сторон
        float currentAspect = (float)Screen.width / Screen.height;

        // Находим коэффициент «t» между вертикальным и горизонтальным экраном (от 0.0 до 1.0)
        float t = Mathf.InverseLerp(verticalAspect, horizontalAspect, currentAspect);

        // Плавно смешиваем FOV и позицию камеры на основе коэффициента t
        mainCamera.fieldOfView = Mathf.Lerp(mobileFieldOfView, pcFieldOfView, t);
        mainCamera.transform.position = Vector3.Lerp(mobileCameraOffset, pcCameraOffset, t);
    }

    private void UpdateScore()
    {
        int currentProgress = Mathf.Abs(characterPos.y - (-1));

        if (currentProgress > maxZReached)
        {
            score++; 
            maxZReached = currentProgress; 

            if (scoreText != null)
            {
                scoreText.text = "Your score: " + score.ToString();
            }
        }
    }

    void Update()
    {
        // Проверяем, изменился ли размер экрана (для ПК в окне или поворота смартфона)
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            AdjustCamera();
        }

        if (gameState == GameState.Ready)
        {
            Vector2Int moveDirection = Vector2Int.zero;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                moveDirection = new Vector2Int(0, -1);
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                moveDirection = new Vector2Int(0, 1);
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                moveDirection = new Vector2Int(1, 0);
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                moveDirection = new Vector2Int(-1, 0);

            // (Touch & Mouse) ---
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

            if (moveDirection != Vector2Int.zero)
            {
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
                    UpdateScore();
                    StartCoroutine(MoveCharacter());
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

        if (delta.magnitude < minSwipeDistance) return Vector2Int.zero;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x < 0 ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
            }
            else
            {
                return delta.y > 0 ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
            }
    }

    private bool inStartArea(Vector2Int pos) {
        return true; 
    }
}