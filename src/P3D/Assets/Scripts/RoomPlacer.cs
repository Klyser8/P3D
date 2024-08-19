using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class RoomPlacer {

    private readonly MapGenerator _mapGenerator;
    public RoomPlacer(MapGenerator mapGenerator) {
        _mapGenerator = mapGenerator;
    }

    public GameObject PlaceRoom(GameObject currentRoom, Entrance entrance, RoomDataTemplate newRoomDataTemplate) {
        var newGridPos = CalculateNewRoomGridPos(currentRoom, entrance);
        
        var newRoom = Object.Instantiate(newRoomDataTemplate.roomPrefab, newGridPos, Quaternion.identity);
        newRoom.name = newRoomDataTemplate.roomType + $" ({newGridPos.x}, {newGridPos.y}, {newGridPos.z})";
        newRoom.transform.position = GameManager.ToWorldPosition(newGridPos);
        
        RoomData newRoomData = CreateRoomData(newRoom, newRoomDataTemplate);
        
        // 1. Get a list of shuffled rotations to cycle through
        List<Rotation> rotations = GetShuffledRotations();
        
        foreach (var rotation in rotations) {
            
            // 2. Ensure that, upon applying the rotation, none of the entrances are leading to an out-of-bounds area.
            // 2.1 Apply rotation
            // 2.2 Offset grid pos towards the direction of each entrance
            // 2.3 Check that the new grid pos is not out of bounds
            // 2.4 Repeat for each entrance in the current rotation
            // 2.5 If this fails, try again with the next rotation.
            var oldRotation = newRoom.transform.rotation;
            var oldEntranceList = newRoomData.entrances;
            newRoom.transform.rotation = GetQuaternionFromRotation(rotation);
            // Update direction field for each entrance
            foreach (var entranceData in newRoomData.entrances) { //TODO: test this out
                entranceData.SetDirection(Direction.GetRotatedDirection(entranceData.GetDirection(), rotation));
            }
            bool isOutOfBounds = false;
            foreach (var newEntrance in newRoomData.entrances) {
                var newGridPosTowards = GameManager.GetGridPosTowards(newEntrance.GetDirection());
                if (_mapGenerator.IsOutOfBounds(newGridPosTowards)) {
                    isOutOfBounds = true;
                }
            }
            if (isOutOfBounds) {
                newRoom.transform.rotation = oldRotation;
                newRoomData.entrances = oldEntranceList;
                continue;
            }
            
            // 3. Check all entrance trackers for current room and adjacent rooms, ensuring that all free ones aren't blocked by a room.
            // 3.1 Loop through all unlinked entrance trackers for the new room.
            // 3.2 If any tracker has a room the direction it's facing, and it is not linked, it means the entrance is being blocked.
            // 3.3 
        }
        
        _mapGenerator.GetPlacedRooms().Add(newRoom);
        return newRoom;
    }

    private Vector3Int CalculateNewRoomGridPos(GameObject currentRoom, Entrance entrance) {
        Vector3Int currentGridPos;
        if (currentRoom == null) {
            currentGridPos = new Vector3Int(0, 0, 10);
        } else {
            currentGridPos = GameManager.ToGridPosition(currentRoom.transform.position);
        }
        if (entrance == null) {
            return currentGridPos + new Vector3Int(0, 0, 0);
        }
        return currentGridPos + GameManager.GetGridPosTowards(entrance.GetDirection());
    }

    private RoomData CreateRoomData(GameObject newRoom, RoomDataTemplate newRoomDataTemplate) {
        var newRoomData = newRoom.AddComponent<RoomData>();
        newRoomData.roomType = newRoomDataTemplate.roomType;
        newRoomData.isEndRoom = newRoomDataTemplate.isEndRoom;
        newRoomData.isMandatory = newRoomDataTemplate.isMandatory;
        newRoomData.suitableRooms = newRoomDataTemplate.suitableRooms;
        newRoomData.entrances = new List<Entrance>();
        foreach (var entranceCardinalPoint in newRoomDataTemplate.entrances) {
            newRoomData.entrances.Add(new Entrance(
                GameManager.CardinalPointToVector3Int(entranceCardinalPoint), 
                CreateEntranceTracker(newRoom, entranceCardinalPoint)));
        }

        return newRoomData;
    }

    private GameObject CreateEntranceTracker(GameObject parentRoom, CardinalPoint cardinalPoint) {
        var entranceTracker = new GameObject("Entrance Tracker");
        entranceTracker.transform.parent = parentRoom.transform;
        entranceTracker.transform.localPosition = cardinalPoint switch {
            CardinalPoint.North => new Vector3(0, 0, -GameManager.RoomWidth / 2.0f),
            CardinalPoint.South => new Vector3(0, 0, GameManager.RoomWidth / 2.0f),
            CardinalPoint.East => new Vector3(-GameManager.RoomLength / 2.0f, 0, 0),
            CardinalPoint.West => new Vector3(GameManager.RoomLength / 2.0f, 0, 0),
            CardinalPoint.Up => new Vector3(0, GameManager.RoomHeight, 0),
            CardinalPoint.Down => new Vector3(0, 0, 0),
            _ => throw new ArgumentOutOfRangeException()
        };
        return entranceTracker;
    }
    
    private List<Rotation> GetShuffledRotations() {
        var rotations = new List<Rotation> { Rotation.Rot0, Rotation.Rot90, Rotation.Rot180, Rotation.Rot270 };
        for (var i = 0; i < rotations.Count; i++) {
            var temp = rotations[i];
            var randomIndex = Random.Range(i, rotations.Count);
            rotations[i] = rotations[randomIndex];
            rotations[randomIndex] = temp;
        }
        return rotations;
    }

    public static Quaternion GetQuaternionFromRotation(Rotation rotation) {
        switch (rotation) {
            case Rotation.Rot0:
                return Quaternion.Euler(0, 0, 0);
            case Rotation.Rot90:
                return Quaternion.Euler(0, 90, 0);
            case Rotation.Rot180:
                return Quaternion.Euler(0, 180, 0);
            case Rotation.Rot270:
                return Quaternion.Euler(0, 270, 0);
            default:
                throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
        }
    }


    public enum Rotation
    {
        Rot0 = 0,
        Rot90 = 90,
        Rot180 = 180,
        Rot270 = 270
        
    }

    public class RoomData : MonoBehaviour {
        public RoomType roomType;
        public List<Entrance> entrances;
        public List<RoomType> suitableRooms;
        public Rotation rotation;
        public bool isMandatory;
        public bool isEndRoom;
    }
    
}
