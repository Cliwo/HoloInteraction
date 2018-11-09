using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugExtension : MonoBehaviour{
	public static TextMesh text;
	public TextMesh target;
	void Start()
	{
		text = target;
	}
	public static void DebugLog(object message)
	{
		text.text = message.ToString();
		Debug.Log(message);
	}

	public static void DebugError(object message)
	{
		text.text = message.ToString();
		Debug.LogError(message);
	}
}

