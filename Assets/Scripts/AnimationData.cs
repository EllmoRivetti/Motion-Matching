using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AnimationData : ScriptableObject
{
	public string m_AnimationClipFileName;
	public AnimationClip m_AnimationClip;
	public List<FrameData> m_FrameData;

	/**
	 * Creates frame data implicitly
	 **/
	public AnimationData(string m_AnimationClipFileName, AnimationClip m_AnimationClip)
	{
		this.m_AnimationClipFileName = m_AnimationClipFileName;
		this.m_AnimationClip = m_AnimationClip;
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
