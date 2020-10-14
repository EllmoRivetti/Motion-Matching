using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System;

public class PathMover : MonoBehaviour
{
    private NavMeshAgent agent;
    private Queue<Vector3> pathPoints = new Queue<Vector3>();

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        FindObjectOfType<PathCreator>().OnNewPathCreated += SetPoints;
    }

    private void SetPoints(IEnumerable<Vector3> points)
    {
        pathPoints = new Queue<Vector3>(points);
    }

    private void Update()
    {
        if (ShouldSetDestination())
            agent.SetDestination(pathPoints.Dequeue());
    }

    private bool ShouldSetDestination()
    {
        if (pathPoints.Count == 0)
            return false;

        if (agent.hasPath == false || agent.remainingDistance < 0.5f)
            return true;

        return false;
    }
}
