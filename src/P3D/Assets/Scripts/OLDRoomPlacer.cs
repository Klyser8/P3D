/*
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class OLDRoomPlacer {

    private readonly OLDMapGenerator _mapGenerator;
    public OLDRoomPlacer(OLDMapGenerator mapGenerator) {
        _mapGenerator = mapGenerator;
    }

    public GameObject PlaceRoom(RoomType newRoomType, int gridX, int gridY, int gridZ) {
        var rotations = GetShuffledRotations();
        if (newRoomType == RoomType.Entrance1) {
            rotations = new List<Rotation> { Rotation.Rot0 };
        }

        var newRoomDataTemplate = _mapGenerator.GetRoomFromType(newRoomType);
        var newRoom = Object.Instantiate(newRoomDataTemplate.roomPrefab,
            new Vector3(gridX * GameManager.RoomLength, gridY * GameManager.RoomHeight, gridZ * GameManager.RoomWidth),
            Quaternion.identity);
        newRoom.name = $"{newRoomType}({gridX},{gridY},{gridZ})";
        var newRoomGridPos = GameManager.ConvertToGridPosition(newRoom.transform.position);

        var newRoomData = newRoom.AddComponent<RoomData>();
        newRoomData.roomType = newRoomType;
        newRoomData.suitableRooms = newRoomDataTemplate.suitableRooms;
        newRoomData.isMandatory = newRoomDataTemplate.isMandatory;
        newRoomData.isEndRoom = newRoomDataTemplate.isEndRoom;

        Debug.Log($"Attempting to place room: {newRoom.name} at grid position: {newRoomGridPos}");

        foreach (var rotation in rotations) {
            Debug.Log($"Trying rotation: {rotation} for room: {newRoom.name}");
            if (TryPlaceRoomWithRotation(newRoom, newRoomData, newRoomDataTemplate, newRoomGridPos, rotation)) {
                _mapGenerator.GetPlacedRooms().Add(newRoom);
                Debug.Log($"Room {newRoom.name} successfully placed with rotation {rotation}");
                return newRoom;
            }
            Debug.Log($"Rotation {rotation} for room {newRoom.name} is not valid");
        }

        // If no valid rotation is found, destroy the room and return null
        Debug.LogWarning($"Could not place room {newRoom.name} with any rotation. Destroying room.");
        Object.Destroy(newRoom);
        return null;
    }

    private bool TryPlaceRoomWithRotation(GameObject newRoom, RoomData newRoomData, RoomDataTemplate newRoomDataTemplate, 
        Vector3 newRoomGridPos, Rotation rotation) {
        Debug.Log($"Checking if position {newRoomGridPos} is occupied for room {newRoom.name}");
        // Check if the room's intended position is already occupied
        if (GameManager.GetRoomAtGridPos(newRoomGridPos) != null) {
            Debug.LogWarning($"Position {newRoomGridPos} is already occupied. Cannot place room {newRoom.name}");
            return false; // Room already exists at this position
        }

        // If the room below or above is a staircase, the rotation needs to be the same as the other staircase room.
        if (newRoomDataTemplate.roomType == RoomType.HallwayStaircase1Bot) {
            var aboveRoom = GameManager.GetAdjacentRoom(newRoomGridPos, Direction.Up);
            if (aboveRoom != null) {
                var aboveRoomData = aboveRoom.GetComponent<RoomData>();
                if (aboveRoomData.roomType == RoomType.HallwayStaircase1Top) {
                    rotation = aboveRoomData.rotation;
                    Debug.Log($"Matching rotation {rotation} with above staircase room {aboveRoom.name}");
                }
            }
        } else if (newRoomDataTemplate.roomType == RoomType.HallwayStaircase1Top) {
            var belowRoom = GameManager.GetAdjacentRoom(newRoomGridPos, Direction.Down);
            if (belowRoom != null) {
                var belowRoomData = belowRoom.GetComponent<RoomData>();
                if (belowRoomData.roomType == RoomType.HallwayStaircase1Bot) {
                    rotation = belowRoomData.rotation;
                    Debug.Log($"Matching rotation {rotation} with below staircase room {belowRoom.name}");
                }
            }
        }

        var entranceTrackers = CreateEntranceTrackers(newRoom, newRoomDataTemplate.entrances);
        newRoom.transform.rotation = GetQuaternionFromRotation(rotation);
        newRoomData.rotation = rotation;

        // Rotate the entrance tracker keyset
        var rotatedEntrances = new Dictionary<Direction, GameObject>();
        foreach (var (entrance, entranceTracker) in entranceTrackers) {
            var rotatedEntrance = RotateEntrance(entrance, (int)rotation);
            Debug.Log($"Rotating entrance {entrance} to {rotatedEntrance} for room {newRoom.name}");
            rotatedEntrances.Add(rotatedEntrance, entranceTracker);
        }
        
        // Check if the new room's entrances are valid
        if (!AreEntrancesValid(newRoomGridPos, rotatedEntrances)) {
            Debug.Log("Entrance Trackers are invalid for room " + newRoom.name);
            DestroyEntranceTrackers(rotatedEntrances);
            return false;
        }

        newRoomData.entrances = rotatedEntrances;
        Debug.Log($"Room {newRoom.name} placed successfully with entrances aligned.");
        return true;
    }

    private Dictionary<Direction, GameObject> CreateEntranceTrackers(GameObject room, List<Direction> entrances) {
        var newEntrances = new Dictionary<Direction, GameObject>();
        foreach (var entrance in entrances) {
            var entranceTracker = CreateEntranceTracker(room, entrance);
            newEntrances.Add(entrance, entranceTracker);
            Debug.Log($"Created entrance tracker for {entrance} in room {room.name} at position {entranceTracker.transform.position}");
        }
        return newEntrances;
    }

    private GameObject CreateEntranceTracker(GameObject roomObject, Direction direction) {
        var entranceTracker = new GameObject("EntranceTracker");
        entranceTracker.transform.parent = roomObject.transform;
        entranceTracker.transform.localPosition = direction switch {
            Direction.North => new Vector3(0, 0, -GameManager.RoomWidth / 2.0f),
            Direction.South => new Vector3(0, 0, GameManager.RoomWidth / 2.0f),
            Direction.East => new Vector3(-GameManager.RoomLength / 2.0f, 0, 0),
            Direction.West => new Vector3(GameManager.RoomLength / 2.0f, 0, 0),
            Direction.Up => new Vector3(0, GameManager.RoomHeight, 0),
            Direction.Down => new Vector3(0, 0, 0),
            _ => throw new ArgumentOutOfRangeException()
        };

        Debug.Log($"Created entrance tracker for direction {direction} in room {roomObject.name} at local position {entranceTracker.transform.localPosition}");
        return entranceTracker;
    }

    private bool AreEntrancesValid(Vector3 roomGridPos, Dictionary<Direction, GameObject> entrances) {
        foreach (var newEntranceDirection in entrances.Keys) {
            var newEntrancePosition = roomGridPos + GameManager.GetGridPosTowards(newEntranceDirection);
            Debug.Log($"Validating entrance {newEntranceDirection} at position {newEntrancePosition} for room at grid position {roomGridPos}");
        
            // Check if the entrance goes out of bounds
            if (_mapGenerator.IsOutOfBounds(newEntrancePosition)) {
                Debug.LogWarning($"Entrance {newEntranceDirection} is out of bounds for room at grid position {roomGridPos}");
                return false;
            }

            // // Check if the new room's entrances are compatible with adjacent rooms
            // var adjacentRoom = GameManager.GetAdjacentRoom(roomGridPos, newEntranceDirection);
            // if (adjacentRoom != null) {
            //     var entranceTracker = entrances[newEntranceDirection];
            //     Debug.Log($"Checking compatibility of entrance {newEntranceDirection} with adjacent room {adjacentRoom.name}");
            //
            //     // Ensure that the new room's entrance aligns with an open passage in the adjacent room
            //     if (!IsAdjacentRoomCompatible(adjacentRoom, entranceTracker)) {
            //         Debug.LogWarning($"Entrance {newEntranceDirection} is not compatible with adjacent room {adjacentRoom.name}");
            //         return false;
            //     }
            // }
            
            // Get all adjacent rooms and check that all entrance trackers are aligned or unpaired
            var adjacentRooms = new List<GameObject>();
            foreach (Direction direction in Enum.GetValues(typeof(Direction))) {
                var adjacentRoom = GameManager.GetAdjacentRoom(roomGridPos, direction);
                if (adjacentRoom != null) {
                    adjacentRooms.Add(adjacentRoom);
                }
            }
            foreach (var adjacentRoom in adjacentRooms) {
                var adjacentRoomData = adjacentRoom.GetComponent<RoomData>();
                var adjacentRoomEntrances = adjacentRoomData.entrances;
                var entranceTracker = entrances[newEntranceDirection];
                Debug.Log($"Checking compatibility of entrance {newEntranceDirection} with adjacent room {adjacentRoom.name}");
                
                foreach (var adjacentRoomEntrance in adjacentRoomEntrances) {
                    
                }
            }
        }
        return true;
    }

    private bool IsAdjacentRoomCompatible(GameObject adjacentRoom, GameObject entranceTracker) {
        var adjacentRoomData = adjacentRoom.GetComponent<RoomData>();
        foreach (var adjacentRoomTracker in adjacentRoomData.entrances.Values) {
            float distance = Vector3.Distance(entranceTracker.transform.position, adjacentRoomTracker.transform.position);
            Debug.Log($"Checking distance between entrance tracker in room and adjacent room: {distance}");
            if (distance <= 1.0f) {
                Debug.Log($"Valid connection found between entrance tracker in room and adjacent room.");
                return true; // Valid connection
            }
        }
        Debug.LogWarning("No valid connection found between entrance tracker in room and adjacent room.");
        return false; // No valid connection found
    }

    private void DestroyEntranceTrackers(Dictionary<Direction, GameObject> entranceTrackers) {
        foreach (var entranceTracker in entranceTrackers.Values) {
            Debug.Log($"Destroying entrance tracker at position {entranceTracker.transform.position}");
            Object.Destroy(entranceTracker);
        }
    }

    private List<Rotation> GetShuffledRotations() {
        var rotations = new List<Rotation> { Rotation.Rot0, Rotation.Rot90, Rotation.Rot180, Rotation.Rot270 };
        for (var i = 0; i < rotations.Count; i++) {
            var temp = rotations[i];
            var randomIndex = Random.Range(i, rotations.Count);
            rotations[i] = rotations[randomIndex];
            rotations[randomIndex] = temp;
        }
        Debug.Log("Shuffled rotations: " + string.Join(", ", rotations));
        return rotations;
    }

    private Direction RotateEntrance(Direction entrance, int rotation) {
        Debug.Log($"Rotating entrance {entrance} by {rotation} degrees");
        return rotation switch {
            0 => entrance,
            90 => entrance switch {
                Direction.North => Direction.East,
                Direction.East => Direction.South,
                Direction.South => Direction.West,
                Direction.West => Direction.North,
                Direction.Up => Direction.Up,
                Direction.Down => Direction.Down,
                _ => throw new ArgumentOutOfRangeException()
            },
            180 => entrance switch {
                Direction.North => Direction.South,
                Direction.East => Direction.West,
                Direction.South => Direction.North,
                Direction.West => Direction.East,
                Direction.Up => Direction.Up,
                Direction.Down => Direction.Down,
                _ => throw new ArgumentOutOfRangeException()
            },
            270 => entrance switch {
                Direction.North => Direction.West,
                Direction.East => Direction.North,
                Direction.South => Direction.East,
                Direction.West => Direction.South,
                Direction.Up => Direction.Up,
                Direction.Down => Direction.Down,
                _ => throw new ArgumentOutOfRangeException()
            },
            _ => throw new ArgumentOutOfRangeException()
        };
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
    
    
    private Direction GetOppositeDirection(Direction direction) {
        return direction switch {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw new ArgumentOutOfRangeException()
        };
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
        public Dictionary<Direction, GameObject> entrances = new();
        public List<RoomType> suitableRooms;
        public Rotation rotation;
        public bool isMandatory;
        public bool isEndRoom;
    }
    
}
*/
