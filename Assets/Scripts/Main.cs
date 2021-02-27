using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MotionMatching.Animation;
using MotionMatching.Matching;

[RequireComponent(typeof(AnimationController))]
public class Main : SerializedMonoBehaviour
{
    public LoadedAnimationFile m_AnimationToUse;
	private AnimationController m_AnimationController;

	private void Awake()
	{
		m_AnimationController = GetComponent<AnimationController>();
		if (!m_AnimationController) Debug.LogError("Please attach an AnimationController to this object", this);
	}

	private void Start()
	{
		BindFrameData();
	}

	[Button]
	private void BindFrameData()
	{
	}

}
