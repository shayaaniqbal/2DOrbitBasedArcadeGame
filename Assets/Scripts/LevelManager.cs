using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public static int selectedLevelIndex = 0;  // Set externally in Scene 0
    public GameObject[] levels;                // Assign 5 level GameObjects in inspector

    private const string LevelKey = "Level_";
    public int currentLevelIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
    }

    public void EnsureFirstLevelUnlocked()
    {
        if (!PlayerPrefs.HasKey(LevelKey + 0))
        {
            PlayerPrefs.SetInt(LevelKey + 0, 1); // Unlock level 0 on first run
            PlayerPrefs.Save();
            Debug.Log("First level unlocked by default.");
        }
    }

    public void LoadSelectedLevel()
    {
        // Disable all levels
        foreach (var level in levels)
        {
            if (level != null) level.SetActive(false);
        }

        // Check if selected level is valid and unlocked
        if (selectedLevelIndex >= 0 && selectedLevelIndex < levels.Length)
        {
            int unlocked = PlayerPrefs.GetInt(LevelKey + selectedLevelIndex, 0);
            if (unlocked == 1)
            {
                levels[selectedLevelIndex].SetActive(true);
                currentLevelIndex = selectedLevelIndex;
                Debug.Log("Loaded Level: " + selectedLevelIndex);
                return;
            }
        }

        // Fallback: show first unlocked level
        for (int i = 0; i < levels.Length; i++)
        {
            if (PlayerPrefs.GetInt(LevelKey + i, 0) == 1)
            {
                levels[i].SetActive(true);
                currentLevelIndex = i;
                selectedLevelIndex = i;
                Debug.Log("Fallback: Loaded first unlocked level " + i);
                return;
            }
        }

        Debug.LogWarning("No unlocked levels found to load.");
    }

    public void CompleteLevel()
    {
        // Save the currently completed level
        PlayerPrefs.SetInt(LevelKey + currentLevelIndex, 1);

        int nextIndex = currentLevelIndex + 1;

        if (nextIndex < levels.Length)
        {
            PlayerPrefs.SetInt(LevelKey + nextIndex, 1); // Unlock the next level
            PlayerPrefs.Save();
            selectedLevelIndex = nextIndex;              // Update selected for next time
            Debug.Log("Level " + currentLevelIndex + " completed. Unlocked Level: " + nextIndex);
        }
        else
        {
            Debug.Log("Last level completed. Returning to main menu or ending game.");
        }
    }

    // OPTIONAL: You can call this from a "Next" button
    public void LoadNextUnlockedLevel()
    {
        int nextIndex = currentLevelIndex + 1;

        if (nextIndex < levels.Length && PlayerPrefs.GetInt(LevelKey + nextIndex, 0) == 1)
        {
            levels[currentLevelIndex].SetActive(false);
            levels[nextIndex].SetActive(true);
            currentLevelIndex = nextIndex;
            selectedLevelIndex = nextIndex;
            Debug.Log("Manually loaded next unlocked level: " + nextIndex);
        }
        else
        {
            Debug.LogWarning("Next level not unlocked or out of range.");
        }
    }

    public void DisableLevels()
    {
        // Disable all levels
        foreach (var level in levels)
        {
            if (level != null) level.SetActive(false);
        }
    }
}
