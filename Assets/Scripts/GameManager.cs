using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float checkRadius = 0.4f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Game objects")]
    [SerializeField] private Transform character;
    [SerializeField] private GameObject characterVisual;
    [SerializeField] private ParticleSystem deathParticles;

    [Header("UI Effects")]
    [SerializeField] private Image deathVignette;
    [SerializeField] private float fadeDuration = 0.4f;

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

    private float horizontalAspect = 16f / 9f;
    private float verticalAspect = 9f / 16f;

    private float lastWidth;
    private float lastHeight;

    [Header("Score Settings")]
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI bestScoreText; // Сюда перетащи текст для рекорда

    private int score = 0;
    private int bestScore = 0; // Переменная под рекорд
    private int maxZReached = 0;

    [SerializeField] private Animator animator;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private float jumpHeight = 0.5f;

    enum GameState
    {
        Ready,
        Moving,
        Dead
    }
    private GameState gameState;
    private Vector2Int characterPos;

    void Awake()
    {
        NewLevel();
        AdjustCamera();

        lastWidth = Screen.width;
        lastHeight = Screen.height;

        if (deathVignette != null)
        {
            Color c = deathVignette.color;
            c.a = 0f;
            deathVignette.color = c;
        }

        // ЗАГРУЗКА РЕКОРДА: Достаем сохраненный рекорд при старте игры
        // Если игра запущена впервые, вернется 0
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateBestScoreText();
    }

    private void NewLevel()
    {
        gameState = GameState.Ready;
        characterPos = new Vector2Int(0, -1);
        character.position = new Vector3(0, 0.575f, -1);
    }

    private void AdjustCamera()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        float currentAspect = (float)Screen.width / Screen.height;
        float t = Mathf.InverseLerp(verticalAspect, horizontalAspect, currentAspect);
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
            if (scoreText != null) scoreText.text = "Your score: " + score.ToString();

            // ПРОВЕРКА РЕКОРДА: Если текущий счет побил рекорд
            if (score > bestScore)
            {
                bestScore = score;
                UpdateBestScoreText();

                // Сохраняем новое значение рекорда в память устройства
                PlayerPrefs.SetInt("BestScore", bestScore);
                PlayerPrefs.Save(); // Принудительно записываем на диск
            }
        }
    }

    // Метод для обновления текста рекорда на экране
    private void UpdateBestScoreText()
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = "Best score: " + bestScore.ToString();
        }
    }

    public void GameOver()
    {
        if (gameState == GameState.Dead) return;
        gameState = GameState.Dead;

        if (characterVisual != null) characterVisual.SetActive(false);

        if (deathParticles != null)
        {
            deathParticles.gameObject.SetActive(true);
            deathParticles.Play();
        }

        StartCoroutine(RestartLevelCoroutine());
    }

    private IEnumerator RestartLevelCoroutine()
    {
        float elapsedTime = 0f;

        if (deathVignette != null)
        {
            Color vignetteColor = deathVignette.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                vignetteColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
                deathVignette.color = vignetteColor;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
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

            if (Pointer.current != null)
            {
                if (Pointer.current.press.wasPressedThisFrame) touchStartPos = Pointer.current.position.ReadValue();
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
        if (animator != null) animator.SetTrigger("DoJump");

        float elapsedTime = 0f;
        float yHeight = 0.575f;
        Vector3 startPos = character.position;
        Vector3 endPos = new Vector3(characterPos.x, yHeight, characterPos.y);

        while (elapsedTime < moveDuration)
        {
            if (gameState == GameState.Dead) yield break;

            float percent = elapsedTime / moveDuration;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, percent);
            float hill = jumpCurve.Evaluate(percent) * jumpHeight;
            newPos.y += hill;
            character.position = newPos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (gameState != GameState.Dead)
        {
            character.position = endPos;
            gameState = GameState.Ready;
        }
    }

    private Vector2Int GetSwipeDirection()
    {
        Vector2 delta = touchEndPos - touchStartPos;
        if (delta.magnitude < minSwipeDistance) return Vector2Int.zero;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) return delta.x < 0 ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
        else return delta.y > 0 ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
    }

    private bool inStartArea(Vector2Int pos) { return true; }
}