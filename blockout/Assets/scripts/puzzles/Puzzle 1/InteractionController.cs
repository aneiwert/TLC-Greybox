using UnityEngine;
using TMPro;
using UnityEngine.UI;   
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviour
{
    [SerializeField]
    Camera playerCamera;

    [SerializeField]
    TextMeshProUGUI interactionText;

    [SerializeField]
    float interactionDistance = 5f;

    IInteractable currentTargetedInteractable;

    public void Update ()
    {
        UpdateCurrentInteractable();

        UpdateInteractionText();

        CheckForInteractionInput();
    }

    void UpdateCurrentInteractable()
    {
        // creates the line where the intertaction ray hits the object from the center of the camera
        var ray = playerCamera.ViewportPointToRay(new Vector2(.5f, .5f));
        // Stores object the interact ray hits
        Physics.Raycast(ray, out var hit, interactionDistance);
        // If raycast hits nothing it does nothing, if it does hit something it will interact with object
        currentTargetedInteractable = hit.collider?.GetComponent<IInteractable>();
    }

    public void UpdateInteractionText()
    {
        if (currentTargetedInteractable == null)
        {
            // Displays no text if there is no interactable object
            interactionText.text = string.Empty;
            
            Debug.Log("Die Idiot!"); 

            return;
        }
            // Displays interaction text if there is an interactable object.
            interactionText.text = currentTargetedInteractable.InteractMessage;
    }

    void CheckForInteractionInput()
    {
        // If the player presses E, player interacts with object.
        if (Input.GetKeyDown(KeyCode.X) && currentTargetedInteractable != null)
        {
            currentTargetedInteractable.Interact();
        }
          // If the player presses E, player interacts with object.
        if (Input.GetKeyDown(KeyCode.E) && currentTargetedInteractable != null)
        {
            currentTargetedInteractable.Interact();
        }
    }
}
