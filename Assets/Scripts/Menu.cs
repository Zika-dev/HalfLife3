using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void playgame()
    {
        int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextLevelIndex);
    }

    public void replaygame()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void quitgame()
    {
        Application.Quit();
    }
}