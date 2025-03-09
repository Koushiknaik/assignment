using UnityEngine;

public class MoveUp : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField, Tooltip("Starting Y-position of the slot reel")]
    private float startingPos = 0f;

    [SerializeField, Tooltip("Upper Y-position threshold")]
    private float endingPos = 10f;

    [SerializeField, Tooltip("Movement speed in units per second")]
    private float speed = 1f;

    [Header("Slot Settings")]
    [SerializeField, Range(1, 5), Tooltip("Number of possible slot positions")]
    private int maxSlots = 3;

    [SerializeField, Tooltip("Vertical spacing between slot positions")]
    private float slotSpacing = 84f;

    private Transform cachedTransform;
    private bool isMoving = false;  // Changed to private as it's controlled by methods
    public int SelectedPoint { get; private set; }  // Renamed and made property

    private const float BASE_POSITION = -166f;  // Named magic number

    private void Start()
    {
        cachedTransform = transform;

        if (startingPos >= endingPos)
        {
            Debug.LogWarning($"{name}: startingPos ({startingPos}) should be less than endingPos ({endingPos})", this);
        }

        // Set initial position
        cachedTransform.localPosition = new Vector3(
            cachedTransform.localPosition.x,
            startingPos,
            cachedTransform.localPosition.z
        );
    }

    /// <summary>
    /// Stops the slot reel and selects a random position
    /// </summary>
    public void StopSpinning()
    {
        if (!isMoving) return;

        isMoving = false;
        SelectedPoint = Random.Range(0, maxSlots);

        SetSlotPosition();
    }

    /// <summary>
    /// Starts the slot reel spinning
    /// </summary>
    public void StartSpinning()
    {
        if (isMoving) return;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;

        cachedTransform.position += Vector3.up * speed * Time.deltaTime;

        if (cachedTransform.localPosition.y > endingPos)
        {
            ResetToStartingPosition();
        }
    }

    private void SetSlotPosition()
    {
        Vector3 newPosition = cachedTransform.localPosition;
        newPosition.y = BASE_POSITION + (SelectedPoint * slotSpacing);
        cachedTransform.localPosition = newPosition;
    }

    private void ResetToStartingPosition()
    {
        Vector3 position = cachedTransform.localPosition;
        position.y = startingPos;
        cachedTransform.localPosition = position;
    }

    // Optional: For debugging in Unity Inspector
    private void OnValidate()
    {
        if (speed < 0f)
        {
            Debug.LogWarning($"{name}: Speed should be positive", this);
            speed = Mathf.Abs(speed);
        }
    }
}