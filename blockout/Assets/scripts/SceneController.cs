//
// Ben Albers
// January 2026
// SceneController.cs
//

using UnityEngine;
using UnityEngine.SceneManagement;

//The SceneController class loads Unity scenes according to the Build Index.
public class SceneController : MonoBehaviour
{
    public int currentSceneIndex;

    // Scenes are stored as a list in Build Profiles
    // File > Build Profiles (or Ctrl + Shift + B)
    //
    // Index Name
    //   0   Splash
    //   1   Title
    //   2   Options
    //   3   Game
    //   4   Loss
    //   5   Win
    //  null Test

    //Start function intializes the current scene according to the build index.
    public void Start()
    {
        currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    }

    //LoadScene function takes the target scene index as an integer, and loads that scene.
    public void LoadScene (int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    //ReloadScene function restarts the current scene.
    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
    }

    //LoadNextScene function loads the next scene in the build index.
    public void LoadNextScene()
    {
        //if there is a next scene
        if (currentSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            //load the next scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex + 1);
        }
        else
        {
            //Debug.Log("No next scene exists.");
        }
    }

    //LoadNextScene function loads the previous scene in the build index.
    public void LoadPreviousScene()
    {
        //if there is a previous scene
        if (currentSceneIndex > 0)
        {
            //load the previous scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex - 1);
        }
        else
        {
            //Debug.Log("No previous scene exists.");
        }
    }

    //Quit function ends the application both inside and outside the Unity editor.
    public void Quit()
    {
        //Check if we're in the editor,
        if (Application.isEditor)
        {
            //Stop playing in the editor.
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            //Close the executable.
            Application.Quit();
        }
    }
}