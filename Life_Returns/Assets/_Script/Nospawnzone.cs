using UnityEngine;

[System.Serializable]
public class NoSpawnZone : MonoBehaviour
{
    public enum ZoneShape
    {
        Circle,
        Box,
        Polygon
    }

    [Header("Zone Settings")]
    public ZoneShape shapeType = ZoneShape.Circle;

    [Header("Circle Settings")]
    [Tooltip("Radius for circular zones")]
    public float radius = 10f;

    [Header("Box Settings")]
    [Tooltip("Size for box zones")]
    public Vector3 boxSize = new Vector3(10f, 10f, 10f);

    [Header("Polygon Settings")]
    [Tooltip("Points defining polygon boundary (XZ plane). Must have at least 3 points.")]
    public Vector2[] polygonPoints = new Vector2[0];

    [Header("Visual Settings")]
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.3f);
    public Color gizmoWireColor = new Color(1f, 0.5f, 0f, 1f);
    public bool showGizmos = true;

    public bool IsPointInside(Vector3 worldPoint)
    {
        switch (shapeType)
        {
            case ZoneShape.Circle:
                return IsPointInCircle(worldPoint);

            case ZoneShape.Box:
                return IsPointInBox(worldPoint);

            case ZoneShape.Polygon:
                return IsPointInPolygon(worldPoint);

            default:
                return false;
        }
    }

    bool IsPointInCircle(Vector3 worldPoint)
    {
        Vector2 point2D = new Vector2(worldPoint.x, worldPoint.z);
        Vector2 center2D = new Vector2(transform.position.x, transform.position.z);

        return Vector2.Distance(point2D, center2D) <= radius;
    }

    bool IsPointInBox(Vector3 worldPoint)
    {
        // Convert world point to local space
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

        // Check if point is within box bounds
        return Mathf.Abs(localPoint.x) <= boxSize.x / 2f &&
               Mathf.Abs(localPoint.z) <= boxSize.z / 2f;
    }

    bool IsPointInPolygon(Vector3 worldPoint)
    {
        if (polygonPoints == null || polygonPoints.Length < 3)
            return false;

        // Convert world point to local 2D
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        Vector2 point = new Vector2(localPoint.x, localPoint.z);

        // Ray casting algorithm for point-in-polygon test
        bool inside = false;
        int j = polygonPoints.Length - 1;

        for (int i = 0; i < polygonPoints.Length; i++)
        {
            if ((polygonPoints[i].y > point.y) != (polygonPoints[j].y > point.y) &&
                point.x < (polygonPoints[j].x - polygonPoints[i].x) * (point.y - polygonPoints[i].y) /
                (polygonPoints[j].y - polygonPoints[i].y) + polygonPoints[i].x)
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    public void DrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = gizmoWireColor;

        switch (shapeType)
        {
            case ZoneShape.Circle:
                DrawCircleGizmo();
                break;

            case ZoneShape.Box:
                DrawBoxGizmo();
                break;

            case ZoneShape.Polygon:
                DrawPolygonGizmo();
                break;
        }
    }

    void DrawCircleGizmo()
    {
        int segments = 64;
        float angleStep = 360f / segments;
        Vector3 prevPoint = transform.position + transform.right * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Vector3 newPoint = transform.position + offset;

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }

        // Draw filled circle
        Gizmos.color = gizmoColor;
        DrawFilledCircle(transform.position, radius, segments);
    }

    void DrawFilledCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);

            // Draw triangle from center to edge
            Gizmos.DrawLine(center, point1);
            Gizmos.DrawLine(point1, point2);
        }
    }

    void DrawBoxGizmo()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        // Draw wireframe
        Gizmos.color = gizmoWireColor;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boxSize.x, 0.1f, boxSize.z));

        // Draw filled (projected to XZ plane)
        Gizmos.color = gizmoColor;

        Vector3 halfSize = boxSize / 2f;
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-halfSize.x, 0, -halfSize.z),
            new Vector3(halfSize.x, 0, -halfSize.z),
            new Vector3(halfSize.x, 0, halfSize.z),
            new Vector3(-halfSize.x, 0, halfSize.z)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % corners.Length]);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    void DrawPolygonGizmo()
    {
        if (polygonPoints == null || polygonPoints.Length < 3)
            return;

        Gizmos.matrix = transform.localToWorldMatrix;

        // Draw polygon outline
        Gizmos.color = gizmoWireColor;
        for (int i = 0; i < polygonPoints.Length; i++)
        {
            Vector3 p1 = new Vector3(polygonPoints[i].x, 0, polygonPoints[i].y);
            Vector3 p2 = new Vector3(polygonPoints[(i + 1) % polygonPoints.Length].x, 0,
                                    polygonPoints[(i + 1) % polygonPoints.Length].y);
            Gizmos.DrawLine(p1, p2);
        }

        // Draw filled area
        Gizmos.color = gizmoColor;
        for (int i = 1; i < polygonPoints.Length - 1; i++)
        {
            Vector3 p0 = new Vector3(polygonPoints[0].x, 0, polygonPoints[0].y);
            Vector3 p1 = new Vector3(polygonPoints[i].x, 0, polygonPoints[i].y);
            Vector3 p2 = new Vector3(polygonPoints[i + 1].x, 0, polygonPoints[i + 1].y);

            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p0);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    void OnDrawGizmos()
    {
        DrawGizmos();
    }

    // Helper method to create a simple rectangular polygon
    public void CreateRectangularPolygon(float width, float height)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        polygonPoints = new Vector2[]
        {
            new Vector2(-halfWidth, -halfHeight),
            new Vector2(halfWidth, -halfHeight),
            new Vector2(halfWidth, halfHeight),
            new Vector2(-halfWidth, halfHeight)
        };
    }

    // Helper method to create a simple triangular polygon
    public void CreateTriangularPolygon(float size)
    {
        float height = size * Mathf.Sqrt(3) / 2f;

        polygonPoints = new Vector2[]
        {
            new Vector2(0, height * 2f / 3f),
            new Vector2(-size / 2f, -height / 3f),
            new Vector2(size / 2f, -height / 3f)
        };
    }
}