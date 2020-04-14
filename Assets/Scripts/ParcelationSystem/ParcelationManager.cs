using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParcelationManager : MonoBehaviour
{
    [SerializeField]
    private GameObject floorPrefab;
    [SerializeField]
    float yOffset = 1.076657f;
    [SerializeField]
    int floorCount = 32;

    //floors generated at runtime which index corresponds to level
    private List<Floor> floors = new List<Floor>();

    // Start is called before the first frame update
    void Awake()
    {
        float yPos = 0;

        for (int i = 0; i < floorCount; i++)
        {
            Debug.Log(yOffset);
            GameObject o = Instantiate(floorPrefab);
            o.transform.SetParent(this.transform);
            o.transform.localPosition = new Vector3(0, yPos, 0);
            floors.Add(o.GetComponent<Floor>());
            yPos += yOffset;
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
