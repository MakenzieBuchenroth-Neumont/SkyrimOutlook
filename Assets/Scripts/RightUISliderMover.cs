using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RightUISliderMover : MonoBehaviour {
	[Header("Moving parts")]
	[SerializeField] RectTransform chevron;
	[SerializeField] RectTransform topLine;
	[SerializeField] RectTransform bottomLine;
	[SerializeField] RectTransform highlight;

	[SerializeField] float verticalOffset = 475f; // tweak until chevron sits right
	[SerializeField] float highlightOffset = 140f;

	public void Initialize(RectTransform chevron, RectTransform topLine, RectTransform bottomLine, RectTransform highlight) {
		this.chevron = chevron;
		this.topLine = topLine;
		this.bottomLine = bottomLine;
		this.highlight = highlight;

		// Setup once initialized
		MakeNonBlocking(chevron);
		MakeNonBlocking(topLine);
		MakeNonBlocking(bottomLine);
		MakeNonBlocking(highlight);
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

		// Move highlight
		Vector3 h = highlight.localPosition;
		h.y = localTargetPos.y + highlightOffset;
		highlight.localPosition = h;
	}
}
