using UnityEngine;

public class SceneRefs : MonoBehaviour {
	public static SceneRefs Instance { get; private set; }

	public RectTransform chevron;
	public RectTransform topLine;
	public RectTransform bottomLine;
	public RectTransform highlight;

	void Awake() => Instance = this;
}
