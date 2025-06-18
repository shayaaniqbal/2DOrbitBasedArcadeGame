using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public LevelManager Levels;
    public LevelSelection LevelSelection;

    public static bool GameStarted { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Persist through scenes
    }

    public void StartGame()
    {
        GameStarted = true;
        Debug.Log("Game has started!");

        Levels.EnsureFirstLevelUnlocked();     // Ensure level 0 is unlocked
        Levels.LoadSelectedLevel();            // Then load the selected level
    }

    public void EndGame()
    {

      

        GameStarted = false;

      
    }

    public void ContinueGame()
    {
        Levels.CompleteLevel();
        Levels.LoadNextUnlockedLevel();

    }

   

}
