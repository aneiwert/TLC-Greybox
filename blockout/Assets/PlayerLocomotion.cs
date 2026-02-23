using System.Collections; 
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.InputSystem; 


namespace ANeiwert.FinalCharacterController
{

    [DefaultExecutionOrder(-2)]
    public class PlayerLocomotion : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{

    //Crouch Controls 
    public GameObject Camera; 
    public Transform normalHeight, crouchedHeight;


    //Inputs and Controls
    
    public PlayerControls PlayerControls { get; private set;}
    public Vector2 MovementInput { get; private set;}
    public Vector2 LookInput { get; private set;}

    void Start()
        {
           
        }
    void Update()

        {
            CheckMovement(); 
            
        }
    private void OnEnable()
    {
        PlayerControls = new PlayerControls(); 
        PlayerControls.Enable(); 

    PlayerControls.PlayerLocomotionMap.Enable(); 
    PlayerControls.PlayerLocomotionMap.SetCallbacks(this); 
    }

    private void OnDisable()
    {
        PlayerControls.PlayerLocomotionMap.Disable(); 
        PlayerControls.PlayerLocomotionMap.RemoveCallbacks(this); 

    }
    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>(); 
        CheckMovement(); 
        print(MovementInput);
        
    }

    public void OnLook(InputAction.CallbackContext context)
        {
           LookInput = context.ReadValue<Vector2>(); 

        }
 public void CheckMovement()
        {

            //W
            if (Input.GetKeyDown(KeyCode.W))
            {
                 bool success = true; 
                  if(success)PlayAudio.PlaySound(SoundType.Footsteps); 
                 if(success)Debug.Log("Forward!");   
            }
             if (Input.GetKeyUp(KeyCode.W))
            {
                 bool success = true; 
                  if(success)PlayAudio.StopAudio();
                 if(success)Debug.Log("Stop");   
            }

            //A
             if (Input.GetKeyDown(KeyCode.A))
            {
                 bool success = true; 
                  if(success)PlayAudio.PlaySound(SoundType.Footsteps); 
                 if(success)Debug.Log("Left!");   
            }
             if (Input.GetKeyUp(KeyCode.A))
            {
                 bool success = true; 
                  if(success)PlayAudio.StopAudio();
                 if(success)Debug.Log("Stop");   
            }

            //S
           if (Input.GetKeyDown(KeyCode.S))
            {
            
               bool success = true;
               if(success)PlayAudio.PlaySound(SoundType.Footsteps);
                 if(success)Debug.Log("Back!");
            
           }
           if (Input.GetKeyUp(KeyCode.S))
            {
                 bool success = true; 
                  if(success)PlayAudio.StopAudio();
                 if(success)Debug.Log("Stop");   
            }
           
              //D
           if (Input.GetKeyDown(KeyCode.D))
            {
            
               bool success = true;
               if(success)PlayAudio.PlaySound(SoundType.Footsteps);
                 if(success)Debug.Log("Right!");
            
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                 bool success = true; 
                  if(success)PlayAudio.StopAudio();
                 if(success)Debug.Log("Stop");   
            }

            //C
          if(Input.GetKeyDown(KeyCode.C))
            {  
                
               Camera.transform.position = crouchedHeight.transform.position; 
                
            }
        if (Input.GetKeyUp(KeyCode.C))
            {

                Camera.transform.position = normalHeight.transform.position; 
               
            }
        }


}

}




