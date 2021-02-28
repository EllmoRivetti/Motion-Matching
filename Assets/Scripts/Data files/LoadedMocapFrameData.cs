using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MotionMatching.Matching
{
    [CreateAssetMenu(fileName = "MotionMatching / Mocap Frame Data", menuName = "MotionMatching / Mocap Frame Data", order = 3)]
    public class LoadedMocapFrameData : SerializedScriptableObject
    {
        public SortedDictionary<int, MocapFrameData> m_FrameData;
    }
}
