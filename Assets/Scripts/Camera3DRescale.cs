using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class Camera3DRescale : MonoBehaviour
{

    public Camera camera;
    public Camera _3Dcamera;
    public float distance = 10.0f;
    public GameObject projectionPlane;
    public RenderTexture renderTexture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _3Dcamera.enabled = false;
        projectionPlane.SetActive(true);

        renderTexture.width = Screen.width;
        renderTexture.height = Screen.height;

        float height = 2.0f * Mathf.Tan(0.5f * camera.fieldOfView * Mathf.Deg2Rad) * distance;
        float width = height * Screen.width / Screen.height;

        _3Dcamera.orthographicSize = height / 2;

        Debug.Log("Screen Width: " + Screen.width + " Screen Height: " + Screen.height);

        projectionPlane.transform.localScale = new Vector3(width / 50, 1, height / 50);
        _3Dcamera.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
