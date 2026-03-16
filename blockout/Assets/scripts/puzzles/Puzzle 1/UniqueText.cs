// Valerie Micohn
using UnityEngine;
using TMPro;
using UnityEngine.UI;   
using UnityEngine.InputSystem;

public class UniqueText : MonoBehaviour, IInteractable
{
    // Can make a specialized interact message for the object
    [SerializeField]
    string objectInteractMessage;

    // Interaction Text
    public GameObject interactionText;

    // Player Variable
    public GameObject player;

    // Specialized message takes priority over default message
    public string InteractMessage => objectInteractMessage;

    //Update is called once per frame
    public void Update()
    {
       if(Input.GetKeyDown(KeyCode.X))
        {
            Interact();
        }
    }

    public void Interact()
    {
        interactionText.GetComponent<TMP_Text>().text = objectInteractMessage; 
    }
}
