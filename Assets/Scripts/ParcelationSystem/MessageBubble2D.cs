using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageBubble2D : MonoBehaviour
{
    public float x = 2f;
    public Vector3 trackedPos;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, x);
    }

    private void Update()
    {
        this.transform.position = ParcelationManager.Instance.cam.WorldToScreenPoint(trackedPos);
    }
}
