using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameController : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
        // Tekan P → pause toggle
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        // Tekan ESC → balik ke Main Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Main Menu");
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;   // Freeze game
        }
        else
        {
            Time.timeScale = 1f;   // Jalan lagi
        }
    }
}