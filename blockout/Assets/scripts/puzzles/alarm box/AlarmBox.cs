using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AlarmBox : MonoBehaviour, IInteractable
{
    // Can make a specialized interact message for the object
    
    public Text ObjectiveText;
    public Text KeycodeText;
    public string objectInteractMessage;
    private float timeToAppear = 2f;
    private float timeWhenDisappear;
    // Interaction Text
    public GameObject interactionText;
    public GameObject Alarmbox;
    // Specialized message takes priority over default message
    public string InteractMessage => objectInteractMessage;
    public bool Istriggered = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        

    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Interact();
            interactionText.GetComponent<TMP_Text>().text = objectInteractMessage;
            OpenAlarmBox();
        }
        if (ObjectiveText.enabled && KeycodeText.enabled && (Time.time >= timeWhenDisappear))
        {
            ObjectiveText.enabled = false;
            KeycodeText.enabled = false;
        }
    }
    public void Interact()
    {
        ObjectiveText.enabled = true;
        KeycodeText.enabled = true;
        timeWhenDisappear = Time.time + timeToAppear;
    }
    public void OpenAlarmBox()
    {
        if (Istriggered == false)
        {
            //play animation
            Istriggered = true;
        }
        // if alarm box already open, dont play code
        // else code to open alarm box animation
    }
}
