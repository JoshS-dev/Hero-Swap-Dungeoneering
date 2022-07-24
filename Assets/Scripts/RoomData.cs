using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using static HSD_Utils;

public class RoomData : MonoBehaviour
{
    public int id;
    
    public bool isCleared = false;

    public GameObject up, right, down, left;

    public RoomType type;
    public MapFeature mapFeature = MapFeature.None;

    [SerializeField]
    public Transform enemies_T;

    public float pickupChance;
    public Vector3 pickupDropCentre = new Vector3(5.5f,-4.5f,0f);

    // Start is called before the first frame update
    void Awake()
    {
        up = right = down = left = null;
        enemies_T = transform.Find("Enemies");
    }

    public void CopyPosition(RoomData copiedRoom) {
        //isCleared = copiedRoom.isCleared;
        up = copiedRoom.up;     right = copiedRoom.right;   down = copiedRoom.down;     left = copiedRoom.left;
        if(up != null)      GetData(up).down = gameObject;
        if(right != null)   GetData(right).left = gameObject;
        if(down != null)    GetData(down).up = gameObject;
        if(left != null)    GetData(left).right = gameObject;
        //mapFeature = copiedRoom.mapFeature;
    }

    protected RoomData GetData(GameObject room) {
        return room.GetComponent<RoomData>();
    }

    // For file referencing
    public string GenerateWallSuffix() {
        string returnString = "";
        if (up == null) returnString += "0"; else returnString += "1";
        if (right == null) returnString += "0"; else returnString += "1";
        if (down == null) returnString += "0"; else returnString += "1";
        if (left == null) returnString += "0"; else returnString += "1";
        return returnString;
    }

    protected void AddToUp(GameObject room) {
        up = room;
        GetData(room).down = gameObject;
    }
    protected void AddToRight(GameObject room) {
        right = room;
        GetData(room).left = gameObject;
    }
    protected void AddToDown(GameObject room) {
        down = room;
        GetData(room).up = gameObject;
    }
    protected void AddToLeft(GameObject room) {
        left = room;
        GetData(room).right = gameObject;
    }

    public void AddInDirection(GameObject room, RoomPosition direction) {
        switch (direction) {
            case RoomPosition.Up:       AddToUp(room);      break;
            case RoomPosition.Right:    AddToRight(room);   break;
            case RoomPosition.Down:     AddToDown(room);    break;
            case RoomPosition.Left:     AddToLeft(room);    break;
        }
    }

    protected void RemoveFromUp() {
        if (up == null) return;
        GetData(up).down = null;
        up = null;
    }
    protected void RemoveFromRight() {
        if (right == null) return;
        GetData(right).left = null;
        right = null;
    }
    protected void RemoveFromDown() {
        if (down == null) return;
        GetData(down).up = null;
        down = null;
    }
    protected void RemoveFromLeft() {
        if (left == null) return;
        GetData(left).right = null;
        left = null;
    }

    public void RemoveAllConnections() {
        RemoveFromUp();
        RemoveFromRight();
        RemoveFromDown();
        RemoveFromLeft();
    }

    public GameObject GetRoomFromDirection(RoomPosition direction) {
        switch (direction) {
            case RoomPosition.Up:   return up;
            case RoomPosition.Right:return right;
            case RoomPosition.Down: return down;
            case RoomPosition.Left: return left;
            default:                return null;
        }
    }

    public int GetNumConnections() {
        return GetConnectionDirections().Count;
    }

    public List<GameObject> GetConnectedRooms() {
        List<GameObject> connections = new List<GameObject>();
        if (up != null) connections.Add(up);
        if (right != null) connections.Add(right);
        if (down != null) connections.Add(down);
        if (left != null) connections.Add(left);
        return connections;
    }

    public List<RoomPosition> GetConnectionDirections() {
        List<RoomPosition> connections = new List<RoomPosition>();
        if (up != null)     connections.Add(RoomPosition.Up);
        if (right != null)  connections.Add(RoomPosition.Right);
        if (down != null)   connections.Add(RoomPosition.Down);
        if (left != null)   connections.Add(RoomPosition.Left);
        return connections;
    }

    public void WakeRoom() {
        foreach(Transform c in enemies_T) {
            c.GetComponent<NavMeshAgent>().enabled = true;
        }
    }

    public void SleepRoom() {
        foreach (Transform c in enemies_T) {
            c.GetComponent<NavMeshAgent>().enabled = false;
        }
    }
    /*
    public override string ToString() {
        return base.ToString();
    }
    */
    
}
