using System.Collections;
using UnityEngine;

public class AlignToCameraOnce : MonoBehaviour
{
    public float distanceFromCamera = 1.5f;
    public bool faceCamera = true;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); // Wait for the end of the frame to ensure the camera is set up
        Transform cam = Camera.main.transform;

        // מקם את האובייקט מול המצלמה
        Vector3 position = cam.position + cam.forward * distanceFromCamera;
        transform.position = position;

        if (faceCamera)
        {
            // הפוך את האובייקט שיסתכל על המצלמה (רק ב-y)
            Vector3 lookDirection = new Vector3(cam.position.x, transform.position.y, cam.position.z);
            transform.LookAt(lookDirection);
            //flip the object to face the camera
            transform.Rotate(0, 180, 0);
        }
    }
}
