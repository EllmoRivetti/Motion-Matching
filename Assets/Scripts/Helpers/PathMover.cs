using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathMover : MonoBehaviour
{
    #region Members
    private NavMeshAgent m_Agent;
    private Queue<Vector3> m_PathPoints = new Queue<Vector3>();
    #endregion

    #region Initialisation
    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        FindObjectOfType<PathCreator>().m_OnNewPathCreated += SetPoints;
    }
    private void SetPoints(IEnumerable<Vector3> points)
    {
        m_PathPoints = new Queue<Vector3>(points);
    }
    #endregion
    #region Update
    private void Update()
    {
        if (ShouldSetDestination())
        {
            m_Agent.SetDestination(m_PathPoints.Dequeue());
        }
    }
    private bool ShouldSetDestination()
    {
        if (m_PathPoints.Count == 0)
            return false;

        if (m_Agent.hasPath == false || m_Agent.remainingDistance < 0.5f)
            return true;

        return false;
    }
    #endregion
}
