/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public float meant to control base player move speed and Interact Mechanic
    public float MoveSpeed = 10f;
    public float MoveSpeed2= 55f; 
    public Transform target;
    public float rotationRate = 15f; 
    // Private floats meant to control variables within script
    private float vInput;
    private float hInput;

    private Vector3 moveInput; 

    private Rigidbody _Rigidbody;

void Awake()
    {
        // To store the component Rigidbody in the property _Rigidbody
        // so that you can use it anywhere in your code.
        TryGetComponent(out _Rigidbody);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        vInput = Input.GetAxis("Vertical") * MoveSpeed * MoveSpeed/2f; 
        hInput = Input.GetAxis("Horizontal") *(MoveSpeed2);
       
        this.transform.Translate(Vector3.forward * vInput * Time.deltaTime); 

        this.transform.Rotate(Vector3.up * hInput * Time.deltaTime); 
 

        if (Input.GetKeyDown("escape")) {
            Cursor.lockState = CursorLockMode.None;
        }
       
    }
    
        
    private void FixedUpdate()
    {
        TryGetComponent(out _Rigidbody);
        // Rigidbody actions are handled by Unity's physics engine, so you should always mess with
        // rigidbody stuff inside FixedUpdate, this will guarantee consistent physics behaviour.
        
        // After this you just simply use your rigidbody position with a += moveInput (like you did) and multiply
        // that Vector3 by a property I called moveSpeed so that you can control how fast your object should move.
        if (_)
        {
            _Rigidbody.position += moveInput;
        } 
        

    moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
     
    }
}
*/