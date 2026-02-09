//
// Ben Albers
// January 2026
// Timer.cs
//

using System;
using UnityEngine;

//The Timer class counts up or down to a set maximum or minimum at a set speed.
public class Timer : MonoBehaviour
{
    //timer variable counts up or down during Update.
    private float timer = 0.0f;
    //maxTimer variable caps how high the timer can count.
    public float maxTimer = 1.0f;
    //minTimer caps how low the timer can count.
    public float minTimer = 0.0f;
    //countUp boolean: true counts up, false counts down.
    public bool countUp = true;
    //timerResets increments when the timer variable resets, tracking seconds passed by default.
    private int timerResets = 0;
    //timeSpeed determines how fast the timer counts (1.0 = 1x speed, 0.0 = stopped). It is distinct from the global Time.timeScale.
    public float timeSpeed = 1.0f;

    //Awake function happens once immediately.
    private void Awake()
    {
        //log zero seconds passed.
        //Debug.Log("Seconds passed: " + timerResets);
    }

    //Start function sets the timer variable to the min or max.
    private void Start()
    {
        //if we're counting up,
        if (countUp) 
        {
            //timer is set to start at the minimum (default 0).
            timer = minTimer;
        }
        //If we're counting down,
        else
        {
            //timer is set to start at the maximum (default 1).
            timer = maxTimer;
        }
        //We can call Start(); again later to reset the timer.
    }

    //Update function increments the timer variable up or down at chosen timeSpeed.
    private void Update()
    {
        //if we're counting up,
        if (countUp)
        {
            //and we're not yet at the max,
            if (timer < maxTimer)
            {
                //increment timer based on chosen timeSpeed and Time.deltaTime.
                timer += timeSpeed * Time.deltaTime;
            }
            //then if we hit the maximum,
            else
            {
                //increment the number of timerResets,
                timerResets++;
                //log the number of timerResets (default to seconds passed),
                Debug.Log("Seconds passed: " + timerResets);
                //call the Start function, reseting the timer to its minimum.
                Start();
            }
        }
        //if we're counting down,
        else
        {
            //and we're not yet at the min,
            if (timer > minTimer)
            {
                //decrement the timer.
                timer -= timeSpeed * Time.deltaTime;
            }
            //then if we hit the minimum,
            else
            {
                //increment the number of timerResets,
                timerResets++;
                //log the number of timerResets (default to seconds passed),
                Debug.Log("Seconds passed: " + timerResets);
                //call the Start function, reseting the timer to its maximum.
                Start();
            }
        }
    }
}