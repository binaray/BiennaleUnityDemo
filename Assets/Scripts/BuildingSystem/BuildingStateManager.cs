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
        DeleteUnits(toModify);

        //Edit
        toModify = new HashSet<int>(newKeys);
        toModify.IntersectWith(currentKeys);
        CheckAndEditUnits(toModify, newState);

        //New insertion
        toModify = new HashSet<int>(newKeys);
        toModify.ExceptWith(currentKeys);
        InsertUnits(toModify, newState);

        //TODO: save current state on application exit for offline support
    }

    public void ClearBuildingState()
    {
        if (currentState.Count > 0)
        {
            HashSet<int> currentKeys = new HashSet<int>(currentState.Keys);
            DeleteUnits(currentKeys);
        }

    }

    //Inserts keys based on properties in the new state
    private void InsertUnits(HashSet<int> insertKeys, Dictionary<int, Unit> newState)
    {
        foreach (int key in insertKeys)
        {
            Debug.LogWarning("Inserted "+key+": "+newState[key].ToString());
            int anchorIndex = newState[key].GetAnchorIndex();
            int unitTypeIndex = newState[key].unitTypeIndex;
            currentState[key] = Instantiate(unitTypeTemplate[unitTypeIndex], BuildingBlockManager.Instance.PositionAtBlockIndex(anchorIndex), Quaternion.identity, this.transform);
            currentState[key].SetUnitProperties(newState[key]);
            //TODO: queue a magical animation here
        }
    }

    private void CheckAndEditUnits(HashSet<int> editKeys, Dictionary<int, Unit> newState)
    {
        foreach (int key in editKeys)
        {
            Debug.LogWarning("Editing " + key + ": " + newState[key].ToString());
            //anchor position changed
            if (currentState[key].anchorIndex!=newState[key].anchorIndex || currentState[key].floorIndex != newState[key].floorIndex)
            {
                int anchorIndex = newState[key].GetAnchorIndex();
                currentState[key].MoveToAnchorIndex(anchorIndex);
                Debug.LogWarning("Moved " + key);
            }
        }
    }

    //Deletes units based on current state
    private void DeleteUnits(HashSet<int> deleteKeys)
    {
        foreach(int key in deleteKeys)
        {
            Debug.LogWarning("Deleted " + key + ": " + currentState[key].ToString());
            Destroy(currentState[key]);
            currentState.Remove(key);
            //TODO: queue a magical animation here
        }
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