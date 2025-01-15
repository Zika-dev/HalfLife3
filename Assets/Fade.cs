using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float fadeDuration = 3f;
    public float TimeToWait = 2f;
    public float fadeStrength = 0.001f;

    public GameObject healthbar;
    public GameObject eyes;
    public Sprite eyesSleepy;
    public eyebehavior eyeswitch;

    private SpriteRenderer image;
    void Start()
    {
        image = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeOut());
        StartCoroutine(eyeswitch.changeEyes(eyesSleepy, 3500));
    }

    IEnumerator FadeOut()
    {
        float time = 0.0f;

        while (time < TimeToWait)
        {
            time += Time.deltaTime;

            yield return null;
        }
        float elapsedTime = 0f;
        healthbar.SetActive(true);
        eyes.SetActive(true);

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