using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public LevelManager Levels;
    public LevelSelection LevelSelection;
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject tapToPlayPanel;
    [SerializeField] private GameObject levelMenuPanel;


    [Header("UI Panels")]
    public Text LevelNumber;
    public Text NextLevel;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        HideAllPanels();
        ShowLevelMenu();
    }

   

    public void ShowWin()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }
    public void ShowGameOver() => ShowPanel(gameOverPanel);
    public void ShowSettings() => ShowPanel(settingsPanel);
    public void HideSettings() => HidePanel(settingsPanel);
    public void ShowTapToPlay() => ShowPanel(tapToPlayPanel);
    public void HideTapToPlay() => HidePanel(tapToPlayPanel);
    public void ShowLevelMenu() => ShowPanel(levelMenuPanel);
    public void HideLevelMenu() => HidePanel(levelMenuPanel);

    public void ShowWinPanel() => ShowPanel(winPanel);
    public void HideWinPanel() => HidePanel(winPanel);

    public void HideGameOverPanel() => HidePanel(gameOverPanel);
    private void ShowPanel(GameObject panel)
    {
        HideAllPanels();
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0;
    }

    private void HidePanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(false);
        if (!AnyPanelActive()) Time.timeScale = 1;
    }

    private void HideAllPanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = 1;
    }

    private bool AnyPanelActive()
    {
        return (winPanel != null && winPanel.activeSelf) ||
               (gameOverPanel != null && gameOverPanel.activeSelf) ||
               (settingsPanel != null && settingsPanel.activeSelf);
    }


    public void RetryLevel()
    {
        Time.timeScale = 1f; // Ensure time resumes before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }


    IEnumerator LoadSceneAfterDelay()
    {
       
        Levels.CompleteLevel();
        LevelSelection.LevelSelectionUpdate();
        ShowWinPanel();
        LevelNumberUpdate();
        yield return null;
        
    }



    public void Homebutton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void LevelNumberUpdate()
    {
        int displayLevel = LevelManager.selectedLevelIndex ;
        LevelNumber.text =  displayLevel.ToString();

        int Level = LevelManager.selectedLevelIndex + 1 ;
        NextLevel.text =  Level.ToString();

    }
}

