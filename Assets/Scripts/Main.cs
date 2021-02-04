using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MotionMatching.Animation;

[RequireComponent(typeof(AnimationController))]
public class Main : RunnableCoroutine
{
	#region Members
	[Header("Animations data")]
	private AnimationController m_AnimationController;
	public UnityAnimationConverter m_AnimationConverter;
    public LoadedAnimationFile m_AnimationToUse;
	public string filename = "";
	#endregion

	private void Awake()
	{
		m_AnimationController = GetComponent<AnimationController>();
		if (!m_AnimationController) Debug.LogError("Please attach an AnimationController to this object", this);
	}

	private void Start()
	{
		s_OnUpdateEvents += TraceExecTime;
	}




	[Button]
	public void LoadAnimationFromFBXFile()
    {
		if (m_AnimationToUse)
        {
			m_AnimationToUse.m_FrameData = AnimationReader.GetFrameData(filename);
			print("Successfully loaded animation data");
		}

	}

	[Button]
	public void LoadAnimationFromUnity(string animationPath)
    {
		m_AnimationConverter.Create(animationPath);
		while (!m_AnimationConverter.CanRetrieveLoadedAnimationFile()) ;
		m_AnimationToUse = m_AnimationConverter.file;

		Debug.Log("Created LoadedAnimationFile at " + animationPath);
	}

	[Button]
	private void BindFrameData()
	{
		m_AnimationController.BindAnimationData(m_AnimationToUse.m_FrameData);
		// m_AnimationController.FixDefaultPosition();
		print("Successfully binded animation data to controller");
	}

	private float m_tmp_timeSinceStart = 0.0f;
	private void TraceExecTime(float deltaTime)
	{
		m_tmp_timeSinceStart += deltaTime;
		// Debug.Log("Executing since " + m_tmp_timeSinceStart + " ms");
	}

}
