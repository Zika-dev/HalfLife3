using UnityEngine;
using UnityEngine.AI;

public class PathFinder : MonoBehaviour
{
    public Transform PointA = null;
    public Transform PointB = null;

    private void Update()
    {
        if (PointA == null || PointB == null)
            return;

        NavMesh.SamplePosition(PointA.position, out NavMeshHit hitA, 10f, NavMesh.AllAreas);
        NavMesh.SamplePosition(PointB.position, out NavMeshHit hitB, 10f, NavMesh.AllAreas);

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path))
        {
            int cnt = path.corners.Length;

            float distance = 0f;
            for (int i = 0; i < cnt - 1; i++)
            {
                distance += (path.corners[i] - path.corners[i + 1]).magnitude;
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.green, 0.1f, true);
            }
            Debug.Log($"Total distance {distance:F2}");
        }
        else
        {
            Debug.LogError("Mission Fail");
        }
    }
}
