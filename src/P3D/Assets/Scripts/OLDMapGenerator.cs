/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class OLDMapGenerator : MonoBehaviour {
    
    // Settings
    public const int MinRooms = 20;
    public const int MaxRooms = 30;

    public const int MaxGridX = 10;
    public const int MinGridX = -10;
    public const int MaxGridY = 3;
    public const int MinGridY = -1;
    public const int MaxGridZ = 10;
    public const int MinGridZ = -10;
    
    public List<RoomDataTemplate> availableRooms;
    public const bool Testing = false;

    private OLDRoomPlacer _roomPlacer;
    private List<GameObject> _placedRooms;
    private Stack<GameObject> _roomStack;
    public void Awake() {
        _roomPlacer = new OLDRoomPlacer(this);
        _placedRooms = new List<GameObject>();
        _roomStack = new Stack<GameObject>();
    }

    public void GenerateEntrance() {
        var startRoom = _roomPlacer.PlaceRoom(RoomType.Entrance1, 0, 0, 10);
        _roomStack.Push(startRoom);
    }
    
    public void Generate() {
        int limit = 0;
        // Listen for spacebar press
        while (_roomStack.Count > 0 && _placedRooms.Count < MaxRooms && limit++ < 1) {
            var currentRoom = _roomStack.Peek();
            var currentRoomData = currentRoom.GetComponent<RoomPlacer.RoomData>();
            //TODO: start over, using entrance trackers.
            Debug.Log("currentRoomData.roomType: " + currentRoomData.roomType);
            Debug.Log("currentRoomData.entrances: " + string.Join(", ", currentRoomData.entrances));

            // 1. Pick a valid entrance
            var entranceDirection = GetNextValidDirection(currentRoomData);
            // Debug.Log("direction picked: " + entranceDirection);

            if (!entranceDirection.HasValue) {
                // All entrances used, backtrack
                _roomStack.Pop();
                continue;
            }

            // 2. Calculate the position of the next room
            var nextRoomGridPos = CalculateNextRoomPosition(currentRoom, entranceDirection.Value);
            Debug.Log("nextRoomGridPos: " + nextRoomGridPos);

            if (!nextRoomGridPos.HasValue) {
                // If the next room is out of bounds, treat this as a dead end
                _roomStack.Pop();
                continue;
            }

            int roomGenerationAttempt = 0;
            GameObject nextRoom = null;
            while (roomGenerationAttempt < 5) {
                // 3. Determine the room type based on boundaries and suitable rooms
                var nextRoomType = DetermineNextRoomType(currentRoomData, entranceDirection.Value);
                Debug.Log("nextRoomType: " + nextRoomType);

                if (!nextRoomType.HasValue) {
                    // If no suitable room is found, treat this as a dead end
                    _roomStack.Pop();
                    continue;
                }
            
                Debug.Log("Attempting to place room: " + nextRoomType.Value + " (Attempt " + roomGenerationAttempt + ")");
                nextRoom = _roomPlacer.PlaceRoom(nextRoomType.Value, (int)nextRoomGridPos.Value.x,
                    (int)nextRoomGridPos.Value.y, (int)nextRoomGridPos.Value.z);
                if (nextRoom != null) {
                    _placedRooms.Add(nextRoom);
                    break;
                }

                roomGenerationAttempt++;
            }

            if (nextRoom == null) {
                // If the room cannot be generated, treat this as a dead end
                _roomPlacer.PlaceRoom(RoomType.DeadEnd1, (int)nextRoomGridPos.Value.x, (int)nextRoomGridPos.Value.y,
                    (int)nextRoomGridPos.Value.z);
                _roomStack.Pop();
                continue;
            }

            Debug.Log("nextRoom: " + nextRoom);
            _roomStack.Push(nextRoom);
        }
        Debug.Log("------ Finished Generating Room ------");
    }

    private Direction? GetNextValidDirection(RoomPlacer.RoomData currentRoomData) {
        var availableEntrances = currentRoomData.entrances.Keys.ToList();
        if (availableEntrances.Count == 0) return null;
        return availableEntrances[Random.Range(0, availableEntrances.Count)];
    }

    private Vector3? CalculateNextRoomPosition(GameObject currentRoom, Direction direction) {
        var currentRoomPosition = new Vector3(
            currentRoom.transform.position.x / GameManager.RoomLength,
            currentRoom.transform.position.y / GameManager.RoomHeight,
            currentRoom.transform.position.z / GameManager.RoomWidth
        );
        // Debug.Log("currentRoomPosition: " + currentRoomPosition);
        var offset = GameManager.GetGridPosTowards(direction); // Returns a Vector3 offset based on the entrance
        var nextRoomGridPos = currentRoomPosition + offset;
        // Check if the next room is out of bounds
        if (IsOutOfBounds(nextRoomGridPos)) {
            return null;
        }
        return nextRoomGridPos;
    }

    private RoomType? DetermineNextRoomType(RoomPlacer.RoomData currentRoomData, Direction direction) {
        if (currentRoomData.roomType == RoomType.HallwayStaircase1Bot) {
            return RoomType.HallwayStaircase1Top;
        }
        if (currentRoomData.roomType == RoomType.HallwayStaircase1Top) {
            return RoomType.HallwayStaircase1Bot;
        }

        var suitableNextRooms = currentRoomData.suitableRooms;
        if (suitableNextRooms.Count == 0) {
            return null;
        }
        return suitableNextRooms[Random.Range(0, suitableNextRooms.Count)];
    }

    public bool IsOutOfBounds(Vector3 position) {
        return position.x < MinGridX || position.x > MaxGridX ||
               position.y < MinGridY || position.y > MaxGridY ||
               position.z < MinGridZ || position.z > MaxGridZ;
    }
    
    /// <summary>
    /// Get the room data template from the room type.
    /// If the room type is not found, return null.
    /// </summary>
    /// <param name="type">The room type to get the room data template from.</param>
    /// <returns>The RoomDataTemplate object.</returns>
    public RoomDataTemplate GetRoomFromType(RoomType type) {
        foreach (RoomDataTemplate room in availableRooms) {
            if (room.roomType == type) {
                return room;
            }
        }
        return null;
    }
    
    public List<GameObject> GetPlacedRooms() {
        return _placedRooms;
    }
}
*/
