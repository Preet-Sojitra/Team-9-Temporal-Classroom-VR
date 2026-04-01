using UnityEngine;
using System.Collections;

public class HighlightingScript : MonoBehaviour
{
    private Outline outline;

    IEnumerator Start()
    {
        outline = GetComponent<Outline>();

        if (outline != null)
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 2f;
            outline.enabled = false;

            yield return null; 

            outline.enabled = true;
            yield return null; 
            outline.enabled = false;
        }
    }

    public void EnableHighlight()
    {
        if (outline != null)
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 2f;
            outline.enabled = true;
        }
    }

    public void DisableHighlight()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}