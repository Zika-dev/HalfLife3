using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour
{
    private Vector3 startPos;
    public Doors doorScript;
    private bool isPressed = false;
    public float pressDistance = 0.5f;
    public float returnDelay = 1f;
    public float speed = 1f;

    private void Start()
    {
        startPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPressed)
        {
            isPressed = true;
            StartCoroutine(PressButton());
            ToggleDoor();
        }
    }

    private IEnumerator PressButton()
    {
        // Move the button
        Vector3 pressedPos = startPos + Vector3.right * pressDistance;
        yield return StartCoroutine(MoveButton(pressedPos, speed));

        // Wait for the delay
        yield return new WaitForSeconds(returnDelay);

        // Return the button
        yield return StartCoroutine(MoveButton(startPos, speed));

        isPressed = false;
    }

    private IEnumerator MoveButton(Vector3 targetPos, float duration)
    {
        float time = 0;
        Vector3 startingPos = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startingPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    private void ToggleDoor()
    {
        doorScript.doorOpen = !doorScript.doorOpen;
    }
}
