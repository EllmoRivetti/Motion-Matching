using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Main : SerializedMonoBehaviour
{
	#region OnUpdateDelegate
	public delegate void OnUpdateDelegate(float deltaTime);
	public static OnUpdateDelegate s_OnUpdateEvents;
	#endregion

	#region Members
	[Header("Update Coroutine")]
	public float m_UpdateDeltaTime = 0.5f;
	public bool m_Run = true;
	public bool m_Pause = false;


	private IEnumerator m_UpdateCR;
	private bool m_UpdateCR_isRunning = false;

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


	#region UpdateCoroutine
	private IEnumerator UpdateCR()
	{
		float futurDeltaTimeCorrection = 0.0f;
		while (true)
		{
			float start = Time.time;
			yield return new WaitForSeconds(m_UpdateDeltaTime + futurDeltaTimeCorrection);
			float end = Time.time;
			float deltaTime = end - start;
			futurDeltaTimeCorrection = deltaTime - m_UpdateDeltaTime;

			if (!m_Pause)
			{
				// Add here any methods to be called on update
				s_OnUpdateEvents?.Invoke(deltaTime);
			}
		}
	}
	
	private void RunUpdateCoroutine()
	{
		if (!m_UpdateCR_isRunning)
		{
			m_UpdateCR = UpdateCR();
			StartCoroutine(m_UpdateCR);
			m_UpdateCR_isRunning = true;
		}
	}
	private void StopUpdateCoroutine()
	{
		try
		{
			StopCoroutine(m_UpdateCR);
			m_UpdateCR = null;
			m_UpdateCR_isRunning = false;
		}
		catch (UnityException e)
		{
			Debug.LogWarning(e.Message);
		}
	}

	private void OnValidate()
	{
		if (m_Run)
		{
			RunUpdateCoroutine();
		}
		else
		{
			StopUpdateCoroutine();
		}
	}
	#endregion
}
