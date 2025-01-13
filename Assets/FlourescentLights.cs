using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FloureecentLights : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private Light light3D;

    [Header("Config")]
    [SerializeField] private bool lightOn;
    [SerializeField] private int flickers = 5;
    [SerializeField] private float flickerSpeedMin = 0.1f;
    [SerializeField] private float flickerSpeedMax = 0.3f;

    public void LightsOff()
    {
        if (lightOn)
        {
            lightOn = false;
            light2D.gameObject.SetActive(false);
            light3D.gameObject.SetActive(false);
        }
        else return;
    }

    public void LightsOn()
    {
        if (!lightOn)
        {
            lightOn = true;
            StartCoroutine(FlickerLights(true));
        }
        else return;
    }


    IEnumerator FlickerLights(bool state)
    {
        for (int i = 0; i != flickers; i++)
        {
            light2D.gameObject.SetActive(!state);
            light3D.gameObject.SetActive(!state);
            yield return new WaitForSeconds(Random.Range(flickerSpeedMin, flickerSpeedMax));
            light2D.gameObject.SetActive(state);
            light3D.gameObject.SetActive(state);
            yield return new WaitForSeconds(Random.Range(flickerSpeedMin, flickerSpeedMax)+0.03f*flickers);
        }
    }

    private void Update()
    {
        if (!lightOn)
        {
            LightsOn();
        }
    }
}
