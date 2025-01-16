using UnityEngine;

public class healthbarExtra : MonoBehaviour
{
    public float healthColorOfset;
    private SpriteRenderer spriteRenderer;
    public DamageBehavior damageBehavior;
    private Color customcolor = new Color(0.3f, 0.87f, 1);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float healthRatio = Mathf.Clamp01(damageBehavior.CurrentHealth / damageBehavior.MaxHealth);

        float r = Mathf.Lerp(1.0f, customcolor.r, healthRatio + healthColorOfset);
        float g = Mathf.Lerp(0.0f, customcolor.g, healthRatio + healthColorOfset);
        float b = Mathf.Lerp(0.0f, customcolor.b, healthRatio + healthColorOfset);

        
        ChangeObjectColor(gameObject, new Color(r, g, b));
    }

    void ChangeObjectColor(GameObject obj, Color newColor)
    {

        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

        spriteRenderer.color = newColor;

    }
}
