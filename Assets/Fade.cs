using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float fadeDuration = 3f;
    public float TimeToWait = 2f;
    public float fadeStrength = 0.001f;
    private SpriteRenderer image;
    void Start()
    {
        image = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeOut());
    }

    // Update is called once per frame
    IEnumerator FadeOut()
    {
        float time = 0.0f;

        while (time < TimeToWait)
        {
            time += Time.deltaTime;

            yield return null;
        }
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            var tempColor = image.color;
            tempColor.a = newAlpha;
            image.color = tempColor;

            yield return null;
        }


    }
}