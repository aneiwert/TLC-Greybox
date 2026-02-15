using System.Collections; 
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.InputSystem; 

//Alex Neiwert
public class PlayAudio : MonoBehaviour
{
    public AudioSource audioSource; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         if(Input.GetKeyDown(KeyCode.W))//if WASD is pressed down player makes 
     {
      audioSource.Play(); 
        
    }
      if(Input.GetKeyDown(KeyCode.A))//if WASD is pressed down player makes sound
    {
      audioSource.Play(); 
        
    }
        else
        {
            Debug.Log("Fart"); 
        }
   }
    }



