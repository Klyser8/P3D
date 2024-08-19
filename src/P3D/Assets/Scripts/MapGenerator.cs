using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour {
    
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

    private RoomPlacer _roomPlacer;
    private List<GameObject> _placedRooms;
    private Stack<GameObject> _roomStack;
    public void Awake() {
        _roomPlacer = new RoomPlacer(this);
        _placedRooms = new List<GameObject>();
        _roomStack = new Stack<GameObject>();
    }

    public void GenerateEntrance() {
        RoomDataTemplate entranceRoomDataTemplate = GetRoomFromType(RoomType.Entrance1);
        var entranceRoom = _roomPlacer.PlaceRoom(null, null, entranceRoomDataTemplate);
        _roomStack.Push(entranceRoom);
    }

    public void Generate() {
        var entrance = PickEntrance();
        var roomType = PickRoomType();
        // 3. placeRoom
    }

    private Entrance PickEntrance() {
        // Fetch the room at the top of the stack
        var currentRoom = _roomStack.Peek();
        var currentRoomData = currentRoom.GetComponent<RoomPlacer.RoomData>();
        // Return a random entrance that's not already linked
        var unlinkedEntrances = currentRoomData.entrances.FindAll(entrance => !entrance.IsLinked());
        return unlinkedEntrances[Random.Range(0, unlinkedEntrances.Count)];
    }

    private RoomType PickRoomType() {
        var currentRoom = _roomStack.Peek();
        var currentRoomData = currentRoom.GetComponent<RoomPlacer.RoomData>();
        var suitableRooms = currentRoomData.suitableRooms;
        
        //1. If the current room is a staircase, the next room must be the complementary one.
        if (currentRoomData.roomType == RoomType.HallwayStaircase1Bot) {
            return RoomType.HallwayStaircase1Top;
        }
        if (currentRoomData.roomType == RoomType.HallwayStaircase1Top) {
            return RoomType.HallwayStaircase1Bot;
        }
        
        //2. Else, pick a random room from the suitable rooms
        return suitableRooms[Random.Range(0, suitableRooms.Count)];
    }

    public bool IsOutOfBounds(Vector3Int gridPosition) {
        return gridPosition.x < MinGridX || gridPosition.x > MaxGridX ||
               gridPosition.y < MinGridY || gridPosition.y > MaxGridY ||
               gridPosition.z < MinGridZ || gridPosition.z > MaxGridZ;
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
