using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> roomUnits;
    [SerializeField]
    public List<GameObject> sprites;

    public static Dictionary<int, UnitType> unitType = new Dictionary<int, UnitType>()
        {
            { -1, new UnitType(new Color(0,0,0,0.5f),new Color(.2f,0.2f,.45f), null) },
            { 0, new UnitType(new Color(1,0,0,0.5f),new Color(1,0.15f,0.15f), new int[] { 0 }) },
            { 1, new UnitType(new Color(0,1,0,0.5f),new Color(0,1,0), new int[] { 0, 1 }) },
            { 2, new UnitType(new Color(0,0.5f,1,0.5f),new Color(0,0.5f,1), new int[] { 0, 2, 3 }) },
            { 3, new UnitType(new Color(0.95f,1,0.4f,0.5f),new Color(1,1,0), new int[] { 7, 2, 3 }) },
            { 4, new UnitType(new Color(0.9f,0.15f,1,0.5f),new Color(0.85f,0.12f,1), new int[] { 4, 8 }) },
            { 5, new UnitType(new Color(1,.7f,0,0.5f),new Color(.5f,.2f,0), new int[] { 0, 1, 5, 6, 7 }) },
            { 6, new UnitType(new Color(.4f,.5f,1,0.5f),new Color(.06f,0f,1), new int[] { 4, 5, 6, 7, 8 }) }
        };

    public void RandomizeUnitSprite(BuildingUnit u)
    {
        int j = u.loc[0];
        int unitTypeIndex;
        if (System.Enum.TryParse(u.type, out LivingArrangement lv))
        {
            print(lv.ToString());
            unitTypeIndex = (int)lv;
        }
        else
        {
            if (System.Enum.TryParse(u.type, out SharedSpace ss))
                unitTypeIndex = (int)ss;
            else
                unitTypeIndex = -1;
        }

        //TODO: handle shared spaces
        System.Random r = new System.Random();
        int n = 0;
        int[] sI = unitType[unitTypeIndex].spriteIndices;
        foreach (int i in Enumerable.Range(0, u.loc[1]+1).OrderBy(x => r.Next()))
        {
            int col = i + j;
            if (n < sI.Length)
            {
                //Debug.LogWarning("row "+u.location[0]+" col "+ col + " fetching: " + sI[n].ToString());
                sprites[col].SetActive(true);
                SpriteRenderer s = sprites[col].GetComponent<SpriteRenderer>();

                s.sprite = ParcelationManager.sprites[sI[n]];
                s.color = Color.black;
                s.material.renderQueue = 3002;
                n++;
            }
            else
            {
                //Debug.LogError("TURNING OFF row " + u.location[0] + " col " + col);
                sprites[col].SetActive(false);
            }
        }
    }

    public void SetEmptyState()
    {
        foreach (GameObject o in roomUnits)
        {
            o.SetActive(false);
        }
    }

    public void DeleteUnit(BuildingUnit u)
    {
        int j = u.loc[0];
        for (int x = 0; x <= u.loc[1]; x++)
        {
            roomUnits[j + x].SetActive(false);
        }
    }

    public void AddUnit(BuildingUnit u)
    {
        int j = u.loc[0];
        int unitTypeIndex;

        if (System.Enum.TryParse(u.type, out LivingArrangement lv))
        {
            unitTypeIndex = (int)lv;
        }
        else
        {
            if (System.Enum.TryParse(u.type, out SharedSpace ss))
                unitTypeIndex = (int)ss;
            else
                unitTypeIndex = -1;
        }
        //TODO: handle shared spaces
        for (int x = 0; x <= u.loc[1]; x++)
        {
            int col = j + x;
            roomUnits[col].SetActive(true);
            Renderer r = roomUnits[col].transform.GetChild(1).GetComponent<Renderer>();
            r.material.SetColor("_Color", unitType[unitTypeIndex]._Color);
            r.material.SetColor("_EmissionColor", unitType[unitTypeIndex]._EmissionColor);
            r.material.renderQueue = 3002;
        }
    }

    //To be deprecated- use add delete-edit-add unit instead
    public void SetUnitArray(List<int> roomUnitTypes)
    {
        if (roomUnitTypes.Count != roomUnits.Count)
        {
            Debug.LogError(string.Format("Array count mismatch!: \n{0} != {1}", roomUnitTypes.Count, roomUnits.Count));
            return;
        }
        for (int i = 0; i < roomUnitTypes.Count; i++)
        {
            //roomUnits[0].transform.GetChild(1).GetComponent<Renderer>().material.SetColor("_Color", unitType[roomUnitTypes[i]]._Color);
            Renderer r = roomUnits[i].transform.GetChild(1).GetComponent<Renderer>();
            r.material.SetColor("_Color", unitType[roomUnitTypes[i]]._Color);
            r.material.SetColor("_EmissionColor", unitType[roomUnitTypes[i]]._EmissionColor);
        }
    }
    


    public void SetRoomMode(bool b)
    {
        foreach (GameObject o in roomUnits)
        {
            o.transform.GetChild(0).gameObject.SetActive(b);
            o.transform.GetChild(1).gameObject.SetActive(!b);
            o.transform.GetChild(2).gameObject.SetActive(!b);
        }
    }
}

public class UnitType
{
    public Color _Color { get; set; }
    public Color _EmissionColor { get; set; }
    public int[] spriteIndices;

    public UnitType(Color color, Color emissionColor, int[] spriteIndices)
    {
        _Color = color;
        _EmissionColor = emissionColor;
        this.spriteIndices = spriteIndices;
    }
}