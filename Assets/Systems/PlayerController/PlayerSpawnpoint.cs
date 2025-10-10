using UnityEngine;

public class PlayerSpawnpoint : MonoBehaviour
{
    GameManager gameManager => GameManager.Instance;
    GameStateManager gameStateManager => GameManager.Instance.GameStateManager;
    PlayerController playerController => GameManager.Instance.PlayerController;
    UIManager uIManager => GameManager.Instance.UIManager;



   

    [Header("Body Gizmo Settings")]
    private Vector3 bodyLocalOffset = new Vector3(0, 1.0f, 0); // Offset for the main body (sphere and cube)
    private Color bodyColor = new Color(1, 0, 0, 0.75f);

    [Header("Arrow Gizmo Settings")]
    private Vector3 arrowLocalOffset = new Vector3(0, 0.75f, -0.2f); // Overall offset for the arrow, relative to the object's origin
    private Color arrowColor = new Color(0, 0, 1, 0.75f);
    private float arrowHeadLength = 0.2f;
    private float arrowHeadHeight = 0.05f;
    private float arrowHeadWidth = 0.065f;




    #region Gizmo Visualization

    void OnDrawGizmos()
    {
        // only allows rotation of the Y axis
        if (transform.eulerAngles.x != 0 || transform.eulerAngles.z != 0)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }

        // --- Group 1: Sphere and Cube (Body of the spawn point) ---
        // Calculate the world position for this group
        Vector3 bodyWorldPosition = transform.position + transform.rotation * bodyLocalOffset;

        // Set the matrix for this group. All subsequent draws will be relative to this.
        Gizmos.matrix = Matrix4x4.TRS(bodyWorldPosition, transform.rotation, transform.localScale);

        Gizmos.color = bodyColor;
        // These are drawn relative to Gizmos.matrix, so their positions are local to it.
        Gizmos.DrawSphere(new Vector3(0.0f, 0.5f, 0.0f), 0.3f);
        Gizmos.DrawCube(new Vector3(0.0f, -0.4f, 0.0f), new Vector3(0.5f, 1.2f, 0.35f));

        // Reset the Gizmo matrix for the next group or other drawing operations
        Gizmos.matrix = Matrix4x4.identity;

        // --- Group 2: Arrow components ---
        // Calculate the world position for this group.
        // This is the single adjustable point for the arrow group.
        Vector3 arrowWorldPosition = transform.position + transform.rotation * arrowLocalOffset;
        Quaternion arrowWorldRotation = transform.rotation; // Arrow's rotation is the object's rotation

        // Set the matrix for the arrow group. All subsequent arrow draws will be relative to this.
        Gizmos.matrix = Matrix4x4.TRS(arrowWorldPosition, arrowWorldRotation, Vector3.one); // Scale of 1 for arrow components

        Gizmos.color = arrowColor;

        // The arrow's shaft and tip are drawn relative to the 'arrowWorldPosition' (which is the origin of Gizmos.matrix)
        // So, their positions are now hardcoded local positions (relative to the arrow group's matrix).
        Gizmos.DrawCube(new Vector3(0.0f, 0.0f, 0.7f), new Vector3(0.05f, 0.05f, 0.5f)); // Arrow shaft
        Gizmos.DrawSphere(new Vector3(0.0f, 0.0f, 1.05f), 0.06f); // Arrowhead tip

        // For the arrow head, we're drawing it relative to the arrow's matrix.
        // The 'localEnd' is the point in the arrow's local space where the head begins.
        // The arrow starts at the origin (0,0,0) of its group's matrix.
        Vector3 arrowLocalStart = Vector3.zero; // Arrow starts at the origin of its local matrix
        Vector3 arrowLocalEnd = new Vector3(0.0f, 0.0f, 1.1f); // End of the arrow shaft, where head starts

        DrawArrowHead(arrowLocalStart, arrowLocalEnd);

        // Always reset the matrix after drawing a group
        Gizmos.matrix = Matrix4x4.identity;
    }

    // Renamed for clarity - this draws the head of the arrow
    private void DrawArrowHead(Vector3 localStart, Vector3 localEnd)
    {
        Vector3 direction = (localEnd - localStart).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        // Calculates positions for the rectangular shapes of the arrowhead
        Vector3 right = rotation * Quaternion.Euler(0, 180 + 45, 0) * new Vector3(0, 0, arrowHeadLength);
        Vector3 left = rotation * Quaternion.Euler(0, 180 - 45, 0) * new Vector3(0, 0, arrowHeadLength);

        // Draws each side of the arrowhead
        DrawArrowRectangle(localEnd, right);
        DrawArrowRectangle(localEnd, left);
    }

    // Simplified DrawArrowRectangle - it assumes Gizmos.matrix is already set for the arrow group
    private void DrawArrowRectangle(Vector3 localOrigin, Vector3 localDirectionAndSize)
    {
        Vector3 cubePosition = localOrigin + localDirectionAndSize / 2f; // Center of the cube
        Quaternion cubeRotation = Quaternion.LookRotation(localDirectionAndSize.normalized);
        Vector3 cubeScale = new Vector3(arrowHeadWidth, arrowHeadHeight, localDirectionAndSize.magnitude);

        // Save the current Gizmos.matrix (which is the arrow group's matrix)
        Matrix4x4 originalMatrix = Gizmos.matrix;

        // Apply a transform for this individual rectangle relative to the arrow group's matrix
        Gizmos.matrix *= Matrix4x4.TRS(cubePosition, cubeRotation, cubeScale);

        Gizmos.DrawCube(Vector3.zero, Vector3.one); // Draw a unit cube, transformed by the current Gizmos.matrix

        // Restore the original Gizmos.matrix for the arrow group before drawing the next part
        Gizmos.matrix = originalMatrix;
    }

    #endregion
}
