using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoxRenderer : MonoBehaviour
{
    public float width = 1f; // Box width
    public float height = 1f; // Box height
    public float depth = 1f; // Box depth

    private LineRenderer lineRenderer;

    void Start()
    {
        // Initialize the LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 8; // Box corners
        lineRenderer.loop = true; // Make it a closed loop
        lineRenderer.useWorldSpace = false; // Relative to GameObject's position
        DrawBox();
    }

    void DrawBox()
    {
        // Calculate corner points
        Vector3[] corners = new Vector3[8];

        // Front face
        corners[0] = new Vector3(-width / 2, -height / 2, depth / 2);
        corners[1] = new Vector3(width / 2, -height / 2, depth / 2);
        corners[2] = new Vector3(width / 2, height / 2, depth / 2);
        corners[3] = new Vector3(-width / 2, height / 2, depth / 2);

        // Back face
        corners[4] = new Vector3(-width / 2, -height / 2, -depth / 2);
        corners[5] = new Vector3(width / 2, -height / 2, -depth / 2);
        corners[6] = new Vector3(width / 2, height / 2, -depth / 2);
        corners[7] = new Vector3(-width / 2, height / 2, -depth / 2);

        // Set positions for the LineRenderer
        lineRenderer.positionCount = 16;

        // Draw front face
        lineRenderer.SetPosition(0, corners[0]);
        lineRenderer.SetPosition(1, corners[1]);
        lineRenderer.SetPosition(2, corners[2]);
        lineRenderer.SetPosition(3, corners[3]);
        lineRenderer.SetPosition(4, corners[0]);

        // Draw connections between front and back faces
        lineRenderer.SetPosition(5, corners[4]);
        lineRenderer.SetPosition(6, corners[5]);
        lineRenderer.SetPosition(7, corners[6]);
        lineRenderer.SetPosition(8, corners[7]);

        // Complete the back face
        lineRenderer.SetPosition(9, corners[4]);
        lineRenderer.SetPosition(10, corners[5]);
        lineRenderer.SetPosition(11, corners[6]);
        lineRenderer.SetPosition(12, corners[7]);
        lineRenderer.SetPosition(13, corners[7]);
        lineRenderer.SetPosition(14, corners[7]);
    }

    public void UpdateBoxSize(float newWidth, float newHeight, float newDepth)
    {
        // Update dimensions and redraw the box
        width = newWidth;
        height = newHeight;
        depth = newDepth;
        DrawBox();
    }
}
