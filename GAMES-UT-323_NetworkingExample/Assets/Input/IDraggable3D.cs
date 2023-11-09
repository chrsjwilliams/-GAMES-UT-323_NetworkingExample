using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public interface IDraggable3D
{
    // touching the draggable
    public Finger id { get; set; }
    public bool canMove { get; set; }
    public Vector3 pos { get; set; }
    public Quaternion rot { get; set; }
    public Vector3 scale { get; set; }

    public void OnTap();
}
