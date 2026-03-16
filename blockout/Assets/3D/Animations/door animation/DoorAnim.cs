using UnityEngine;

public class DoorAnim : MonoBehaviour
{
    //Angel Rennick
    public GameObject player;
    public bool CorrectCode = false;
    public bool istriggered = false;
    // Update is called once per frame
    void Update()
    {
        if (CorrectCode == true && istriggered == false)
        {
            player.GetComponent<Animator>().Play("DoorOpening");
            Debug.Log("Click");
            istriggered = true;
        }

    }
}
