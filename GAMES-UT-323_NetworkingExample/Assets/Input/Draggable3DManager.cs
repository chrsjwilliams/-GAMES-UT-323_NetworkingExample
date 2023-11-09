using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class Draggable3DManager : MonoBehaviour
{
    // To cache references for better perfromance
    private Camera mainCamera;
    private InputManager inputManager;

    [SerializeField] private bool disableInput;
    [SerializeField] private float tapThreshold;

    private float dist;
    private Vector3 offset;
    private IDraggable3D toDrag;

    Vector3 pos;

    Vector3 startPos;
    Vector3 endPos;

    private void Awake()
    {
        inputManager = InputManager.Instance;
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // Subscribe to the OnStartTouchEvent
        inputManager.OnStartTouch += StartTouch;
        inputManager.OnEndTouch += EndTouch;
        inputManager.OnTouchMoved += TouchMoved;
    }

    private void OnDisable()
    {
        // Subscribe to the OnStartTouchEvent
        inputManager.OnStartTouch -= StartTouch;
        inputManager.OnEndTouch -= EndTouch;
        inputManager.OnTouchMoved -= TouchMoved;
    }

    public void DiableInput(bool b)
    {
        disableInput = b;
        if (toDrag != null)
        {
            EndTouch(toDrag.id, Time.time);
        }
    }

    private void StartTouch(Finger finger, float time)
    {
        if (disableInput) return;

        CalculateFingetPosition(finger, setOffset: true, out pos);
        startPos = pos;
    }

    private void TouchMoved(Finger finger, float time)
    {
        if (toDrag == null || disableInput) return;
        if (!toDrag.canMove) return;

        pos = new Vector3(finger.screenPosition.x, finger.screenPosition.y, dist);
        pos = mainCamera.ScreenToWorldPoint(pos);
        toDrag.pos = pos + offset;
    }

    private void EndTouch(Finger finger, float time)
    {
        if (toDrag == null || toDrag.id != finger) return;

        CalculateFingetPosition(finger, setOffset: false, out pos);
        endPos = pos;
   
        if (Vector3.Distance(startPos, endPos) <= tapThreshold)
        {
            toDrag.OnTap();
        }

        toDrag.id = null;
        toDrag = null;
    }

    void CalculateFingetPosition(Finger finger, bool setOffset, out Vector3 position)
    {
        position = Vector3.negativeInfinity;
        Ray ray = mainCamera.ScreenPointToRay(finger.screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Check to see if our raycast hit an object of our super class type
            var hitObject = hit.collider.gameObject.GetComponent<NetworkPlayer>();
            // only continue if the hit object implements the IDraggable3D interface
            if (hitObject is IDraggable3D && ((IDraggable3D)hitObject).canMove)
            {
                toDrag = (IDraggable3D)hitObject;

                toDrag.id = finger;
                // adjust dist so our objects remain on the same depth plane
                dist = hit.transform.position.z - mainCamera.transform.position.z;
                position = new Vector3(finger.screenPosition.x, finger.screenPosition.y, dist);
                position = mainCamera.ScreenToWorldPoint(pos);

                if (!setOffset) return;
                offset = toDrag.pos - position;
            }
        }
    }
}
