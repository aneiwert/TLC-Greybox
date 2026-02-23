using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class ComputerVirus : MonoBehaviour, IInteractable
{
    public string InteractMessage => objectInteractMessage; 

    public Material Blue;

    public Material Red;
    
    [SerializeField]
    string objectInteractMessage;
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
        Interact();
        }
    }

    public void Interact()
    {
        gameObject.GetComponent<Renderer>().material = Blue;
    }
}
