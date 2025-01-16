using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Canvas mainCanvas;
    public Canvas settingsCanvas;
    public Toggle cameraLockToggle;

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

    public void openSettings()
    {
        mainCanvas.gameObject.SetActive(false);
        settingsCanvas.gameObject.SetActive(true);
    }

    public void closeSettings()
    {
        mainCanvas.gameObject.SetActive(true);
        settingsCanvas.gameObject.SetActive(false);
    }

    public void setCameraLock()
    {
        if(cameraLockToggle.isOn)
        {
            SettingsManager.Instance.cameraLock = true;
        }
        else
        {
            SettingsManager.Instance.cameraLock = false;
        }
        print("cameralock: " + SettingsManager.Instance.cameraLock);
    }
}