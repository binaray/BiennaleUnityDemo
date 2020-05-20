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
            { 6, new UnitType(new Color(.4f,.5f,1,0.5f),new Color(.06f,0f,1), new int[] { 4, 5, 6, 7, 8 }) },
            { 100, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { }) }
        };

    public void RandomizeUnitSprite(BuildingUnit u)
    {
        int unitTypeIndex;
        if (System.Enum.TryParse(u.type, out LivingArrangement lv))
        {
            //print(lv.ToString());
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


        //Debug.LogError(string.Format("Fetching from [{0},{1}], count: {2}", u.loc[0], u.loc[1], u.loc[1] + 1 - u.loc[0]));
        
        foreach (int i in Enumerable.Range(u.loc[0], u.loc[1]+1-u.loc[0]).OrderBy(x => r.Next()))
        {
            if (n < sI.Length)
            {
                //Debug.LogWarning("row "+u.floor+" col "+ i + " fetching: " + sI[n].ToString());
                sprites[i].SetActive(true);
                SpriteRenderer s = sprites[i].GetComponent<SpriteRenderer>();

                s.sprite = ParcelationManager.sprites[sI[n]];
                s.color = Color.black;
                s.material.renderQueue = 3002;
                n++;
            }
            else
            {
                //Debug.LogError("TURNING OFF row " + u.location[0] + " col " + col);
                sprites[i].SetActive(false);
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
        for (int x = u.loc[0]; x <= u.loc[1]; x++)
        {
            roomUnits[x].SetActive(false);
        }
    }

    public void AddUnit(BuildingUnit u)
    {
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
        if (u.user_id == ParcelationManager.Instance.currentUserId)
        {
            for (int x = u.loc[0]; x <= u.loc[1]; x++)
            {
                roomUnits[x].SetActive(true);
                Renderer r = roomUnits[x].transform.GetChild(1).GetComponent<Renderer>();
                r.material.SetColor("_Color", unitType[100]._Color);
                r.material.SetColor("_EmissionColor", unitType[100]._EmissionColor);
                r.material.renderQueue = 3002;
            }
        }
        else
        {
            for (int x = u.loc[0]; x <= u.loc[1]; x++)
            {
                roomUnits[x].SetActive(true);
                Renderer r = roomUnits[x].transform.GetChild(1).GetComponent<Renderer>();
                r.material.SetColor("_Color", unitType[unitTypeIndex]._Color);
                r.material.SetColor("_EmissionColor", unitType[unitTypeIndex]._EmissionColor);
                r.material.renderQueue = 3002;
            }
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