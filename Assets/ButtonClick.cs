using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{

    public Image blackScreen;
    public float duration = 0.5f;


    public void PlayClick()
    {
        StartCoroutine(FadeIntoBlack());
    }
    IEnumerator FadeIntoBlack()
    {
        blackScreen.transform.SetAsLastSibling();
        float elapsedTime = 0.0f;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float newAlpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            var tempColor = blackScreen.color;
            tempColor.a = newAlpha;
            blackScreen.color = tempColor;
            yield return null;
        }

        int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        Debug.Log(nextLevelIndex);
        SceneManager.LoadScene(nextLevelIndex);
    }
}
