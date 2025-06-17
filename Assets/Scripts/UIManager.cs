using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;

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
    }

    public void ShowWin()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }
    public void ShowGameOver() => ShowPanel(gameOverPanel);
    public void ShowSettings() => ShowPanel(settingsPanel);
    public void HideSettings() => HidePanel(settingsPanel);

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
        LevelManager.Instance.CompleteLevel();
        winPanel.SetActive(true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }

}

