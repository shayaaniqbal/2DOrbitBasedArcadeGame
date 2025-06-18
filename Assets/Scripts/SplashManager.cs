using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    private AdMobManager adMobManager;
    // Call this function to load Scene 1
    private void Awake()
    {
        adMobManager = FindObjectOfType<AdMobManager>();
    }
    public void LoadNextScene()
    {
        SceneManager.LoadScene(1); // Loads scene with build index 1
    }


    public void ShowIAd()
    {
       adMobManager.ShowInterstitial();
    }

   public void ShowRAd()
    {
        adMobManager.ShowRewarded();
    }
}
