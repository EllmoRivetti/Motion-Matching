using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MotionMatching.Animation;

public class AnimationData : ScriptableObject
{
	public string m_AnimationClipFileName;
	public AnimationController m_AnimationController;
	public List<FrameData> m_FrameData;

	/**
	 * Creates frame data implicitly
	 **/
	public AnimationData(string m_AnimationClipFileName, AnimationController m_AnimationController)
	{
		this.m_AnimationClipFileName = m_AnimationClipFileName;
		this.m_AnimationController = m_AnimationController;
		InitFrameData();
	}

	private void InitFrameData()
	{
		/*
		foreach (var frame in m_AnimationClip)
		{
			float frameTime = 0;
			Vector2 positionHipProjection = Vector2.zero;
			Vector2 positionFuturHipProjection = Vector2.zero;
			Vector3 rightFeetPosition = Vector3.zero, leftFeetPosition = Vector3.zero;
			FrameData.FeetPositions positionFeet = new FrameData.FeetPositions(rightFeetPosition, leftFeetPosition);
			FrameData frameData = new FrameData(
				frameTime, 
				positionHipProjection, 
				positionFuturHipProjection, 
				positionFeet
			);
		}
		*/
	}
}
