using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Components;
using System.Collections;

public class RefreshNavmesh : MonoBehaviour
{
    public NavMeshSurface Surface2D;
    public float updateInterval = 0.1f;

    private void Start()
    {
        StartCoroutine(UpdateNavMeshPeriodically());
    }

    private IEnumerator UpdateNavMeshPeriodically()
    {
        while (true)
        {
            Surface2D.UpdateNavMesh(Surface2D.navMeshData);
            yield return new WaitForSeconds(updateInterval);
        }
    }
}
