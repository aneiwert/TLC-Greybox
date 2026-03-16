using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AlarmBox : MonoBehaviour, IInteractable
{
    //Angel Rennick
    // Can make a specialized interact message for the object
    public Canvas KeycodeText;
    public string objectInteractMessage;
    public float timeToAppear = 3f;
    public float timeWhenDisappear;
    // Interaction Text
    public GameObject interactionText;

    // Specialized message takes priority over default message
    public string InteractMessage => objectInteractMessage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Interact();
            interactionText.GetComponent<TMP_Text>().text = objectInteractMessage;
        }
        if (KeycodeText.enabled && (Time.time >= timeWhenDisappear))
        {
            KeycodeText.enabled = false;
        }
    }
    public void Interact()
    {
        KeycodeText.enabled = true;
        timeWhenDisappear = Time.time + timeToAppear;
    }
   
}
