using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MotionMatching.Animation;

[RequireComponent(typeof(AnimationReader))]
[RequireComponent(typeof(AnimationController))]
public class Main : RunnableCoroutine
{
	#region Members
	[Header("Animations data")]
	public AnimationClipBank m_AnimationBank;
	private AnimationReader m_AnimationReader;
	private AnimationController m_AnimationController;
    [ShowInInspector]
    private GameObject m_Character;
	#endregion

	private void Awake()
	{
		m_AnimationReader = GetComponent<AnimationReader>();
		if (!m_AnimationReader) Debug.LogError("Please attach an AnimationReader to this object", this);

		m_AnimationController = GetComponent<AnimationController>();
		if (!m_AnimationController) Debug.LogError("Please attach an AnimationController to this object", this);

	}

	private void Start()
	{
		s_OnUpdateEvents += TraceExecTime;
		InitFrameData();
	}

	private void InitFrameData()
	{
		var node = m_AnimationReader.ReadFile("Assets/Animations/01_09.fbx");
		var animationData = m_AnimationReader.GetFrameData(node.GetNodeKey("Takes")[0].GetNodeKey("Take")[0]);
		print(animationData);
		m_AnimationController.BindAnimationData(animationData);
		print("Successfully binded animation data");
	}

	private float m_tmp_timeSinceStart = 0.0f;
	private void TraceExecTime(float deltaTime)
	{
		m_tmp_timeSinceStart += deltaTime;
		// Debug.Log("Executing since " + m_tmp_timeSinceStart + " ms");
	}

}
