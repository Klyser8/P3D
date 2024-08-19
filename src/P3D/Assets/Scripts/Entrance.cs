using UnityEngine;

public class Entrance {
    private Vector3Int _direction;
    private readonly GameObject _tracker;
    private bool _isLinked;
    
    public Entrance(Vector3Int direction, GameObject tracker) {
        _direction = direction;
        _tracker = tracker;
        _isLinked = false;
    }
    
    public Vector3Int GetDirection() {
        return _direction;
    }
    
    public GameObject GetTracker() {
        return _tracker;
    }
    
    public bool IsLinked() {
        return _isLinked;
    }
    
    public void SetDirection(Vector3Int direction) {
        _direction = direction;
    }
    
    public void SetLinked(bool isLinked) {
        _isLinked = isLinked;
    }
}