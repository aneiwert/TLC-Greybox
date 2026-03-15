using UnityEngine;

public class DoorAnim : MonoBehaviour
{
    public GameObject player;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Door"))
        {
            player.GetComponent<Animator>().Play("DoorOpening");
            Debug.Log("Click");
        }

    }
}
