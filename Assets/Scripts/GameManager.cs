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

    // Update
    void Update() {
        if(gameState == GameState.Ready){
            Vector2Int moveDirection = Vector2Int.zero;
            
            if(Keyboard.current.upArrowKey.wasPressedThisFrame){
                character.localRotation = Quaternion.Euler(0, 180, 0);
                moveDirection.y = -1;
            }
            else if(Keyboard.current.downArrowKey.wasPressedThisFrame){
                character.localRotation = Quaternion.Euler(0, 0, 0);
                moveDirection.y = 1;
            }
            else if(Keyboard.current.leftArrowKey.wasPressedThisFrame){
                character.localRotation = Quaternion.Euler(0, 90, 0);
                moveDirection.x = 1;
            }        
            else if(Keyboard.current.rightArrowKey.wasPressedThisFrame){
                character.localRotation = Quaternion.Euler(0, -90, 0);
                moveDirection.x = -1;
            }   

        
            if(Keyboard.current.wKey.wasPressedThisFrame){
                character.localRotation = Quaternion.Euler(0, 180, 0);
                moveDirection.y = -1;
            }
            else if(Keyboard.current.sKey.wasPressedThisFrame){
                character.localRotation = Quaternion.Euler(0, 0, 0);
                moveDirection.y = 1;
            }
            else if(Keyboard.current.aKey.wasPressedThisFrame){
                character.localRotation = Quaternion.Euler(0, 90, 0);
                moveDirection.x = 1;
            }        
            else if(Keyboard.current.dKey.wasPressedThisFrame){
                character.localRotation = Quaternion.Euler(0, -90, 0);
                moveDirection.x = -1;
            }   


            if(moveDirection != Vector2Int.zero){
                Vector2Int destination = characterPos + moveDirection;

                Vector3 targetWorldPos = new Vector3(destination.x, 0.575f, destination.y);

                bool isBlocked = Physics.CheckSphere(targetWorldPos, checkRadius, obstacleLayer);

                if (!isBlocked && inStartArea(destination))
                {
                    characterPos = destination;
                    StartCoroutine(MoveCharacter());
                }
                else {
                    Debug.Log("Путь заблокирован!");
                }
                
            }
        }
    } 

    private IEnumerator MoveCharacter()
    {
        gameState = GameState.Moving;

        float elapsedTime = 0f;
        float yHeight = 0.575f;

        Vector3 startPos = character.position;
        Vector3 endPos = new Vector3(characterPos.x, yHeight, characterPos.y);

        while (elapsedTime < moveDuration) {
            float percent = elapsedTime / moveDuration;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, percent);
            character.position = newPos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        character.position = endPos;

        if (gameState == GameState.Moving) {
            gameState = GameState.Ready;
        }
    } 


    private bool inStartArea(Vector2Int pos) {
        return true; 
    }

} 