using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Testing : MonoBehaviour
{
    [SerializeField] private PathfindingVisual pathfindingVisual;
    [SerializeField] private PathfindingDebugStepVisual pathfindingDebugStepVisual;
    [SerializeField] private CharacterPathfindingMovementHandler characterPathfinding;


    private Pathfinding pathfinding;
    private Vector3 curStart;

    private void Start()
    {
        pathfinding = new Pathfinding(10, 10);
        pathfindingVisual.SetGrid(pathfinding.GetGrid());
        pathfindingDebugStepVisual.Setup(pathfinding.GetGrid());


    }

    private void Update()
    {

        Vector3 clickPosition = -Vector3.one;
        Plane plane = new Plane(Vector3.up, 0f);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distanceToPlane;
        if (plane.Raycast(ray, out distanceToPlane))
        {
            clickPosition = ray.GetPoint(distanceToPlane);
        }

        if (Input.GetMouseButtonDown(0))
        {
//            Debug.Log(clickPosition);
            pathfinding.GetGrid().GetXZ(clickPosition, out int x, out int z);
            List<PathNode> path = pathfinding.FindPath((int)curStart.x, (int)curStart.z, x, z);
            curStart = new Vector3(path[path.Count - 1].x, 0, path[path.Count - 1].z);
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(new Vector3(path[i].x, 0, path[i].z) * 10f + Vector3.one * 5f, new Vector3(path[i + 1].x, 0, path[i + 1].z) * 10f + Vector3.one * 5f, Color.white, 15f);
                }
            }
            characterPathfinding.SetTargetPosition(clickPosition);
        }

        if (Input.GetMouseButtonDown(1))
        {
            pathfinding.GetGrid().GetXZ(clickPosition, out int x, out int z);

            pathfinding.GetNode(x, z).SetIsWalkable(!pathfinding.GetNode(x, z).isWalkable);
        }
    }
}
