using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class RunnableCoroutine : SerializedMonoBehaviour
{
	#region OnUpdateDelegate
	public delegate void OnUpdateDelegate(float deltaTime);
	public static OnUpdateDelegate s_OnUpdateEvents;
	#endregion

	#region Members
	[Header("Coroutine parameters")]
	public float m_UpdateDeltaTime = 0.5f;
	public bool m_Run = true;
	public bool m_Pause = false;
	
	protected IEnumerator m_CR;
	protected bool m_CRisRunning = false;
	#endregion

	#region UpdateCoroutine
	protected IEnumerator UpdateCR()
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

	protected void RunCoroutine()
	{
		if (!m_CRisRunning)
		{
			m_CR = UpdateCR();
			StartCoroutine(m_CR);
			m_CRisRunning = true;
		}
	}
	protected void StopCoroutine()
	{
		try
		{
			StopCoroutine(m_CR);
			m_CR = null;
			m_CRisRunning = false;
		}
		catch (UnityException e)
		{
			Debug.LogWarning(e.Message);
		}
	}

	protected void OnValidate()
	{
		if (m_Run)
		{
			RunCoroutine();
		}
		else
		{
			StopCoroutine();
		}
	}
	#endregion
}
