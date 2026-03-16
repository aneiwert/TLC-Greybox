using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AlarmCode : MonoBehaviour
{
    //Angel Rennick
    [SerializeField] InputField inputField;
    [SerializeField] Text resultText;
    public Canvas AlarmCanvas;
    public GameObject Door;
    public bool Canvasbool = false;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && Canvasbool == false )
        {
            AlarmCanvas.enabled = true;
            Canvasbool = true;
            GameObject.FindWithTag("Lid").GetComponent<alarmboxAnim>().AlarmAnim = true;
            InteractKey();
        }
        
        else if (Input.GetKeyDown(KeyCode.O) && Canvasbool == true)
        {
            AlarmCanvas.enabled = false;
            Canvasbool = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public void InteractKey()
    {
        string input = inputField.text;
        if (input == "8593")
        {
            resultText.text = "O";
            resultText.color = Color.green;
            GameObject.FindWithTag("door").GetComponent<DoorAnim>().CorrectCode = true;
            // code to open door aniamtion
        }
        else if (input != "8593" && input != "")
        {
            resultText.text = "X";
            resultText.color = Color.red;
            // play sound or show that code is wrong
        }

    }

}
