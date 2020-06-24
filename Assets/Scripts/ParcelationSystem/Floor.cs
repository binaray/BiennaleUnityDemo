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
            { 100, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 100 }) },
            { 101, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 101 }) },
            { 102, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 102 }) },
            { 103, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 103 }) },
            { 104, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 104 }) },
            { 105, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 105 }) },
            { 106, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 106 }) },
            { 107, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 107 }) },
            { 108, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 108 }) },
            { 109, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 109 }) },
            { 110, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 110 }) },
            { 111, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 111 }) },
            { 112, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 112 }) },
            { 113, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 113 }) },
            { 114, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 114 }) },
            { 115, new UnitType(new Color(1f,.1f,1,0.5f),new Color(1f,1f,1), new int[] { 115 }) }
        };

    public void RandomizeUnitSprite(BuildingUnit u)
    {
        int unitTypeIndex;
        System.Random r = new System.Random();
        if (System.Enum.TryParse(u.type, out LivingArrangement lv))
        {
            //print(lv.ToString());
            unitTypeIndex = (int)lv;
            int n = 0;
            int[] sI = unitType[unitTypeIndex].spriteIndices;

            //Debug.LogError(string.Format("Fetching from [{0},{1}], count: {2}", u.loc[0], u.loc[1], u.loc[1] + 1 - u.loc[0]));
            int study = u.user_input.requiredRooms[RequiredRooms.Study.ToString()];
            foreach (int i in Enumerable.Range(u.loc[0], u.loc[1] + 1 - u.loc[0]).OrderBy(x => r.Next()))
            {
                //output study on facade
                if (study > 0)
                {
                    if ((i > 1 && i < 4) || (i > 6 && i < 9) || (i > 11 && i < 14)) { }
                    else
                    {
                        sprites[i].SetActive(true);
                        SpriteRenderer s = sprites[i].GetComponent<SpriteRenderer>();

                        s.sprite = ParcelationManager.sprites[9];
                        s.color = Color.black;
                        s.material.renderQueue = 3002;
                        study--;
                        continue;
                    }
                }
                //output cohabitation otherwise
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
        else
        {
            if (System.Enum.TryParse(u.type, out SharedSpace ss))
            {
                unitTypeIndex = 100 + (int)ss;
                int[] sI = unitType[unitTypeIndex].spriteIndices;

                int j = u.loc[0] + (u.loc[1] - u.loc[0]) / 2;
                if ((j == 2) || (j == 7) || (j == 12)) j--;
                else if ((j == 3) || (j == 8) || (j == 13)) j++;

                foreach (int i in Enumerable.Range(u.loc[0], u.loc[1] + 1 - u.loc[0]).OrderBy(x => r.Next()))
                {
                    if (i == j)
                    {
                        sprites[i].SetActive(true);
                        SpriteRenderer s = sprites[i].GetComponent<SpriteRenderer>();
                        s.sprite = ParcelationManager.sprites[sI[0]];
                        s.color = Color.black;
                        s.material.renderQueue = 3002;
                    }
                    else
                    {
                        //Debug.LogError("TURNING OFF row " + u.location[0] + " col " + col);
                        sprites[i].SetActive(false);
                    }
                }
            }
            else
                unitTypeIndex = -1;
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

        if (u.user_id == ParcelationManager.Instance.currentUserId)
        {
            int unitTypeIndex;

            if (System.Enum.TryParse(u.type, out LivingArrangement lv))
            {
                ParcelationManager.Instance.userUnitR.Clear();
                unitTypeIndex = (int)lv;
                for (int x = u.loc[0]; x <= u.loc[1]; x++)
                {
                    roomUnits[x].SetActive(true);
                    Renderer r = roomUnits[x].transform.GetChild(1).GetComponent<Renderer>();
                    r.material.SetColor("_Color", unitType[unitTypeIndex]._Color);    //TODO: Additional Hightlights
                    r.material.SetColor("_EmissionColor", unitType[unitTypeIndex]._EmissionColor);
                    r.material.renderQueue = 3002;
                    ParcelationManager.Instance.userUnitR.Add(r);
                }
                ParcelationManager.Instance.UColor = unitType[unitTypeIndex]._EmissionColor;
                Vector4 v = unitType[unitTypeIndex]._EmissionColor;
                print("v set");
                ParcelationManager.Instance.uDelta = (Vector4.one - v) / 10f;
                print(ParcelationManager.Instance.uDelta);
            }
        }
        else
        {

            int unitTypeIndex;

            if (System.Enum.TryParse(u.type, out LivingArrangement lv))
            {
                unitTypeIndex = (int)lv;
            }
            else
            {
                if (System.Enum.TryParse(u.type, out SharedSpace ss))
                    unitTypeIndex = 100 + (int)ss;
                else
                    unitTypeIndex = -1;
            }
            for (int x = u.loc[0]; x <= u.loc[1]; x++)
            {
                roomUnits[x].SetActive(true);
                Renderer r = roomUnits[x].transform.GetChild(1).GetComponent<Renderer>();
                r.material.SetColor("_Color", unitType[unitTypeIndex]._Color);
                r.material.SetColor("_EmissionColor", unitType[unitTypeIndex]._EmissionColor);
                r.material.renderQueue = 3002;
            }
        }

        RandomizeUnitSprite(u);
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