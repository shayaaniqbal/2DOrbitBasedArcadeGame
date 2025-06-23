using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public static int selectedLevelIndex = 0;  // Set externally in Scene 0
    public GameObject[] levelPrefabs;          // ⬅️ New: Assign level prefabs here
    public GameObject[] levels;
    private const string LevelKey = "Level_";
    public int currentLevelIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // ⬇️ Optional: initialize runtime levels array
        levels = new GameObject[levelPrefabs.Length];
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
        selectedLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);

        DisableLevels();

        if (selectedLevelIndex >= 0 && selectedLevelIndex < levelPrefabs.Length)
        {
            int unlocked = PlayerPrefs.GetInt(LevelKey + selectedLevelIndex, 0);
            if (unlocked == 1)
            {
               

                if (levels[selectedLevelIndex] == null)
                    levels[selectedLevelIndex] = Instantiate(levelPrefabs[selectedLevelIndex]);

                levels[selectedLevelIndex].SetActive(true);
                currentLevelIndex = selectedLevelIndex;
                Debug.Log("Loaded Level: " + selectedLevelIndex);
                return;
            }
        }

        for (int i = 0; i < levelPrefabs.Length; i++)
        {
            if (PlayerPrefs.GetInt(LevelKey + i, 0) == 1)
            {
                if (levels[i] == null)
                    levels[i] = Instantiate(levelPrefabs[i]);

                levels[i].SetActive(true);
                currentLevelIndex = i;
                selectedLevelIndex = i;
                Debug.Log("Fallback: Loaded first unlocked level " + i);
                return;
            }
        }
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

        // ✅ If it's past the last level, just reload the scene
        if (nextIndex >= levelPrefabs.Length)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (PlayerPrefs.GetInt(LevelKey + nextIndex, 0) == 1)
        {
            if (levels[currentLevelIndex] != null)
                levels[currentLevelIndex].SetActive(false);

            if (Level.Instance != null)
            {
                Destroy(Level.Instance.gameObject);
                Level.Instance = null;
            }

            if (levels[nextIndex] == null)
                levels[nextIndex] = Instantiate(levelPrefabs[nextIndex]);

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

    public void PlayCurrentLevel()
    {

        SoundManager.Instance.PlayButtonClickSFX();
        // Destroy ALL previously loaded levels (not just current one)
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                Destroy(levels[i]);
                levels[i] = null;
            }
        }

        // Now load the new level
        selectedLevelIndex = currentLevelIndex;

        if (selectedLevelIndex >= 0 && selectedLevelIndex < levelPrefabs.Length)
        {
            levels[selectedLevelIndex] = Instantiate(levelPrefabs[selectedLevelIndex]);
            levels[selectedLevelIndex].SetActive(true);
            currentLevelIndex = selectedLevelIndex;
            Debug.Log("Playing level at index: " + selectedLevelIndex);
        }
        else
        {
            Debug.LogWarning("Selected level index out of bounds: " + selectedLevelIndex);
        }
    }



    public void DisableLevels()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
                levels[i].SetActive(false);
        }
    }
}
