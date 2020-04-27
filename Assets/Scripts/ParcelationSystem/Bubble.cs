using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, Random.Range(4f, 7f));
        GetComponent<Renderer>().material.renderQueue = 3002;
        transform.GetChild(0).GetComponent<Renderer>().material.renderQueue = 3002;
    }
    
    public void SetText(string txt)
    {
        transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text = txt;
    }
}
