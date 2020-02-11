using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPrefab : MonoBehaviour
{
    public int Index { get; set; }
    public int UnitId { get; set; }
    public int Section { get; set; } //section index; 0:left, 1:mid, 2:right
    public Vector3 Position { get; private set; }

    private void Awake()
    {
        //null values before assignment on start
        UnitId = -1;    //-1 belongs means unoccupancy
    }

    // Start is called before the first frame update
    void Start()
    {
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
