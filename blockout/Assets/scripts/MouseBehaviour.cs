using UnityEngine;
//Alex Neiwert 
public class MouseBehaviour : MonoBehaviour
{



    void Start()
    {
        
    }


    void Update()
    {
        //Get the mouse position in screen space
        Vector3 mousePosition = Input.mousePosition; 

        //Move mouse away from camera

        mousePosition.z = 10; 
        //Conversion
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);


        if (!MenuManager.gameIsPaused)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                //Player Foward
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
              //No going forward  
            }
            
        }
    }
}
