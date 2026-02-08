using UnityEngine;
using UnityEngine.SceneManagement;
//Alex Neiwert
public class MenuManager : MonoBehaviour
{
    
public void PlayGame()
    {
       SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }




















}
