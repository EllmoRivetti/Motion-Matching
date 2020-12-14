using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Main : RunnableCoroutine
{
	#region Members
	[Header("Animations data")]
	public AnimationClipBank m_AnimationBank;
	#endregion

	private void Start()
	{
		s_OnUpdateEvents += TraceExecTime;
		InitFrameData();
	}

	private void InitFrameData()
	{
		
	}


	private float m_tmp_timeSinceStart = 0.0f;
	private void TraceExecTime(float deltaTime)
	{
		m_tmp_timeSinceStart += deltaTime;
		Debug.Log("Executing since " + m_tmp_timeSinceStart + " ms");
	}
}
