using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationBank", menuName = "MotionMaching/AnimationBank")]
public class AnimationBank : ScriptableObject
{
	public List<AnimationClip> m_Animations;
}