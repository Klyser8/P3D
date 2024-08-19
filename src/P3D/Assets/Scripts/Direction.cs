using UnityEngine;

public class Direction {
    public static readonly Vector3Int North = new(0, 0, 1);
    public static readonly Vector3Int East = new(1, 0, 0);
    public static readonly Vector3Int South = new(0, 0, -1);
    public static readonly Vector3Int West = new(-1, 0, 0);
    public static readonly Vector3Int Up = new(0, 1, 0);
    public static readonly Vector3Int Down = new(0, -1, 0);
    public static readonly Vector3Int[] All = {North, East, South, West, Up, Down};
    public static readonly Vector3Int[] Horizontal = {North, East, South, West};
    public static readonly Vector3Int[] Vertical = {Up, Down};
    
    public static Vector3Int GetRotatedDirection(Vector3Int direction, RoomPlacer.Rotation rotation) {
        return rotation switch {
            RoomPlacer.Rotation.Rot0 => direction,
            RoomPlacer.Rotation.Rot90 => new Vector3Int(direction.z, direction.y, -direction.x),
            RoomPlacer.Rotation.Rot180 => new Vector3Int(-direction.x, direction.y, -direction.z),
            RoomPlacer.Rotation.Rot270 => new Vector3Int(-direction.z, direction.y, direction.x),
            _ => throw new System.ArgumentOutOfRangeException()
        };
    }
}
