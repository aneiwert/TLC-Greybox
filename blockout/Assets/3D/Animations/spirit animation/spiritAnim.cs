using UnityEngine;

public class spiritAnim : MonoBehaviour
{
    public GameObject Player;
    public bool SpiritAnima = false;
    public bool istriggered = false;
    // Update is called once per frame
    void Update()
    {
        if (SpiritAnima == true && istriggered == false)
        {
            Player.GetComponent<Animator>().Play("Take 001");
            istriggered = true;
            Debug.Log("Click");
        }

    }
}
