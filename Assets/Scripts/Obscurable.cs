using UnityEngine;

public class Obscurable : MonoBehaviour
{
    void Start()
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();
        foreach (Renderer rendr in renders)
        {
            rendr.material.renderQueue = 3002; // set their renderQueue
        }
    }
}
