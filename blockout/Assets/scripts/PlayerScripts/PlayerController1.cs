using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public float meant to control base player move speed and Interact Mechanic
    public float MoveSpeed = 10f;
    public float RotSpeed = 75f; 
    // Private floats meant to control variables within script
    private float vInput;
    private float hInput;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        vInput = Input.GetAxis("Vertical") * MoveSpeed; 
        hInput = Input.GetAxis("Horizontal") * RotSpeed;
       
        this.transform.Translate(Vector3.forward * vInput * Time.deltaTime); 

        this.transform.Rotate(Vector3.up * hInput * Time.deltaTime); 
 

        if (Input.GetKeyDown("escape")) {
            Cursor.lockState = CursorLockMode.None;
        }   
    }
}
