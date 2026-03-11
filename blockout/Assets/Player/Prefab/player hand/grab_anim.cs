using UnityEngine;

public class grab_anim : MonoBehaviour
{
    public GameObject Player;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            Player.GetComponent<Animator>().Play("GrabAnim");
    }
}
