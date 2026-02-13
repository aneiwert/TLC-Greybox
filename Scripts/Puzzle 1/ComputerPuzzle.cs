using UnityEngine;
using TMPro;
using UnityEngine.UI;   
using UnityEngine.InputSystem;

public class ComputerPuzzle : MonoBehaviour, IInteractable
{
    // Can make a specialized interact message for the object
    [SerializeField]
    string objectInteractMessage;

    // Interaction Text
    public GameObject interactionText;

    // Specialized message takes priority over default message
    public string InteractMessage => objectInteractMessage; 
    
    // Assigns all the computers for the puzzle
    public GameObject computer1;
    public GameObject computer2;
    public GameObject computer3;
    public GameObject computer4;
    public GameObject computer5;

    public int currentComputer;

    public void Start()
    {
        //assign the first infected computer
        currentComputer = 1;
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
        Interact();
        currentComputer++;
        interactionText.GetComponent<TMP_Text>().text = objectInteractMessage;    
        
        }
    }

    public void Interact()
    {
        switch (currentComputer)
        {
            case 1:
                computer1.GetComponent<MeshRenderer>().material.color = Color.red;
                break;
            case 2:
                computer2.GetComponent<MeshRenderer>().material.color = Color.red;
                computer1.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
            case 3:
                computer3.GetComponent<MeshRenderer>().material.color = Color.red;
                computer2.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
            case 4:
                computer4.GetComponent<MeshRenderer>().material.color = Color.red;
                computer3.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
            case 5:
                computer5.GetComponent<MeshRenderer>().material.color = Color.red;
                computer4.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
        }
    }

}