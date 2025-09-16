using UnityEngine;

public enum GameState { MAIN = 0, WATING, FIGHT, RESULT }

public class GameManager : MonoBehaviour
{
    public static GameManager Inst;

    private GameState gameState;
    public GameState GetGameState() => gameState;
    public void SetGameState(GameState stats) => gameState = stats;

    int targetStage;

    public void SetTargetStage(int stage) => targetStage = stage;
    public int GetTargetStage() => targetStage;

    void Awake()
    {
        Inst = this;
    }

}