using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System;
using MotionMatching.Matching;
using MotionMatching.Animation;

public class PathMover : MonoBehaviour
{
    public bool m_UseMotionMatching = true;
    public AnimationController m_AnimationController;
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
        {
            if (m_UseMotionMatching)
            {
                Vector3 movementDestination = pathPoints.Dequeue();
                Vector3 characterMovement = movementDestination - transform.position;

                var frame = GetBestFrame(characterMovement);
                ApplyAnimationFrame(frame);
            }
            else
            {
                agent.SetDestination(pathPoints.Dequeue());
            }
        }
    }

    private MocapFrameData GetBestFrame(Vector3 movement)
    {
        float bestScore = -1;
        MocapFrameData bestFrame = null;
        foreach(var kvp in m_AnimationController.m_LoadedMocapFrameData.m_FrameData)
        {
            var i_frame = kvp.Key;
            var frame = kvp.Value;
            var score = frame.GetFrameScore(new Vector2(movement.x, movement.z));

            if (bestScore < score || bestFrame == null)
            {
                bestScore = score;
                bestFrame = frame;
            }
        }
        return bestFrame;
    }

    private void ApplyAnimationFrame(MocapFrameData frameData)
    {
        m_AnimationController.SetBonesData(m_AnimationController.m_FrameData[frameData.m_FrameNumber]);
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
