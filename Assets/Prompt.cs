using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interaction : MonoBehaviour
{
    public Image Border;
    public RawImage image;
    public TextMeshProUGUI textLabel;
    public float TypingSpeed = 0.8f;
    public bool CanBeInteractedWith = true;
    public bool Dissapear = false;

    bool debounce = false;
    bool isOpen = false;
    public RawImage rawImage;
    public float scrollSpeed = 1f;
    public Color selectedColor = new Color(0.4f, 0.6f, 1.0f);
    public float swirlIntensity = 0.1f;
    public float swirlSpeed = 2f;
    public float glitchFrequency = 0.1f;

    private Material material;
    private float lastGlitchTime = 0;
    public float originalTimerValue = 0.5f;
    float timer;

    private void Start()
    {
        material = rawImage.material;
        material.SetColor("_Color", selectedColor);
        StartTextInteraction("cyka blyatt", new Vector2(0,0),  0.2f);

    }

    private void Update()
    {
        if (Dissapear && !debounce)
        {
            StartCoroutine(EndInteraction());
            debounce = true;
        }

        float swirlX = Mathf.Sin(Time.time * swirlSpeed + rawImage.uvRect.y * 10) * swirlIntensity;
        float swirlY = Mathf.Cos(Time.time * swirlSpeed + rawImage.uvRect.x * 10) * swirlIntensity;

        if (Time.time - lastGlitchTime > glitchFrequency)
        {
            rawImage.uvRect = new Rect(
                Mathf.Sin(Time.time * 10) * swirlIntensity + swirlX,
                (Time.time * scrollSpeed % 1) + swirlY,
                1, 1
            );
            lastGlitchTime = Time.time;
        }
        else
        {
            rawImage.uvRect = new Rect(
                Mathf.Sin(Time.time * 10) * swirlIntensity + swirlX,
                (Time.time * scrollSpeed % 1) + swirlY,
                1, 1
            );
        }

        material.SetColor("_Color", selectedColor);


    }

    public void StartImageInteraction( Image image, Vector2 size)
    {

    }    
    public void StartTextInteraction(string textInput, Vector2 position, float typeSpeed)
    {
        if (CanBeInteractedWith)
        {
            #region Error Handling
            if (position == null)
            { Debug.LogWarning("Vector2 Position in 'StartTextInteraction' function is NULL. Position is now {0,0}"); position = new Vector2(0, 0); }

            if (typeSpeed <= 0)
            { Debug.LogWarning("Typing Speed is cannot be 0 or lower. Typing Speed is now 0.2"); typeSpeed = TypingSpeed; }
            if(textInput == null || textInput == "")
            {
                Debug.LogWarning("Text Input is NULL or Empty. Text is now 'No Prompt Available'"); textInput = "No Prompt Available";
            }
            #endregion

            CanBeInteractedWith = false;

            StartCoroutine(StartTypeANDExpand(textInput, position, typeSpeed));
        }

    }



    public void ChangeColor(Color newColor)
    {
        selectedColor = newColor;
        material.SetColor("_Color", selectedColor);
    }

    public IEnumerator StartTypeANDExpand(string text, Vector2 pos, float speed)
    {
        
        CanBeInteractedWith = true;
        image.gameObject.SetActive(true);
        Border.gameObject.SetActive(true);

        textLabel.text = "";

        image.rectTransform.sizeDelta = new Vector2(0, 0);


        float expansionDuration = originalTimerValue;
        float elapsedTime = 0f;


        while (elapsedTime < 0.1)
        {
            float heigth = Mathf.Lerp(0, 2.9216f, elapsedTime / 0.1f);
            float width = Mathf.Lerp(0, Border.rectTransform.rect.width, elapsedTime / 0.1f);

            image.rectTransform.sizeDelta = new Vector2(width, heigth);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0f;



        while (elapsedTime < expansionDuration)
        {
            float width = Mathf.Lerp(image.rectTransform.rect.width, Border.rectTransform.rect.width, elapsedTime / expansionDuration);
            float height = Mathf.Lerp(image.rectTransform.rect.height, Border.rectTransform.rect.height, elapsedTime / expansionDuration);
            image.rectTransform.sizeDelta = new Vector2(width, height);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.rectTransform.sizeDelta = new Vector2(Border.rectTransform.rect.width, Border.rectTransform.rect.height);



        // Start Typing
        bool includeArrow = true;
        for (int i = 0; i <= text.Length; i++)
        {
            textLabel.text = includeArrow ? $"> {text.Substring(0, i)}" : $"  {text.Substring(0, i)}"; ;
            includeArrow = !includeArrow;
            yield return new WaitForSeconds(speed);
        }
        while (!Dissapear)
        {
            textLabel.text = includeArrow ? $"> {text}" : $"  {text}";
            includeArrow = !includeArrow;
            yield return new WaitForSeconds(TypingSpeed);
        }
        CanBeInteractedWith = true;

    }


    public IEnumerator EndInteraction()
    {



        textLabel.text = "";



        float expansionDuration = 0.5f;
        float elapsedTime = 0f;


        while (elapsedTime < 0.1)
        {
            float heigth = Mathf.Lerp(image.rectTransform.rect.height, 2.9216f, elapsedTime / 0.1f);

            image.rectTransform.sizeDelta = new Vector2(image.rectTransform.rect.width, heigth);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0f;



        while (elapsedTime < expansionDuration)
        {
            float width = Mathf.Lerp(image.rectTransform.rect.width, 0, elapsedTime / expansionDuration);
            image.rectTransform.sizeDelta = new Vector2(width, image.rectTransform.rect.height);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.rectTransform.sizeDelta = new Vector2(0, 0);





        CanBeInteractedWith = true;

    }


}