using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TestOdinInspectorImport : MonoBehaviour
{
	[Button]
    public void TestImport(string message)
	{
		print(message);
	}
}
