using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelTagChecker : MonoBehaviour
{
    void Update()
    {
        // Check if the left mouse button was clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the mouse cursor on screen in the direction of the camera
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Cast a ray from the camera to the mouse cursor
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object is this panel
                if (hit.transform == this.transform)
                {
                    // Log the tag of this object
                    Debug.Log("Clicked on panel with tag: " + gameObject.tag);

                    // If this object has a parent, log the parent's tag
                    if (transform.parent != null)
                    {
                        Debug.Log("Parent's tag: " + transform.parent.gameObject.tag);
                    }
                    else
                    {
                        Debug.Log("This panel has no parent.");
                    }
                }
            }
        }
    }
}
