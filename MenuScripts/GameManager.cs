using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private GameObject gameOverScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hides Game Over Screen
        gameOverScreen.SetActive(false);
        // Hides Cursor
        Cursor.visible = false;
        // Locks Cursor in Place
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(gameOverScreen.activeInHierarchy)
        {   
            // If player is dead and game over screen appears, re-enable cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // Continues to hide cursor as Initialized
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public void TriggerGameOver()
    {
        // Displays Game Over Screen
        gameOverScreen.SetActive(true);
        // Pauses Game
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Resumes the Game
        Time.timeScale = 1f;
        // Reloads the Scene
        SceneManager.LoadScene("SampleScene");
        Debug.Log("I'm Reloading!");
    }

    public void mainMenu()
    {
        // Loads the Menu scene
        SceneManager.LoadScene("Menu");
        Debug.Log("Menu Time!");
    }

    public void quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
