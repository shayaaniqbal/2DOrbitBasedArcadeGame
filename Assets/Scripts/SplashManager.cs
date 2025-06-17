using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    // Call this function to load Scene 1
    public void LoadNextScene()
    {
        SceneManager.LoadScene(1); // Loads scene with build index 1
    }

   
}
