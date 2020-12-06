using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationClipBank", menuName = "MotionMaching/AnimationClipBank")]
public class AnimationClipBank : ScriptableObject
{
	public List<AnimationClip> m_Animations;
}