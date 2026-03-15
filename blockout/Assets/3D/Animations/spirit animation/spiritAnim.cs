using UnityEngine;

public class spiritAnim : MonoBehaviour
{
    public GameObject Player;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Spirit"))
        {
            Player.GetComponent<Animator>().Play("Take 001");
            Debug.Log("Click");
        }

    }
}
