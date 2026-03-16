using UnityEngine;

public class Escape : MonoBehaviour
{
    //Angel Rennick
    [SerializeField] private GameObject EscapeScreen;
    public Canvas alarmcode;
    public bool escape = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        EscapeScreen.SetActive(false);
    }
    public void OnTriggerEnter(Collider other)
    {
        EscapeScreen.SetActive(true);
        alarmcode.enabled = false;
        escape = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }
}
