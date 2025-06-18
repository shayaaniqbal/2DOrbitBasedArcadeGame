using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    public Button[] levelButtons;



    private void Start()
    {
        LevelSelectionUpdate();
    }

    public void LevelSelectionUpdate()
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
                UIManager.Instance.HideLevelMenu();
                UIManager.Instance.ShowTapToPlay();
                Debug.Log("???????");
            });
        }
    }
}
