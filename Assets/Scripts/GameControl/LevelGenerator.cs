using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static HSD_Utils;

public class LevelGenerator : MonoBehaviour
{
    readonly float roomPathSideChance = 5 / 12f;

    readonly List<(int, float)> StartingInitBranchProbs = new List<(int, float)>() {
        (4, 1f ),
        (3, 1f ),
        (2, 1/3f ),
    };

    GameObject[] startingRooms;
    GameObject[] regularRooms;

    GameObject storedRoomsLocation;
    GameObject storedWallsLocation;

    public List<List<RoomData>> mapGrid;
    public Vector2Int startingRoomCoords;
    public Vector2Int bossRoomCoords;

    public int roomCount;

    private List<Vector2Int> cornerRooms = new List<Vector2Int>();
    private List<Vector2Int> deadEndRooms = new List<Vector2Int>();
    private List<Vector2Int> branchOffRooms = new List<Vector2Int>();
    private List<Vector2Int> junctionRooms = new List<Vector2Int>();

    private float stemOffBranchChance   = 1 / 2f;
    private float stemOffCornerChance   = 2 / 6f;
    private float stemOffJunctionChance = 1 / 6f;

    GameObject terrainObj;

    public int roomID;

    private bool awoken = false;
    private void Awake() {
        if (!awoken) {
            terrainObj = GameObject.Find("/Environment/Terrain");

            startingRooms = Resources.LoadAll<GameObject>("Rooms/StartingRooms");
            regularRooms = Resources.LoadAll<GameObject>("Rooms/Regular");

            storedRoomsLocation = GameObject.Find("/Data/Rooms");
            storedWallsLocation = GameObject.Find("/Data/Walls");
            awoken = true;
        }
    }

    public System.Tuple<GameObject, GameObject> GenerateLevel(int seed, int distToBoss, int maxOtherDist = 3, int minRooms = 0) {
        if (!awoken) {
            Debug.Log("LATE AWAKE");
            terrainObj = GameObject.Find("/Environment/Terrain");

            startingRooms = Resources.LoadAll<GameObject>("Rooms/StartingRooms");
            regularRooms = Resources.LoadAll<GameObject>("Rooms/Regular");

            storedRoomsLocation = GameObject.Find("/Data/Rooms");
            storedWallsLocation = GameObject.Find("/Data/Walls");
            awoken = true;
        }
        // set up first room
        GameObject startingRoom = Instantiate(randFromList(startingRooms, seed),terrainObj.transform);
        RoomData startingRoom_D = startingRoom.GetComponent<RoomData>();
        startingRoom_D.isCleared = true;

        //if(mapGrid != null) mapGrid.Clear();
        mapGrid = new List<List<RoomData>> {new List<RoomData>{ startingRoom_D } };
        startingRoomCoords = new Vector2Int(0, 0);
        cornerRooms.Clear();
        deadEndRooms.Clear();
        branchOffRooms.Clear();
        junctionRooms.Clear();
        roomCount = 1;

        // add all other rooms
        int numBranches = ProbabilityDistributionGetter(Random.Range(0f, 1f), StartingInitBranchProbs);
        List<RoomPosition> branchDirections = ShuffleList(CopyList(AllRoomPositionsL),0);
        for(int _ = 0; _ < 4 - numBranches; _++) {
            branchDirections.RemoveAt(Random.Range(0, branchDirections.Count - 1));
        }
        Debug.Log("Init paths: " + ListToString(branchDirections));
        
        CarvePath(seed, startingRoom_D, Vector2Int.zero, distToBoss, false, branchDirections[0], true, Resources.LoadAll<GameObject>("Rooms/Boss/RockGolemBoss"));
        for(int i = 1; i < branchDirections.Count; i++) {
            CarvePath(seed, startingRoom_D, startingRoomCoords, maxOtherDist, true, branchDirections[i]);
        }
        /*
        RoomPosition mainDirection = (RoomPosition)Random.Range(1, 4 + 1);
        CarvePath(seed, startingRoom_D, Vector2Int.zero, distToBoss, false, mainDirection, true, Resources.LoadAll<GameObject>("Rooms/Boss/RockGolemBoss"));
        
        CarvePath(seed, startingRoom_D, startingRoomCoords, maxOtherDist, true, (RoomPosition)underOverflowCalc((int)mainDirection + 1, 1, 4));
        CarvePath(seed, startingRoom_D, startingRoomCoords, maxOtherDist, true, (RoomPosition)underOverflowCalc((int)mainDirection + 2, 1, 4));
        CarvePath(seed, startingRoom_D, startingRoomCoords, maxOtherDist, true, (RoomPosition)underOverflowCalc((int)mainDirection + 3, 1, 4));
        */
        RecursiveReorganizeFloor(startingRoomCoords, new List<Vector2Int>() { startingRoomCoords });
        
        int requiredDeadEnds = 4;
        while (roomCount < minRooms || deadEndRooms.Count < requiredDeadEnds) {
            List<Vector2Int> roomPool;
            float randRoomPool = Random.Range(0f, 1f);
            if (IsBetweenInc(randRoomPool, 0f, stemOffBranchChance)){ // between 0 and X
                if (branchOffRooms.Count > 0) roomPool = branchOffRooms;
                else {
                    float start = 0;
                    if (IsBetweenInc(randRoomPool, start, start + stemOffBranchChance * stemOffCornerChance / (1 - stemOffBranchChance)) && cornerRooms.Count > 0)
                        roomPool = cornerRooms;
                    else {
                        if (junctionRooms.Count > 0) roomPool = junctionRooms;
                        else roomPool = cornerRooms;
                    }
                }
            }
            else if(IsBetweenInc(randRoomPool,stemOffBranchChance,stemOffBranchChance + stemOffCornerChance)) { // between X and Y
                if (cornerRooms.Count > 0) roomPool = cornerRooms;
                else {
                    float start = stemOffBranchChance;
                    if (IsBetweenInc(randRoomPool, start, start + stemOffCornerChance * stemOffBranchChance / (1 - stemOffCornerChance)) && branchOffRooms.Count > 0)
                        roomPool = branchOffRooms;
                    else {
                        if (junctionRooms.Count > 0) roomPool = junctionRooms;
                        else roomPool = branchOffRooms;
                    }
                }
            }
            else { // between Y and 1
                if (junctionRooms.Count > 0) roomPool = junctionRooms;
                else {
                    float start = stemOffBranchChance + stemOffCornerChance;
                    if (IsBetweenInc(randRoomPool, start, start + stemOffJunctionChance * stemOffBranchChance / (1 - stemOffJunctionChance)) && branchOffRooms.Count > 0)
                        roomPool = branchOffRooms;
                    else {
                        if (cornerRooms.Count > 0) roomPool = cornerRooms;
                        else roomPool = branchOffRooms;
                    }
                }
            }
            /*
            string poolString;
            if (roomPool == branchOffRooms) poolString = "BranchOff";
            else if (roomPool == cornerRooms) poolString = "Corner";
            else poolString = "Junction";
            */
            if (roomPool.Count == 0) {
                Debug.Log("Branch, Corner, Junction all empty");
                roomPool = deadEndRooms; 
                //poolString = "DeadEnd";
            }

            Vector2Int stemPoint = randFromList(roomPool, seed);
            //Debug.Log("POS: " + stemPoint + " POOL: " + poolString + ": " + ListToString(roomPool));

            //PrintMapGrid();
            RoomData stemData = MatrixAtCoords(mapGrid, stemPoint);
            //Debug.Log("STEMDATA: " + stemData);
            List<RoomPosition> stemFreePositions = AllRoomPositionsL.Except(stemData.GetConnectionDirections()).ToList();
            int maxDist = distToBoss - DistanceBetweenPoints(stemPoint, startingRoomCoords);
            int dist = (maxDist - 1 > 1) ? Random.Range(1, maxDist) : 1;

            CarvePath(seed, stemData, stemPoint, dist, true, randFromList(stemFreePositions, seed));

            RecursiveReorganizeFloor(startingRoomCoords, new List<Vector2Int>() { startingRoomCoords });
        }
        
        CleanRoomLists(false);
        /*
        foreach (Vector2Int coords in deadEndRooms) {
            Debug.Log("DeadEnd " + coords + " to start: " + DistanceBetweenPoints(coords, startingRoomCoords) + ", to boss: " + DistanceBetweenPoints(coords,bossRoomCoords));
        }
        */
        Debug.Log("ROOM COUNT: " + roomCount);  

        // generate wall for first room
        GameObject foundWall = FindMatchingWall(startingRoom_D.GenerateWallSuffix());
        foundWall.transform.parent = terrainObj.transform;
        foundWall.SetActive(true);
        return new System.Tuple<GameObject, GameObject>(startingRoom, foundWall);
    }

    // assumes valid input
    public GameObject FindMatchingWall(string suffix) {
        Transform[] trs = storedWallsLocation.GetComponentsInChildren<Transform>(true);
        foreach(Transform t in trs) {
            if(t.name == "Walls_" + suffix) {
                return Instantiate(t.gameObject);
            }
        }
        return null;
    }

    /*
     * Test seeds:
     * 552797207
     * 255382440
     */
    private void CarvePath(int seed, RoomData currRoom, Vector2Int currentCoords, int distance, bool canDoubleBack, 
    RoomPosition start, bool endsInBoss = false, GameObject[] bossRoomPool = null) {
        RoomPosition pathStraight;
        int currDistance = distance;

        bool TryAddRoomToPath(GameObject instantiatedRoom, RoomPosition direction, RoomData instRoomData, bool sideways = false) {
            System.Tuple<bool, Vector2Int> result = AddToMapGrid(currentCoords, direction, instRoomData);
            if(result.Item1 == false) { // no collision
                // test if connected where it shouldnt
                bool nevermind = false;
                List<RoomData> connections = NeighbouringData(result.Item2);
                foreach(RoomData room in connections) {
                    if (room != null) {
                        if (room.type == RoomType.Boss) { // boss must be a deadend
                            Debug.Log("BOSSNEAR");
                            nevermind = true;
                            break;
                        }
                    }
                }
                if(GetPhysicalNeighbors(result.Item2, connections) > 3) {
                    nevermind = true;
                }

                if (nevermind) {
                    RemoveFromMapGrid(result.Item2);
                    return false;
                }
                // tests passed
                currRoom.AddInDirection(instantiatedRoom, direction);
                pathStraight = direction;
                currentCoords = result.Item2;
                roomCount++;
                return true;
            }
            return false;
        }
        GameObject GenerateRoom(GameObject[] pool) {
            GameObject room = Instantiate(randFromList(pool, seed), storedRoomsLocation.transform);
            room.GetComponent<RoomData>().id = roomID;
            roomID++;
            room.SetActive(false);
            return room;
        }
        
        if (start == RoomPosition.None) pathStraight = (RoomPosition)Random.Range(1, 4 + 1);
        else pathStraight = start;
        // guarantee first room goes in start direction
        GameObject firstRoom = GenerateRoom(regularRooms);
        RoomData roomData = firstRoom.GetComponent<RoomData>();

        TryAddRoomToPath(firstRoom, pathStraight, roomData);
        
        currRoom = roomData;
        currDistance--;

        bool ignoreSidePaths = true;

        while (currDistance > 0) {
            GameObject newRoom = GenerateRoom(regularRooms);
            RoomData newRoomData = newRoom.GetComponent<RoomData>();
            if(1f - Random.Range(0f,1f) < roomPathSideChance && ignoreSidePaths == false) { // go sideways
                if (!canDoubleBack && pathStraight != start) {
                    RoomPosition courseCorrect;
                    if ((RoomPosition)underOverflowCalc((int)pathStraight - 1, 1, 4) == start) courseCorrect = (RoomPosition)underOverflowCalc((int)pathStraight - 1, 1, 4);
                    else courseCorrect = (RoomPosition)underOverflowCalc((int)pathStraight + 1, 1, 4);
                    if(TryAddRoomToPath(newRoom, courseCorrect, newRoomData, true) == false) {
                        // couldnt add to the side, try to add to straight
                        if(TryAddRoomToPath(newRoom, pathStraight, newRoomData, false) == false) {
                            break;
                        }
                    }
                }
                else {
                    RoomPosition[] sides = new RoomPosition[2] { (RoomPosition)underOverflowCalc((int)pathStraight - 1, 1, 4),
                                                             (RoomPosition)underOverflowCalc((int)pathStraight + 1, 1, 4) };
                    int randIdx = Random.Range(0, sides.Length);
                    if (TryAddRoomToPath(newRoom, sides[randIdx], newRoomData, true) == false) {
                        if (TryAddRoomToPath(newRoom, sides[underOverflowCalc(randIdx + 1, 0, 1)], newRoomData, true) == false) {
                            // couldnt add to the sides, try to add to straight
                            if (TryAddRoomToPath(newRoom, pathStraight, newRoomData, false) == false) {
                                break;
                            }
                        }
                    }
                }
                ignoreSidePaths = true;
            }
            else { // go straight
                if (TryAddRoomToPath(newRoom, pathStraight, newRoomData, false) == false) {
                    break;
                }
                ignoreSidePaths = false;
            }
            currRoom = newRoom.GetComponent<RoomData>();
            currDistance--;
            
        }
        
        if (endsInBoss) {
            GameObject lastRoom = GenerateRoom(bossRoomPool);
            ReplaceRoom(currentCoords, lastRoom.GetComponent<RoomData>());
            roomID--;
            bossRoomCoords = currentCoords;
        }
    }

    private void ReplaceRoom(Vector2Int coords, RoomData replacer) {
        RoomData original = MatrixAtCoords(mapGrid, coords);
        replacer.CopyPosition(original);
        replacer.id = original.id;
        Destroy(original.gameObject);
        mapGrid[coords.x][coords.y] = replacer;
    }

    // returns if it would collide
    private System.Tuple<bool,Vector2Int> AddToMapGrid(Vector2Int coordsFrom, RoomPosition direction, RoomData room) {
        switch (direction) {
            case RoomPosition.Up:
                if (coordsFrom.x == 0) { // has to add to the top
                    mapGrid.Insert(0, new List<RoomData>(new RoomData[mapGrid[0].Count]));
                    ShiftAllCoordinates(1, 0);
                    mapGrid[0][coordsFrom.y] = room;
                    return new System.Tuple<bool, Vector2Int>(false, new Vector2Int(0, coordsFrom.y));
                }
                else {
                    if (MatrixAtCoords(mapGrid, Shift2IVector(coordsFrom, -1, 0)) != null) return new System.Tuple<bool, Vector2Int>(true, Shift2IVector(coordsFrom, -1, 0));
                    // else
                    mapGrid[coordsFrom.x - 1][coordsFrom.y] = room;
                    return new System.Tuple<bool, Vector2Int>(false, Shift2IVector(coordsFrom, -1, 0));
                }
            case RoomPosition.Right:
                if (coordsFrom.y == mapGrid[0].Count - 1) { // has to add to the right
                    foreach (List<RoomData> r in mapGrid) { r.Add(null); }
                    mapGrid[coordsFrom.x][mapGrid[0].Count - 1] = room;
                    return new System.Tuple<bool, Vector2Int>(false, new Vector2Int(coordsFrom.x, mapGrid[0].Count - 1));
                }
                else {
                    if (MatrixAtCoords(mapGrid, Shift2IVector(coordsFrom, 0, 1)) != null) return new System.Tuple<bool, Vector2Int>(true, Shift2IVector(coordsFrom, 0, 1));
                    // else
                    mapGrid[coordsFrom.x][coordsFrom.y + 1] = room;
                    return new System.Tuple<bool, Vector2Int>(false, Shift2IVector(coordsFrom, 0, 1));
                }
            case RoomPosition.Down:
                if(coordsFrom.x == mapGrid.Count - 1) { // has to add to the bottom
                    mapGrid.Add(new List<RoomData>(new RoomData[mapGrid[0].Count]));
                    mapGrid[mapGrid.Count - 1][coordsFrom.y] = room;
                    return new System.Tuple<bool, Vector2Int>(false, new Vector2Int(mapGrid.Count - 1, coordsFrom.y));
                }
                else {
                    if (MatrixAtCoords(mapGrid, Shift2IVector(coordsFrom, 1, 0)) != null) return new System.Tuple<bool, Vector2Int>(true, Shift2IVector(coordsFrom, 1, 0));
                    // else
                    mapGrid[coordsFrom.x + 1][coordsFrom.y] = room;
                    return new System.Tuple<bool, Vector2Int>(false, Shift2IVector(coordsFrom, 1, 0));
                }
            case RoomPosition.Left:
                if(coordsFrom.y == 0) { // has to add to the left
                    foreach(List<RoomData> r in mapGrid) { r.Insert(0, null); }
                    ShiftAllCoordinates(0, 1);
                    mapGrid[coordsFrom.x][0] = room;
                    return new System.Tuple<bool, Vector2Int>(false, new Vector2Int(coordsFrom.x, 0));
                }
                else {
                    if (MatrixAtCoords(mapGrid, Shift2IVector(coordsFrom, 0, -1)) != null) return new System.Tuple<bool, Vector2Int>(true, Shift2IVector(coordsFrom, 0, -1));
                    // else
                    mapGrid[coordsFrom.x][coordsFrom.y - 1] = room;
                    return new System.Tuple<bool, Vector2Int>(false, Shift2IVector(coordsFrom, 0, -1));
                }
        }
        Debug.LogError("How did this happen");
        return new System.Tuple<bool, Vector2Int>(true, Vector2Int.zero); // something went wrong
    }

    private void RemoveFromMapGrid(Vector2Int coords) {
        RoomData roomToDelete = MatrixAtCoords(mapGrid, coords);
        roomToDelete.RemoveAllConnections();
        Destroy(roomToDelete.gameObject);
        mapGrid[coords.x][coords.y] = null;
        if(coords.x == 0 || coords.x == mapGrid.Count - 1 || coords.y == 0 || coords.y == mapGrid[0].Count - 1) {
            ShrinkMapGrid();
        }
    }

    private void ShrinkMapGrid() {
        bool changesMade = true;
        while (changesMade) {
            changesMade = false;

            bool shaveTop = true;
            foreach(RoomData cell in mapGrid[0]) {
                if(cell != null) {
                    shaveTop = false;
                    break;
                }
            }
            if (shaveTop) {
                mapGrid.RemoveAt(0);
                ShiftAllCoordinates(-1, 0);
                changesMade = true;
            }

            bool shaveRight = true;
            foreach(List<RoomData> row in mapGrid) {
                if(row[row.Count - 1] != null) {
                    shaveRight = false;
                    break;
                }
            }
            if (shaveRight) {
                foreach (List<RoomData> row in mapGrid) {
                    row.RemoveAt(row.Count - 1);
                }
                changesMade = true;
            }

            bool shaveBot = true;
            foreach (RoomData cell in mapGrid[mapGrid.Count - 1]) {
                if (cell != null) {
                    shaveBot = false;
                    break;
                }
            }
            if (shaveBot) {
                mapGrid.RemoveAt(mapGrid.Count - 1);
                changesMade = true;
            }

            bool shaveLeft = true;
            foreach (List<RoomData> row in mapGrid) {
                if (row[0] != null) {
                    shaveLeft = false;
                    break;
                }
            }
            if (shaveLeft) {
                foreach (List<RoomData> row in mapGrid) {
                    row.RemoveAt(0);
                }
                ShiftAllCoordinates(0, -1);
                changesMade = true;
            }
        }
    }

    private void ShiftAllCoordinates(int x, int y) {
        Vector2Int shift = new Vector2Int(x, y);
        startingRoomCoords += shift;
        bossRoomCoords += shift;
        for(int i = 0; i < cornerRooms.Count; i++) {
            Vector2Int vec = cornerRooms[i];
            vec += shift;
            cornerRooms[i] = vec;
        }
        for (int i = 0; i < deadEndRooms.Count; i++) {
            Vector2Int vec = deadEndRooms[i];
            vec += shift;
            deadEndRooms[i] = vec;
        }
        for(int i = 0; i < branchOffRooms.Count; i++) {
            Vector2Int vec = branchOffRooms[i];
            vec += shift;
            branchOffRooms[i] = vec;
        }
        for(int i = 0; i < junctionRooms.Count; i++) {
            Vector2Int vec = junctionRooms[i];
            vec += shift;
            junctionRooms[i] = vec;
        }
    }

    private List<RoomData> NeighbouringData(Vector2Int coords) {
        List<RoomData> roomData = new List<RoomData> {
            MatrixAtCoords(mapGrid, Shift2IVector(coords, -1, 0)),
            MatrixAtCoords(mapGrid, Shift2IVector(coords, 0, 1)),
            MatrixAtCoords(mapGrid, Shift2IVector(coords, 1, 0)),
            MatrixAtCoords(mapGrid, Shift2IVector(coords, 0, -1))
        };
        return roomData;
    }

    private int GetPhysicalNeighbors(Vector2Int coords, List<RoomData> neighborData = null) {
        int count = 0;
        if(neighborData == null) neighborData = NeighbouringData(coords);
        foreach(RoomData r in neighborData) {
            if (r != null) count++;
        }
        return count;
    }

    public int DistanceBetweenPoints(Vector2Int currPoint, Vector2Int destination) {
        return DistanceBetweenPoints(currPoint, destination, 0, new List<Vector2Int>());
    }

    public int DistanceBetweenPoints(Vector2Int currPoint, Vector2Int destination, int currDist, List<Vector2Int> prevPoints) {
        if (currPoint == destination) return currDist;
        else {
            if (prevPoints.Contains(currPoint)) return int.MaxValue;
            prevPoints.Add(currPoint);

            if (MatrixAtCoords(mapGrid, currPoint) == default(RoomData)) return int.MaxValue;
            currDist++;

            return Mathf.Min(DistanceBetweenPoints(Shift2IVector(currPoint, -1, 0), destination, currDist, new List<Vector2Int>(prevPoints)),
                             DistanceBetweenPoints(Shift2IVector(currPoint, 0, 1), destination, currDist, new List<Vector2Int>(prevPoints)),
                             DistanceBetweenPoints(Shift2IVector(currPoint, 1, 0), destination, currDist, new List<Vector2Int>(prevPoints)),
                             DistanceBetweenPoints(Shift2IVector(currPoint, 0, -1), destination, currDist, new List<Vector2Int>(prevPoints)));
        }
    }

    // Up right down left
    private List<Vector2Int> UpdateMapConnections(Vector2Int coords) {
        List<Vector2Int> roomsChanged = new List<Vector2Int>();
        RoomData dataAtCoords = MatrixAtCoords(mapGrid, coords);
        
        if (coords.x - 1 >= 0 && MatrixAtCoords(mapGrid, Shift2IVector(coords, -1, 0)) != null && dataAtCoords.up == null) { // up
            dataAtCoords.up = MatrixAtCoords(mapGrid, Shift2IVector(coords, -1, 0)).gameObject;
            MatrixAtCoords(mapGrid, Shift2IVector(coords, -1, 0)).down = dataAtCoords.gameObject;
            roomsChanged.Add(Shift2IVector(coords, -1, 0));
        }
        if (coords.y + 1 < mapGrid[coords.x].Count && MatrixAtCoords(mapGrid, Shift2IVector(coords, 0, 1)) != null && dataAtCoords.right == null) { // right
            dataAtCoords.right = MatrixAtCoords(mapGrid, Shift2IVector(coords, 0, 1)).gameObject;
            MatrixAtCoords(mapGrid, Shift2IVector(coords, 0, 1)).left = dataAtCoords.gameObject;
            roomsChanged.Add(Shift2IVector(coords, 0, 1));
        }
        if (coords.x + 1 < mapGrid.Count && MatrixAtCoords(mapGrid, Shift2IVector(coords, 1, 0)) != null && dataAtCoords.down == null) { // down
            dataAtCoords.down = MatrixAtCoords(mapGrid, Shift2IVector(coords, 1, 0)).gameObject;
            MatrixAtCoords(mapGrid, Shift2IVector(coords, 1, 0)).up = dataAtCoords.gameObject;
            roomsChanged.Add(Shift2IVector(coords, 1, 0));
        }
        if (coords.y - 1 >= 0 && MatrixAtCoords(mapGrid, Shift2IVector(coords, 0, -1)) != null && dataAtCoords.left == null) { // left
            dataAtCoords.left = MatrixAtCoords(mapGrid, Shift2IVector(coords, 0, -1)).gameObject;
            MatrixAtCoords(mapGrid, Shift2IVector(coords, 0, -1)).right = dataAtCoords.gameObject;
            roomsChanged.Add(Shift2IVector(coords, 0, -1));
        }
        return roomsChanged;
    }
    
    private void ReorganizeRoomAndNeighbours(Vector2Int coords, RoomData data, bool debug = false) {
        if (coords == new Vector2Int(-1, -1)) debug = true;
        int numConnections = data.GetNumConnections();
        if (data.type != RoomType.Boss) {
            if (numConnections > 1) {
                deadEndRooms.Remove(coords);
                if (numConnections == 2) {
                    List<RoomPosition> connections = data.GetConnectionDirections();
                    if (AreDirectionsPerpendicular(connections[0], connections[1]) == true) { // 2 connections at perpendicular angles
                        AddIfNotIn(cornerRooms, coords);
                        branchOffRooms.Remove(coords);
                    }
                    else { // 2 connections directly across from eachother
                        RoomData first = data.GetRoomFromDirection(connections[0]).GetComponent<RoomData>();
                        RoomData second = data.GetRoomFromDirection(connections[1]).GetComponent<RoomData>();

                        if (((first.GetNumConnections() == 2 && !AreDirectionsPerpendicular(first.GetConnectionDirections()[0], first.GetConnectionDirections()[1])) || (first.GetNumConnections() == 1))
                         && ((second.GetNumConnections() == 2 && !AreDirectionsPerpendicular(second.GetConnectionDirections()[0], second.GetConnectionDirections()[1])) || (second.GetNumConnections() == 1))) {
                            AddIfNotIn(branchOffRooms, coords);
                        }
                        else {
                            branchOffRooms.Remove(coords);
                        }
                    }
                    junctionRooms.Remove(coords);
                }
                else if (numConnections > 2) {
                    cornerRooms.Remove(coords);
                    branchOffRooms.Remove(coords);
                    if (numConnections == 3) {
                        AddIfNotIn(junctionRooms, coords);
                    }
                    else junctionRooms.Remove(coords);
                }
            }
            else { // if numConnections == 1
                AddIfNotIn(deadEndRooms, coords);
            }
        }
        else {
            cornerRooms.Remove(coords);
            branchOffRooms.Remove(coords);
            deadEndRooms.Remove(coords);
            junctionRooms.Remove(coords);
        }
    }
    
    private void RecursiveRoomConnectors(Vector2Int currCoords, List<Vector2Int> prevRooms) {
        if (currCoords.x < 0 || currCoords.x >= mapGrid.Count || currCoords.y < 0 || currCoords.y >= mapGrid[0].Count || MatrixAtCoords(mapGrid, currCoords) == null) return;
        if (!prevRooms.Contains(currCoords)) {
            prevRooms.Add(currCoords);

            UpdateMapConnections(currCoords);

            RecursiveRoomConnectors(Shift2IVector(currCoords, -1, 0), new List<Vector2Int>(prevRooms));
            RecursiveRoomConnectors(Shift2IVector(currCoords, 0, 1), new List<Vector2Int>(prevRooms));
            RecursiveRoomConnectors(Shift2IVector(currCoords, 1, 0), new List<Vector2Int>(prevRooms));
            RecursiveRoomConnectors(Shift2IVector(currCoords, 0, -1), new List<Vector2Int>(prevRooms));
        }
    }
    
    private void RecursiveReorganizeFloor(Vector2Int currCoords, List<Vector2Int> noFeatures) {
        RecursiveReorganizeFloor(currCoords, new List<Vector2Int>(), noFeatures);
    }
    private void RecursiveReorganizeFloor(Vector2Int currCoords, List<Vector2Int> prevRooms, List<Vector2Int> noFeatures, bool roomConnectionsDone = false) {
        if (!roomConnectionsDone) {
            ClearNullRooms();
            RecursiveRoomConnectors(currCoords, new List<Vector2Int>());
        }
        if (currCoords.x < 0 || currCoords.x >= mapGrid.Count || currCoords.y < 0 || currCoords.y >= mapGrid[0].Count || MatrixAtCoords(mapGrid, currCoords) == null) return;
        if (!prevRooms.Contains(currCoords)) {
            prevRooms.Add(currCoords);
            if (!noFeatures.Contains(currCoords)) {
                RoomData roomData = MatrixAtCoords(mapGrid, currCoords);
                ReorganizeRoomAndNeighbours(currCoords, roomData);
            }
            
            RecursiveReorganizeFloor(Shift2IVector(currCoords, -1, 0), new List<Vector2Int>(prevRooms), noFeatures, true); //up
            RecursiveReorganizeFloor(Shift2IVector(currCoords, 0, 1), new List<Vector2Int>(prevRooms), noFeatures, true); //right
            RecursiveReorganizeFloor(Shift2IVector(currCoords, 1, 0), new List<Vector2Int>(prevRooms), noFeatures, true); //down
            RecursiveReorganizeFloor(Shift2IVector(currCoords, 0, -1), new List<Vector2Int>(prevRooms), noFeatures, true); //left
        }
    }

    private void CleanRoomLists(bool display = true) {
        cornerRooms.ForEach(item => branchOffRooms.Remove(item));
        deadEndRooms.ForEach(item => branchOffRooms.Remove(item));

        ClearNullRooms();

        foreach (Vector2Int coords in cornerRooms) {    MatrixAtCoords(mapGrid, coords).mapFeature = MapFeature.Corner; }
        foreach (Vector2Int coords in deadEndRooms) {   MatrixAtCoords(mapGrid, coords).mapFeature = MapFeature.DeadEnd; }
        foreach (Vector2Int coords in branchOffRooms) { MatrixAtCoords(mapGrid, coords).mapFeature = MapFeature.BranchOff; }
        foreach (Vector2Int coords in junctionRooms) {  MatrixAtCoords(mapGrid, coords).mapFeature = MapFeature.Junction; }

        if (display) {
            Debug.Log("CornerRooms: " + ListToString(cornerRooms));
            Debug.Log("DeadEnds: " + ListToString(deadEndRooms));
            Debug.Log("BranchRooms: " + ListToString(branchOffRooms));
            Debug.Log("JunctionRooms: " + ListToString(junctionRooms));
            Debug.Log("Boss: " + bossRoomCoords);
            Debug.Log("BossDist: " + DistanceBetweenPoints(startingRoomCoords, bossRoomCoords) + " " + Vector2Int.Distance(startingRoomCoords, bossRoomCoords));
        }
    }

    private void ClearNullRooms() {
        List<Vector2Int> newlyNullRooms = new List<Vector2Int>();
        foreach (Vector2Int coords in cornerRooms) {    if (MatrixAtCoords(mapGrid, coords) == null || coords == null) newlyNullRooms.Add(coords); }
        foreach (Vector2Int coords in deadEndRooms) {   if (MatrixAtCoords(mapGrid, coords) == null || coords == null) newlyNullRooms.Add(coords); }
        foreach (Vector2Int coords in branchOffRooms) { if (MatrixAtCoords(mapGrid, coords) == null || coords == null) newlyNullRooms.Add(coords); }
        foreach (Vector2Int coords in junctionRooms) {  if (MatrixAtCoords(mapGrid, coords) == null || coords == null) newlyNullRooms.Add(coords); }
        cornerRooms = cornerRooms.Except(newlyNullRooms).ToList();
        deadEndRooms = deadEndRooms.Except(newlyNullRooms).ToList();
        branchOffRooms = branchOffRooms.Except(newlyNullRooms).ToList();
        junctionRooms = junctionRooms.Except(newlyNullRooms).ToList();
    }

    private void PrintMapGrid() {
        foreach(List<RoomData> row in mapGrid) {
            string line = "";
            foreach (RoomData room in row) {
                if (room == null) line += "0 ";
                else line += "1 ";
            }
            Debug.Log(line);
        }
    }
}
