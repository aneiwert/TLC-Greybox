using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public float meant to control base player move speed and Interact Mechanic
    public float speed = 10f;
    public bool Interact = false;
    // Private floats meant to control variables within script
    private float translation;
    private float straffe;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        translation = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        straffe = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        transform.Translate(straffe, 0, translation);

        if (Input.GetKeyDown("escape")) {
            Cursor.lockState = CursorLockMode.None;
        }  

        // interact key
        if (Input.GetKeyDown(KeyCode.E))
        {   // Makes it so the E key interacts with objects and prints Interact to console for testing purposes
            Interact = true;
            Debug.Log("Interact");
        }
        else 
        {   // Makes the interact button a one time occurance to prevent interact being active 24/7
            if (Interact == true)
            {
                Interact = false;
            }
        } 
    }
}
