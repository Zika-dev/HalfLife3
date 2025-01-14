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
    private bool CanBeInteractedWith = true;
    public bool Dissapear = false;
    public VerticalLayoutGroup[] verticalComp;
    public ContentSizeFitter[] contentComp;
    private bool debounce = false;
    private bool isOpen = false;
    public RawImage rawImage;
    public float scrollSpeed = 1f;
    public Color selectedColor = new Color(0.4f, 0.6f, 1.0f);
    public float swirlIntensity = 0.1f;
    public float swirlSpeed = 2f;
    public float glitchFrequency = 0.1f;

    private Material material;
    private float lastGlitchTime = 0;
    public float originalTimerValue = 0.5f;

    public float padding = 10f;
    public float scaleSpeed = 0.1f;
    public string textToType = "";
    private RectTransform textRect;

 

    private void Start()
    {
        material = rawImage.material;
        material.SetColor("_Color", selectedColor);
       
        StartInteraction("", new Vector2(0, 0), TypingSpeed);
       
    }

    private void Update()
    {
        if (Dissapear&& !debounce)
        {

            foreach (VerticalLayoutGroup i in verticalComp)
            {
                i.enabled = false;
            }
            foreach (ContentSizeFitter i in contentComp)
            {
                i.enabled = false;
            }

            StartCoroutine(EndInteraction());
            debounce = true;
        }

        
        #region Shader Effects
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

        #endregion
    }


    public void StartInteraction(string textInput, Vector2 position, float typeSpeed)
    {
        if (CanBeInteractedWith)
        {
            
            if (typeSpeed == 0 || typeSpeed == null) typeSpeed = TypingSpeed;
            textInput = textToType;

            if (textInput == null || textInput == "") textInput = "No Text Available";
           

            if (position == null)
           
            { Debug.LogError("Position is null!"); return; }
            foreach (VerticalLayoutGroup i in verticalComp)
            {
                i.enabled = false;
            }
            foreach (ContentSizeFitter i in contentComp)
            {
                i.enabled = false;
            }
            if (isOpen) { StartCoroutine(EndInteraction()); }
            Dissapear = false;
            debounce = false;
            Border.gameObject.SetActive(true);

            CanBeInteractedWith = false;
            StartCoroutine(StartTypeANDExpand(textInput, typeSpeed, position));
        }
        
    }



    public void ChangeColor(Color newColor)
    {
        if (newColor == null) newColor = new Color(0, 0, 0);

        selectedColor = newColor;
        material.SetColor("_Color", selectedColor);
    }

    public IEnumerator StartTypeANDExpand(string text, float typeSpeed, Vector2 pos)
    {

        isOpen = true;
        CanBeInteractedWith = true;
        image.gameObject.SetActive(true);
        Border.gameObject.SetActive(true);
        Border.rectTransform.position = pos;
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

       

        while(elapsedTime < expansionDuration)
        {
            float width = Mathf.Lerp(image.rectTransform.rect.width, Border.rectTransform.rect.width, elapsedTime / expansionDuration);
            float height = Mathf.Lerp(image.rectTransform.rect.height, Border.rectTransform.rect.height, elapsedTime / expansionDuration);
            image.rectTransform.sizeDelta = new Vector2(width, height);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
       
        foreach(VerticalLayoutGroup i in verticalComp)
        {
            i.enabled = true;
        }
        foreach(ContentSizeFitter i in contentComp)
        {
            i.enabled = true;
        }
        image.rectTransform.sizeDelta = new Vector2(Border.rectTransform.rect.width, Border.rectTransform.rect.height);

        textRect = textLabel.rectTransform;

        // Start Typing
        bool includeArrow = true;
        for (int i = 0; i <= text.Length; i++)
        {
           textLabel.text = includeArrow ? $"> {text.Substring(0, i)}" : $"  {text.Substring(0, i)}"; ;
            includeArrow = !includeArrow;

            
            yield return new WaitForSeconds(typeSpeed);
        }
        while (!Dissapear)
        {
            textLabel.text = includeArrow ? $"> {text}" : $"  {text}";
            includeArrow = !includeArrow;
            yield return new WaitForSeconds(0.2f);
        }
      
        
    }


    public IEnumerator EndInteraction()
    {

        isOpen = false;

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

        Border.gameObject.SetActive(false);
        image.rectTransform.sizeDelta = new Vector2(0,0);



        

        CanBeInteractedWith = true;

        StartInteraction("hello amigo", new Vector2(0, 0), 0.05f);

    }


}
