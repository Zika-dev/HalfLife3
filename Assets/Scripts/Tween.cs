using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Tween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private Button button;
    public Vector2 targetSize = new Vector2(0.2f, 0.2f);
    private Vector2 originalSize;
    public float duration = 0.1f;
    private TMP_Text buttonText;
    void Start()
    {
        button = GetComponent<Button>();
        buttonText = button.transform.Find("Text (TMP)").GetComponent<TMP_Text>(); ;
        originalSize = transform.localScale;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        button.transform.DOScale(targetSize + originalSize, duration);

        buttonText.color = new Color(1, 0.95f, 0, 1);
    
    }
   public void OnPointerExit(PointerEventData eventData)
    {
        button.transform.DOScale(originalSize, duration);
        buttonText.color = new Color(0, 0, 0, 1);
    }
    
}
