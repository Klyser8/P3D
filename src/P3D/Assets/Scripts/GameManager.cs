using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public const int RoomWidth = 10;
    public const int RoomLength = 10;
    public const int RoomHeight = 5;
    
    private static MapGenerator _mapGenerator;
    
    private void Start()
    {
        _mapGenerator = FindObjectOfType<MapGenerator>();
        _mapGenerator.GenerateEntrance();
    }

    private void Update() {
        var spacebar = KeyCode.Space;
        if (Input.GetKeyDown(spacebar)) {
            Debug.Log("Spacebar pressed");
        }
    }
    
    [CanBeNull]
    public static GameObject GetRoomAtGridPos(Vector3 gridPos) {
        return _mapGenerator.GetPlacedRooms()
            .Select(room => room.gameObject)
            .FirstOrDefault(room => 
                Mathf.Approximately(room.transform.position.x, gridPos.x * RoomLength) &&
                Mathf.Approximately(room.transform.position.y, gridPos.y * RoomHeight) &&
                Mathf.Approximately(room.transform.position.z, gridPos.z * RoomWidth));
    }
    
    [CanBeNull] 
    public static GameObject GetAdjacentRoom(Vector3 gridPos, Vector3 direction) {
        // This method retrieves the room adjacent to the specified grid position in the specified direction
        var adjacentPosition = gridPos + GetGridPosTowards(direction);
        return GetRoomAtGridPos(adjacentPosition);
    }
    
    public static Vector3Int ToGridPosition(Vector3 position) {
        // This method converts a world position to a grid position
        return new Vector3Int(
            Mathf.RoundToInt(position.x / RoomLength),
            Mathf.RoundToInt(position.y / RoomHeight),
            Mathf.RoundToInt(position.z / RoomWidth)
        );
    }

    public static Vector3 ToWorldPosition(Vector3Int gridPosition) {
        // This method converts a grid position to a world position
        return new Vector3(
            gridPosition.x * RoomLength,
            gridPosition.y * RoomHeight,
            gridPosition.z * RoomWidth
        );
    }
    
    public static Vector3Int GetGridPosTowards(Vector3 direction, int distance = 1) {
        direction.Scale(new Vector3(RoomLength * distance, RoomHeight * distance, RoomWidth * distance));
        return new Vector3Int(
            Mathf.RoundToInt(direction.x),
            Mathf.RoundToInt(direction.y),
            Mathf.RoundToInt(direction.z)
        );
    }
    
    public static Vector3Int CardinalPointToVector3Int(CardinalPoint cardinalPoint) {
        // This method converts a cardinal point to a Vector3Int
        return cardinalPoint switch {
            CardinalPoint.North => Direction.North,
            CardinalPoint.East => Direction.East,
            CardinalPoint.South => Direction.South,
            CardinalPoint.West => Direction.West,
            CardinalPoint.Up => Direction.Up,
            CardinalPoint.Down => Direction.Down,
            _ => throw new ArgumentOutOfRangeException(nameof(cardinalPoint), cardinalPoint, null)
        };
    }
    
}