using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    public Button[] levelButtons;

    void Start()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i;
            bool unlocked = PlayerPrefs.GetInt("Level_" + index, index == 0 ? 1 : 0) == 1;

            levelButtons[i].interactable = unlocked;
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("SelectedLevelIndex", index);
                PlayerPrefs.Save();
                SceneManager.LoadScene(1); // Load Scene 1 that contains your levels
            });
        }
    }
}
