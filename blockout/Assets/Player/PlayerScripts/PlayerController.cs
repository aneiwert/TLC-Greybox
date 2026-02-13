using System.Collections; 
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.InputSystem; 
//Alex Neiwert

namespace ANeiwert.FinalCharacterController{
     [DefaultExecutionOrder(-1)]

public class PlayerController : MonoBehaviour
{

    [Header("Components")]
    [SerializeField] private CharacterController _characterController; 
    [SerializeField] private Camera _playerCamera; 

     [Header("Base Movement")]
    public float runAcceleration = 50f; 
    public float runSpeed = 4f; 
    public float drag = 0.1f; 


    [Header("Camera Settings")]
    public float lookSenseH = 0.1f; 
    public float lookSenseV = 0.1f; 
    public float looklimitV = 89f; 
    private PlayerLocomotion _playerLocomotion; 
    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero; 
    private void Awake()
        {
           _playerLocomotion = GetComponent<PlayerLocomotion>(); 
        }

    private void Update()
        {
            //Camera movement 
            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized; 
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
            //Players movement direction    
            Vector3 movementDirection = cameraRightXZ * _playerLocomotion.MovementInput.x + cameraForwardXZ * _playerLocomotion.MovementInput.y; 
      // player Movement 
            Vector3 movementDelta = movementDirection * runAcceleration * Time.deltaTime; 
            Vector3 newVelocity = _characterController.velocity + movementDelta; 
        

        //add drag to player 
        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, runSpeed);
        //Move Character only once per frame 
        _characterController.Move(newVelocity * Time.deltaTime); 
        
        
        }

        private void LateUpdate()
        {
            _cameraRotation.x += lookSenseH * _playerLocomotion.LookInput.x; 
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotion.LookInput.y, -looklimitV, looklimitV); 

            _playerTargetRotation.x += transform.eulerAngles.x + _playerLocomotion.LookInput.x; 
            transform.rotation = Quaternion.Euler(0f, _playerTargetRotation.x, 0f); 
           
           _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f); 


        }


}

}
