using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPrefab : MonoBehaviour
{
    public int Index { get; set; }
    public int UnitId { get; set; }
    public float Offset { get; set; }
    public Vector3 Position { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        UnitId = -1;    //-1 belongs means unoccupancy
        Offset = Index;
        Position = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetColor(Color color)
    {
        Material m_Material = this.gameObject.GetComponent<Renderer>().material;
        m_Material.color = color;
    }
}
