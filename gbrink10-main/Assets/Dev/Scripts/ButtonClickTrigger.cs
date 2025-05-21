using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonClickTrigger : MonoBehaviour
{
    public Button targetButton; // Reference to the Button component
    public bool triggerButtonClick = false; // Set this to true to trigger the button click
    private bool isClicking = false; // Flag to prevent double clicks

    void Update()
    {
        if (triggerButtonClick && targetButton != null && !isClicking)
        {
            StartCoroutine(HandleButtonClick());
        }
    }

    IEnumerator HandleButtonClick()
    {
        Debug.Log("Button click triggered");
        isClicking = true;

        // Trigger the button's onClick event
        targetButton.onClick.Invoke();

        // Wait for a short duration to prevent double clicks
        yield return new WaitForSeconds(0.5f);

        // Reset the trigger and flag for reuse
        triggerButtonClick = false;
        isClicking = false;
    }

    //ondisable
    private void OnDisable()
    {
        isClicking = false;
        triggerButtonClick = false;
    }
}