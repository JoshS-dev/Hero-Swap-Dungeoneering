using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using static HSD_Utils;
using static GameStateManager;

public class RunManager : MonoBehaviour
{
    public int RUN_SEED = -1;
    public float runTime;

    GameObject player;
    GameObject terrainObj;
    GameObject storedRoomsLocation;

    LevelGenerator _lg;
    public GameObject currRoom;
    private RoomData currRoom_D;
    private Vector2Int currCoords;
    private GameObject currWalls;
    readonly float doorLenience = 0.1f;
    readonly float doorDepthPush = 0.125f;

    NavMeshSurface2d _nms;
    UserInterfaceManager _uim;
    MenuHandler _mh;

    List<float> randomValuePool = new List<float>();
    private int randomPoolIdx;
    private int randomPoolSize = 1024;

    [SerializeField]
    GameObject BasePickup;

    void Awake() {
        player = GameObject.Find("/Player");
        terrainObj = GameObject.Find("/Environment/Terrain");
        storedRoomsLocation = GameObject.Find("/Data/Rooms");

        _nms = GameObject.Find("/NavMesh").GetComponent<NavMeshSurface2d>();
        _lg = GetComponent<LevelGenerator>();
        GameObject UIobj = GameObject.Find("/UI");
        _uim = UIobj.GetComponent<UserInterfaceManager>();
        
        _mh = GameObject.Find("/Menus").GetComponent<MenuHandler>();

        _mh.ShowNewGameMenu();
    }

    void Update() {
        if(CurrGamestate == GameState.Running) {
            runTime += Time.deltaTime;
        }

        //if(currRoom != null && currRoom.transform.Find("Enemies").childCount == 0 && currRoom_D.isCleared == false) {
        if (currRoom != null && currRoom_D.enemies_T.childCount == 0 && currRoom_D.isCleared == false) {
            //Debug.Log("EMPTY");
            currRoom_D.isCleared = true;
            if (currRoom_D.pickupChance > 0) {
                if (NextRandomPoolVal(currRoom_D.id) <= currRoom_D.pickupChance) {
                    Instantiate(BasePickup, currRoom_D.pickupDropCentre, Quaternion.identity, currRoom.transform.Find("Pickups"));
                }
            }
            if(currRoom_D.type == RoomType.Boss) {
                _mh.ShowNewGameMenu(true);
            }
        }
        //currRoom_D.isCleared = true;

        // CHECK IF PLAYER GOING THROUGH OPEN DOOR
        if (currRoom != null && currRoom_D.isCleared) {
            //UP
            if(currRoom_D.up != null && (player.transform.position.y > (0.5f + doorDepthPush)) && (Mathf.Abs(player.transform.position.x - 5.5f) <= doorLenience)) {
                //Debug.Log("UP");
                player.transform.position = new Vector3(5.5f, -9.5f, 0f);
                
                currCoords.x -= 1;
                ChangeRoom(currRoom_D.up);
            }
            //RIGHT
            if (currRoom_D.right != null && (player.transform.position.x > (10.5f + doorDepthPush)) && (Mathf.Abs(player.transform.position.y - -4.5f) <= doorLenience)) {
                //Debug.Log("RIGHT");
                player.transform.position = new Vector3(0.5f, -4.5f, 0f);

                currCoords.y += 1;
                ChangeRoom(currRoom_D.right);
            }
            //DOWN
            if (currRoom_D.down != null && (player.transform.position.y < (-9.5f - doorDepthPush)) && (Mathf.Abs(player.transform.position.x - 5.5f) <= doorLenience)) {
                //Debug.Log("DOWN");
                player.transform.position = new Vector3(5.5f, 0.5f, 0f);

                currCoords.x += 1;
                ChangeRoom(currRoom_D.down);
            }
            //LEFT
            if (currRoom_D.left != null && (player.transform.position.x < (0.5f - doorDepthPush)) && (Mathf.Abs(player.transform.position.y - -4.5f) <= doorLenience)) {
                //Debug.Log("LEFT");
                player.transform.position = new Vector3(10.5f, -4.5f, 0f);

                currCoords.y -= 1;
                ChangeRoom(currRoom_D.left);
            }
        }
    }

    private void ChangeRoom(GameObject newRoom) {
        UnloadRoom();

        currRoom = newRoom;
        currRoom_D = currRoom.GetComponent<RoomData>();

        currRoom.GetComponent<Transform>().parent = terrainObj.transform;
        currRoom.SetActive(true);

        currWalls = _lg.FindMatchingWall(currRoom_D.GenerateWallSuffix());
        currWalls.transform.parent = terrainObj.transform;
        currWalls.SetActive(true);

        _nms.BuildNavMesh();
        currRoom_D.WakeRoom();
        _uim.UpdateMap(currCoords);

        BasePickup.GetComponent<PickupInstance>().roomSeeded = currRoom_D.id;

        //Debug.Log(currRoom_D.GetNumConnections() + " " + currRoom_D.mapFeature);
    }

    public void UnloadRoom() {
        Destroy(currWalls);

        if (currRoom != null) {
            currRoom.transform.parent = storedRoomsLocation.transform;
            currRoom_D.SleepRoom();
            currRoom.SetActive(false);
        }
    }

    public void StartRun() {
        runTime = 0f;
        SetSeed(RUN_SEED);
        randomValuePool.Clear();
        randomPoolIdx = -1;
        for (int i = 0; i < randomPoolSize; i++) { randomValuePool.Add(Random.Range(0f, 1f)); }
        _lg.roomID = 0;
        // SMALL: 5, 3, 20
        // MEDIUM: 7, 5, 30
        // LARGE: 10, 8, 40
        System.Tuple<GameObject,GameObject> returnVal = _lg.GenerateLevel(RUN_SEED, 7, 5, 30);
        currRoom = returnVal.Item1;
        currWalls = returnVal.Item2;
        currCoords = _lg.startingRoomCoords;
        Debug.Log("START: " +   currCoords);
        currRoom_D = currRoom.GetComponent<RoomData>();
        _nms.BuildNavMesh();
        StartCoroutine(DelayUpdateMap());

        ChangeState(GameState.Running);
    }

    private IEnumerator DelayUpdateMap() {
        yield return new WaitForFixedUpdate();
        _uim.UpdateMap(currCoords);
    }

    public float NextRandomPoolVal(int forceIdx = -1) {
        if (forceIdx == -1) { randomPoolIdx = underOverflowCalc(randomPoolIdx + 1, 0, randomPoolSize - 1); }
        else {
            Debug.Log(forceIdx + ": " + randomValuePool[underOverflowCalc(forceIdx, 0, randomPoolSize - 1)]);
            return randomValuePool[underOverflowCalc(forceIdx, 0, randomPoolSize - 1)]; 
        }
        //Debug.Log(randomPoolIdx + ": " + randomValuePool[randomPoolIdx]);
        return randomValuePool[randomPoolIdx];
    }
}
