using UnityEngine;

public class CamAnim : MonoBehaviour
{
    public GameObject player;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Camera"))
        {
            player.GetComponent<Animator>().Play("CameraShake");
            Debug.Log("Click");
        }

    }
}
