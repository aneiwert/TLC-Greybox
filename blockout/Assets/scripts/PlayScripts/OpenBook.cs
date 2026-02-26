using UnityEngine;
using TMPro;
using UnityEngine.UI;   
using UnityEngine.InputSystem;
public class OpenBook : MonoBehaviour, IInteractable
{
    
    public string InteractMessage => objectInteractMessage; 

    [SerializeField]
    GameObject spawnPrefab; 


    [SerializeField]
    string objectInteractMessage; 

    void Spawn()
    {
        
    }

    public void Interact()
    {
        Spawn();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
