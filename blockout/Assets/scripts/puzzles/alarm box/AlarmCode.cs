using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AlarmCode : MonoBehaviour, IInteractable
{
    [SerializeField] InputField inputField;
    [SerializeField] Text resultText;
    public Canvas AlarmCanvas;
    public string objectInteractMessage;
    public GameObject interactionText;
    public string InteractMessage => objectInteractMessage;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            AlarmCanvas.enabled = true;
            Interact();
            interactionText.GetComponent<TMP_Text>().text = objectInteractMessage;
        }
       

    }
    public void Interact()
    {
        string input = inputField.text;
        if (input == "8593")
        {
            resultText.text = "O";
            resultText.color = Color.green;
            // code to open door aniamtion
        }

        else
        {
            resultText.text = "X";
            resultText.color = Color.red;
            // play sound or show that code is wrong
        }

    }

}
