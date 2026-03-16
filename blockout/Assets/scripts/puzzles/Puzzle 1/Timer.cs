using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Timer : MonoBehaviour
{
    // Timer text from the hierarchy
    public TextMeshProUGUI timerText;
    // Set the timer duration here. If the timer is at 0 the player is diasbled.
    [SerializeField] float remainingTime;
    // Put the 1st monitor you will purge here, the first monitor has the timer script
    public GameObject ComputerPuzzle;
    // Player Variable
    public GameObject player;
    // Key Object
    public GameObject Key;
    public GameObject Clue;
    public GameObject PauseMenu;
    public Canvas Keycode;
    public bool Death = false;
    // Vector 3 Variable for Instantiating key in a specific area
    // Int variable that keeps track of the amount of computers the player has purged
    [SerializeField] int currentComputer;

    void Update()
    {
        // to the timer every second
        remainingTime -= Time.deltaTime;
        // Counts the total minutes of the timer
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        // Counts the Seconds of the timer
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        // Put the calculated seconds and minutes into the UI text element which is displayed on screen
        timerText.text = string.Format("{00:00}:{1:00}", minutes, seconds);

        currentComputer = ComputerPuzzle.GetComponent<ComputerPuzzle>().currentComputer;

        // Win and Lose conditions based on Objectives completed AND time remaining
        // First If statement starts the timer
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        // If player has more than 0 seconds left on timer AND has purged 6 viruses, the timer stops and the player lives
        else if (remainingTime > 0 && currentComputer >= 7)
        {
            timerText.text = string.Format("Congrats! You purged what you could, but I am still here...");
        }
        // If the player doesn't succeed, game over screen appears AND player is disabled
        else
        {
            player.SetActive(false);
            Object.FindFirstObjectByType<GameManagerScript>().TriggerGameOver();
        }
        // Conditional Statement if player purges all viruses.
        if (currentComputer >= 7)
        {
            // Vector 3 variable made as a location for the key to spawn in
            Key.SetActive(true);
            Clue.SetActive(true);
        }
        if (remainingTime < 2)
        {
            Death = true;
            PauseMenu.SetActive(false);
            Keycode.enabled = false;
            Debug.Log("2 Seconds Left!");
        }
    }
}
