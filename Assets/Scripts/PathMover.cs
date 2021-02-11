using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System;
using MotionMatching.Matching;
using MotionMatching.Animation;
using Sirenix.OdinInspector;

public class PathMover : MonoBehaviour
{
    public bool m_UseMotionMatching = true;
    public AnimationController m_AnimationController;
    [Range(0, 50)] public int m_MotionMatchingFramesIntervalToUse = 10;
    [ShowInInspector] private int m_MMIntervalFramesRemaining = 0;

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
        if (ShouldSetDestination() && !m_InsideUpdateAlready)
        {
            StartCoroutine(_Update());
        }
    }

    private bool m_InsideUpdateAlready = false;
    private IEnumerator _Update()
    {
        m_InsideUpdateAlready = true;

        if (m_UseMotionMatching && m_MMIntervalFramesRemaining == 0)
        {
            m_MMIntervalFramesRemaining = m_MotionMatchingFramesIntervalToUse;

            Vector3 movementDestination = pathPoints.Dequeue();
            Vector3 characterMovement = movementDestination - transform.position;

            var frame = GetBestFrame(characterMovement);
            print(frame.m_FrameNumber);
            ApplyAnimationFrame(frame);
        }

        agent.SetDestination(pathPoints.Dequeue());


        m_InsideUpdateAlready = false;
        yield return null;
    }

    private int m_GetBestFrame_ProgressBar_Max = 0;
    [ProgressBar(0, 116)]
    [ShowInInspector] private int m_GetBestFrame_ProgressBar_Current = 0;
    private MocapFrameData GetBestFrame(Vector3 movement)
    {
        // Debug.Log("----------------------------");
        // Debug.Log("inside GetBestFrame");
        // Debug.Log(movement);

        float bestScore = -1;
        MocapFrameData bestFrame = null;
        m_GetBestFrame_ProgressBar_Max = m_AnimationController.m_LoadedMocapFrameData.m_FrameData.Count - 1;

        foreach (var kvp in m_AnimationController.m_LoadedMocapFrameData.m_FrameData)
        {
            m_GetBestFrame_ProgressBar_Current = kvp.Key;
            var frame = kvp.Value;
            // print(m_GetBestFrame_ProgressBar_Current + " / " + (m_AnimationController.m_LoadedMocapFrameData.m_FrameData.Count - 1));

            var score = frame.GetFrameScore(new Vector2(movement.x, movement.z));

            if (bestScore < score || bestFrame == null)
            {
                bestScore = score;
                bestFrame = frame;
            }
        }
        // Debug.Log(bestScore);
        // Debug.Log(bestFrame);
        return bestFrame;
    }

    private void ApplyAnimationFrame(MocapFrameData frameData)
    {
        Debug.Log("inside ApplyAnimationFrame");
        m_AnimationController.Run(frameData.m_FrameNumber, m_MMIntervalFramesRemaining);
        // m_AnimationController.SetBonesData(m_AnimationController.m_FrameData[frameData.m_FrameNumber]);
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
