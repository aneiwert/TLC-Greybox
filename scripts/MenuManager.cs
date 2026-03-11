using System.Collections; 
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
//Alex Neiwert
public class MenuManager : MonoBehaviour
{

    public GameObject pauseMenu; 
    
    void Update()
    {
           if (Input.GetKeyDown(KeyCode.Escape))
            {
                  PauseMenu();    
            }
    }

    // Checks to see if Pause menu is true or not, if it is true Escape quits out of menu.
    public void PauseMenu()
    {
       pauseMenu.SetActive(!pauseMenu.activeSelf);  
       

       Debug.Log("Pause"); 
    }
public void PlayGame()
    {
       SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Options()
    {
        
    }

// Quits out of Application, NOT the editor
public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }




















}
