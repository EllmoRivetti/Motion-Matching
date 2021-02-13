using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System;
using MotionMatching.Matching;
using MotionMatching.Animation;
using Sirenix.OdinInspector;


namespace MotionMatching.Animation
{
    public class MM_Mover : MonoBehaviour
    {
        public Transform m_Destination;
        public AnimationController m_AnimationController;
        [Range(0, 50)] public int m_MotionMatchingFramesIntervalToUse = 10;
        [ShowInInspector] private int m_MMIntervalFramesRemaining = 0;

        private bool m_CalculatingMotionMatchingFrame = false;
        
        void Update()
        {
            if (!m_CalculatingMotionMatchingFrame && m_MMIntervalFramesRemaining == 0)
            {
                m_CalculatingMotionMatchingFrame = true;
                StartCoroutine(MoveUsingMotionMatching());
                m_CalculatingMotionMatchingFrame = false;
            }
        }

        private IEnumerator MoveUsingMotionMatching()
        {
            print("inside MoveUsingMotionMatching");
            m_MMIntervalFramesRemaining = m_MotionMatchingFramesIntervalToUse;

            Vector3 movementDestination = m_Destination.position;
            Vector3 characterMovement = movementDestination - transform.position;

            var frame = GetBestFrame(characterMovement);
            ApplyAnimationFrame(frame);

            yield return null;
        }

        private MocapFrameData GetBestFrame(Vector3 movement)
        {
            // Debug.Log("----------------------------");
            // Debug.Log("inside GetBestFrame");
            // Debug.Log(movement);

            float bestScore = -1;
            MocapFrameData bestFrame = null;

            foreach (var kvp in m_AnimationController.m_LoadedMocapFrameData.m_FrameData)
            {
                var frame = kvp.Value;
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
            Debug.Log("ApplyAnimationFrame (from:" + frameData.m_FrameNumber + "; interval: " + m_MMIntervalFramesRemaining + ")");
            m_AnimationController.RunNFramesFromFrame(
                m_MMIntervalFramesRemaining, 
                frameData.m_FrameNumber, 
                () => m_MMIntervalFramesRemaining = 0
            );
        }
    }
}   