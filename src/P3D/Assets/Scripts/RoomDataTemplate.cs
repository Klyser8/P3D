using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the data for a room.
///
/// - roomType: The type of room. See RoomType.cs for more information.
/// - entrances: The entrances of the room. See CardinalPoint.cs for more information.
/// - suitableRooms: The room types that can be connected to this room.
/// - isMandatory: If the room is mandatory to be placed, for the game to be possible.
/// - isEndRoom: If the room is the end room.
/// - entrancePositions: The positions of the entrances.
/// 
/// </summary>
[CreateAssetMenu(fileName = "RoomDataTemplate", menuName = "Rooms/RoomDataTemplate", order = 1)]
public class RoomDataTemplate : ScriptableObject
{
    public RoomType roomType;
    public List<CardinalPoint> entrances;
    public List<RoomType> suitableRooms;
    public bool isMandatory;
    public bool isEndRoom;

    public GameObject roomPrefab;
}
