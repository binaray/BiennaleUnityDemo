using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//This class manages occupied units' properties and position

public class BuildingStateManager : MonoBehaviour
{
    //Template unit prefabs
    [SerializeField]
    private UnitObject[] unitTypeTemplate;

    private int currentUser = -1;
    private Dictionary<int, UnitObject> currentState;
    public static BuildingStateManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        currentState = new Dictionary<int, UnitObject>();

        //string savedStateJson = PlayerPrefs.GetString(Constants.UNIT_DB_STRING_PREF, "");
        //if (savedStateJson != "")
        //{
        //    currentState = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Unit>>(savedStateJson);

        //}
        //else
        //    currentState = new Dictionary<int, Unit>();
    }

    public void UpdateBuildingState(Dictionary<int, Unit> newState)
    {
        HashSet<int> currentKeys = new HashSet<int>(currentState.Keys);
        HashSet<int> newKeys = new HashSet<int>(newState.Keys);
        newState.ToList().ForEach(x => Debug.Log(x.Key));
        

        //Deletion
        HashSet<int> toModify = new HashSet<int>(currentKeys);
        toModify.ExceptWith(newKeys);

        //Edit
        toModify = new HashSet<int>(newKeys);
        toModify.IntersectWith(currentKeys);

        //New insertion
        toModify = new HashSet<int>(newKeys);
        toModify.ExceptWith(currentKeys);
        InsertUnits(toModify, newState);

        //TODO: save current state on application exit for offline support
    }

    private void InsertUnits(HashSet<int> insertKeys, Dictionary<int, Unit> newState)
    {
        foreach (int key in insertKeys)
        {
            Debug.LogWarning("Inserted "+key+": "+newState[key].ToString());
            int anchorIndex = newState[key].GetAnchorIndex();
            int unitTypeIndex = newState[key].unitTypeIndex;
            currentState[key] = Instantiate(unitTypeTemplate[unitTypeIndex], BuildingBlockManager.Instance.PositionAtBlockIndex(anchorIndex), Quaternion.identity, this.transform);
            //TODO: queue a magical animation here
        }
    }

    private void DeleteUnits()
    {

    }

}

//json serializable which is used to compare states with the server's
[System.Serializable]
class BuildingState
{
    public List<Unit> occupiedUnits = new List<Unit>();
    public static BuildingState CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<BuildingState>(jsonString);
    }
}