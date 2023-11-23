using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using AISystem.Contracts;
using AISystem.Pathing;

namespace AISystem.Systems
{
    public class AIManager : IManager
    {
        List<IBeing> m_beings = new();
        NavMeshPath m_navMeshPath = new();
        Vector3[] m_navMeshCorners = new Vector3[64];

        public void ResetEnemies()
        {            
            foreach (IBeing enemy in m_beings)
            {                
             if(enemy is Enemy)
                {
                    Enemy e = (Enemy)enemy;
                    e.ResetToPosition();
                }
            }
        }

        public void RegisterBeing(IBeing being) => m_beings.Add(being);

        public void DeregisterBeing(IBeing being) => m_beings.Remove(being);
        
        public IBeing[] GetCloseBeings(float distance, Vector3 position)
        {
            float sqrDist = distance * distance;
            return m_beings.Where(e => Vector3.SqrMagnitude(e.m_position - position) <= sqrDist).ToArray();
        }

        public bool CanSeeBeing(IBeing being, Vector3 origin, Vector3 direction, float viewDistance, LayerMask opticMask)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, viewDistance, opticMask, QueryTriggerInteraction.Ignore))
            {
                var hitPlayer = hit.collider.GetComponentInParent<IBeing>();
                if (hitPlayer == being)
                {
                    return true;
                }
            }
            return false;
        }

        public Path GeneratePath(PathRequest request)
        {
            if (NavMesh.CalculatePath(request.m_origin, request.m_destination, UnityEngine.AI.NavMesh.AllAreas, m_navMeshPath) == false)
            {
                Debug.LogWarning("Could not generate path");
                return Path.Empty;
            }

            int numCorners = m_navMeshPath.GetCornersNonAlloc(m_navMeshCorners);

            Path path = PathGenerator.GetPath(
                m_navMeshCorners.Take(numCorners).ToArray(),
                request.m_originDirection,
                request.m_destinationDirection
            );
            return path;
        }       
    }
}