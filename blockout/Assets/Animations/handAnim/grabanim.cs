using UnityEngine;

public class grabanim : MonoBehaviour
{
    public GameObject Player;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Grab"))
        {
            Player.GetComponent<Animator>().Play("Take 001");
            Debug.Log("Click");
        }

    }
}

