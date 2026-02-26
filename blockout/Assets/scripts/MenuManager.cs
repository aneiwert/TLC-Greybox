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


    public void PauseMenu()
    {
       pauseMenu.SetActive(true);  
       

       Debug.Log("Pause"); 
    }
public void PlayGame()
    {
       SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Options()
    {
        
    }


public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }




















}
