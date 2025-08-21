using UnityEngine;
using UnityEngine.UI;

public class UISliderMover : MonoBehaviour {
	[Header("Moving parts")]
	[SerializeField] RectTransform chevron;
	[SerializeField] RectTransform topLine;
	[SerializeField] RectTransform bottomLine;

	[SerializeField] float verticalOffset = 0f; // tweak until chevron sits right

	void Awake() {
		MakeNonBlocking(chevron);
		MakeNonBlocking(topLine);
		MakeNonBlocking(bottomLine);
	}

	static void MakeNonBlocking(RectTransform rt) {
		foreach (var g in rt.GetComponentsInChildren<Graphic>(true))
			g.raycastTarget = false;

		var cg = rt.GetComponent<CanvasGroup>();
		if (!cg) cg = rt.gameObject.AddComponent<CanvasGroup>();
		cg.blocksRaycasts = false;
	}

	public void MoveToTarget(RectTransform target) {
		// Use the pivot position directly since it's already at the top
		Vector3 localTargetPos = chevron.parent.InverseTransformPoint(target.position);

		// Apply optional offset
		localTargetPos.y += verticalOffset;

		// Calculate offsets dynamically
		float topOffsetY = topLine.localPosition.y - chevron.localPosition.y;
		float bottomOffsetY = bottomLine.localPosition.y - chevron.localPosition.y;

		// Move chevron
		Vector3 c = chevron.localPosition;
		c.y = localTargetPos.y;
		chevron.localPosition = c;

		// Keep lines aligned
		Vector3 t = topLine.localPosition;
		t.y = localTargetPos.y + topOffsetY;
		topLine.localPosition = t;

		Vector3 b = bottomLine.localPosition;
		b.y = localTargetPos.y + bottomOffsetY;
		bottomLine.localPosition = b;
	}
}
