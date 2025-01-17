using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Interaction : MonoBehaviour, IDragHandler
{

    [Header("Image Related Settings")]
    public Image Border;
    public RawImage image;
    public TextMeshProUGUI textLabel;
    public RawImage rawImage;
    public Texture example;

    [Header("Variables")]
    public float TypingSpeed = 0.8f;
    public bool CanBeInteractedWith = true;
    public bool Refresh = false;
    public bool isImage = false;
    bool debounce = false;
    bool isOpen = false;
    public string TextInp;
    public Texture ImageInp;
    public Vector2 posInp;
    public Vector2 sizeInp;
    public Color selectedColor = new Color(0.4f, 0.6f, 1.0f);
    public float timeUntilDissapear = 5f;

    private Material material;

    public float originalTimerValue = 0.5f;
    public float fontSizeScale = 5f;

    [Header("DEBUG")]

    public bool Drag_Position = false;
    public bool Drag_Size = false;
    public bool GetCurrentPosition = false;
    public bool GetCurrentSize = false;
    public Canvas canvasRectTransform;
    private Vector2 originalSize;
    private Vector2 originalMousePosition;
    public bool Dissapear = false;
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (Drag_Position)
            Border.rectTransform.anchoredPosition += eventData.delta / canvasRectTransform.scaleFactor;


        if (Drag_Size)
        {
            Vector2 currentMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform, eventData.position, eventData.pressEventCamera, out currentMousePosition);

            Vector2 sizeDelta = currentMousePosition - originalMousePosition;
            rawImage.rectTransform.sizeDelta = originalSize + new Vector2(sizeDelta.x, sizeDelta.y);
        }

    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalSize = rawImage.rectTransform.sizeDelta;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform, eventData.position, eventData.pressEventCamera, out originalMousePosition);
    }

    private void Start()
    {
        material = rawImage.material;
        material.SetColor("_Color", selectedColor);
        //StartTextInteraction("cyka blyatt", new Vector2(0,0),  0.2f);
        //StartImageInteraction(example, new Vector2(-51, -62.9f), new Vector2(140, 50));
        EndInteraction();

    }


    private void Update()
    {
        if (Refresh && !debounce)
        {
            StartCoroutine(EndInteraction());
            debounce = true;



        }
      

        float imageWidth = rawImage.rectTransform.rect.width;
        float imageHeight = rawImage.rectTransform.rect.height;

        textLabel.fontSize = Mathf.RoundToInt(Mathf.Min(imageWidth, imageHeight) / fontSizeScale);
        material.SetColor("_Color", selectedColor);
    }



    private void FixedUpdate()
    {
        if (GetCurrentPosition)
        {
            Debug.Log("Current Prompt Position: " + Border.rectTransform.position);
            GetCurrentPosition = false;
        }
        if (GetCurrentSize)
        {
            Debug.Log("Current Prompt Size: " + rawImage.rectTransform.sizeDelta);
            GetCurrentSize = false;
        }
    }
    public void StartImageInteraction(Texture imageTexture, Vector2 position, Vector2 size)
    {
        if (CanBeInteractedWith)
        {
            rawImage.gameObject.SetActive(true);
            Border.gameObject.SetActive(true);
            textLabel.text = "";
            if (imageTexture != null)
            {
                rawImage.texture = imageTexture;
            }
            else
            {
                rawImage.texture = null;
                textLabel.text = "> No Texture Available";
            }


            rawImage.rectTransform.anchoredPosition = position;
            Border.rectTransform.sizeDelta = size;
            rawImage.rectTransform.sizeDelta = size;


            StartCoroutine(AnimateImageAppearance());
        }
    }

    private IEnumerator AnimateImageAppearance()
    {


        float elapsedTime = 0f;
        Vector2 initialSize = new Vector2(0, 0);
        Vector2 targetSize = rawImage.rectTransform.sizeDelta;

        rawImage.rectTransform.sizeDelta = initialSize;

        while (elapsedTime < 0.1)
        {
            float heigth = Mathf.Lerp(0, rawImage.rectTransform.rect.height / 48, elapsedTime / 0.1f);
            float width = Mathf.Lerp(0, Border.rectTransform.rect.width, elapsedTime / 0.1f);

            image.rectTransform.sizeDelta = new Vector2(width, heigth);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        float expansionDuration = originalTimerValue;
        elapsedTime = 0f;



        while (elapsedTime < expansionDuration)
        {
            float width = Mathf.Lerp(image.rectTransform.rect.width, Border.rectTransform.rect.width, elapsedTime / expansionDuration);
            float height = Mathf.Lerp(image.rectTransform.rect.height, Border.rectTransform.rect.height, elapsedTime / expansionDuration);
            image.rectTransform.sizeDelta = new Vector2(width, height);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rawImage.rectTransform.sizeDelta = targetSize;
    }
    public void StartTextInteraction(string textInput, Vector2 position, float typeSpeed, Vector2 size)
    {
        if (CanBeInteractedWith)
        {
            #region Error Handling
            if (position == null)
            { Debug.LogWarning("Vector2 Position in 'StartTextInteraction' function is NULL. Position is now {0,0}"); position = new Vector2(0, 0); }

            if (typeSpeed <= 0)
            { Debug.LogWarning("Typing Speed is cannot be 0 or lower. Typing Speed is now 0.2"); typeSpeed = TypingSpeed; }
            if (textInput == null || textInput == "")
            {
                Debug.LogWarning("Text Input is NULL or Empty. Text is now 'No Prompt Available'"); textInput = "No Prompt Available";
            }
            #endregion

            CanBeInteractedWith = false;
            textLabel.text = "";
            rawImage.texture = null;
            Border.rectTransform.sizeDelta = size;
            rawImage.rectTransform.sizeDelta = size;
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
            float heigth = Mathf.Lerp(0, rawImage.rectTransform.rect.height / 48, elapsedTime / 0.1f);
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



        bool includeArrow = true;
        for (int i = 0; i <= text.Length; i++)
        {
            textLabel.text = includeArrow ? $"> {text.Substring(0, i)}" : $"  {text.Substring(0, i)}"; ;
            includeArrow = !includeArrow;
            yield return new WaitForSeconds(speed);
        }
        while (!Refresh)
        {
            textLabel.text = includeArrow ? $"> {text}" : $"  {text}";
            includeArrow = !includeArrow;
            yield return new WaitForSeconds(0.5f);
        }
        CanBeInteractedWith = true;



        float time = 0f;

        while(time < timeUntilDissapear)
        {
            time += Time.deltaTime;

        }
        EndInteraction();




    }


    public IEnumerator EndInteraction()
    {



        textLabel.text = "";



        float expansionDuration = 0.5f;
        float elapsedTime = 0f;


        while (elapsedTime < 0.1)
        {
            float heigth = Mathf.Lerp(image.rectTransform.rect.height, rawImage.rectTransform.rect.height / 48, elapsedTime / 0.1f);

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
        debounce = false;
        Refresh = false;


        //if (isImage) { StartImageInteraction(ImageInp, posInp, sizeInp); } else { StartTextInteraction(TextInp, posInp,TypingSpeed, sizeInp); }

    }

    void DissapearNOW()
    {
        textLabel.text = "";
        image.rectTransform.sizeDelta = new Vector2(0, 0);
        CanBeInteractedWith = true;
        debounce = false;
        Refresh = false;

    }


}