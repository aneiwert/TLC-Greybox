using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class AlarmInteract : MonoBehaviour, IInteractable
    //Angel Rennick
{
    [SerializeField]
    string objectInteractMessage;
    public GameObject interactionText;
    public bool istriggered = false;
    public string InteractMessage => objectInteractMessage;
    // Update is called once per frame
    public void Update()
    {
        if (istriggered == false)
        {
            Interact();
            istriggered = false;
        }
        
    }
    public void Interact()
    {
        istriggered = true;
    }

}
