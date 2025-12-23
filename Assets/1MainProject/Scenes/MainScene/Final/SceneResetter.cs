using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneResetter : MonoBehaviour
{
    public Transform ball;

    public void ResetScene()
    {
        // Kembalikan timescale dulu kalau sebelumnya nge-pause
        Time.timeScale = 1f;

        // Ambil nama scene yang sedang aktif
        string currentScene = SceneManager.GetActiveScene().name;

        // Load ulang scene tersebut
        SceneManager.LoadScene(currentScene);
        ball.position = new Vector3(0,100,8.5f);
    }

    void Update()
{
    if (Input.GetKeyDown(KeyCode.R))
    {
        ResetScene();
    }
}

}