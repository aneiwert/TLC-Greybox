using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Timer : MonoBehaviour
{ 
    public TextMeshProUGUI timerText; 
    
    [SerializeField] float remainingTime;

    public GameObject ComputerPuzzle;

    public GameObject player;

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
       timerText.text = string.Format("{00:00}:{1:00}", minutes , seconds);

       currentComputer = ComputerPuzzle.GetComponent<ComputerPuzzle>().currentComputer;

    
       if (remainingTime > 0)
       {
        remainingTime -= Time.deltaTime;
       }
       else if (remainingTime > 0 && currentComputer >= 6)
       {
        timerText.text = string.Format("Congrats! You purged what you could, but I am still here...");
       }
       else
       {
        player.SetActive(false);
        Object.FindFirstObjectByType<GameManagerScript>().TriggerGameOver();
       }
    }
}
