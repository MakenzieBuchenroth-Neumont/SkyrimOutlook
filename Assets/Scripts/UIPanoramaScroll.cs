using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UIPanoramaScroll : MonoBehaviour {
	[Header("Panorama Setup")]
	public RectTransform panoramaImage;
	private Image imageComponent;

	[Header("Panorama Sprites")]
	public Sprite[] panoramaSprites;

	[Header("Scroll Settings")]
	public float scrollSpeed = 200f;
	public float pauseTime = 2f;

	[Header("TextMeshProUGUI Contrast")]
	public List<TextMeshProUGUI> contrastTexts = new List<TextMeshProUGUI>(); // Drag texts here in Inspector

	private float leftX, rightX;
	private Color currentTextColor = Color.white;

	private void Start() {
		imageComponent = panoramaImage.GetComponent<Image>();

		// Pick a random sprite
		if (panoramaSprites != null && panoramaSprites.Length > 0) {
			int index = Random.Range(0, panoramaSprites.Length);
			imageComponent.sprite = panoramaSprites[index];
			UpdateTextContrast();
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(panoramaImage);
		CalculateBounds();

		StartCoroutine(PanoramaLoop());
	}

	private void CalculateBounds() {
		float screenWidth = ((RectTransform)panoramaImage.parent).rect.width;
		float imageWidth = panoramaImage.rect.width;
		float halfDiff = (imageWidth - screenWidth) * 0.5f;

		leftX = halfDiff;
		rightX = -halfDiff;
	}

	private void UpdateTextContrast() {
		Texture tex = imageComponent.mainTexture;
		float avg = 1f;

		if (tex != null) {
			// Simple fallback: use texture size midpoint color if readable
			try {
				Texture2D readable = tex as Texture2D;
				if (readable != null && readable.isReadable) {
					Color c = readable.GetPixel(readable.width / 2, readable.height / 2);
					avg = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
				}
			}
			catch { avg = 1f; }
		}

		currentTextColor = (avg > 0.5f) ? Color.black : Color.white;
		ApplyTextColors();
	}

	private void ApplyTextColors() {
		foreach (TextMeshProUGUI t in contrastTexts) {
			if (t != null)
				t.color = currentTextColor;
		}
	}

	/// <summary>
	/// Call this when you spawn a prefab that has TextMeshProUGUI components.
	/// Adds them to the list and applies current color.
	/// </summary>
	public void RegisterTextsFrom(GameObject obj) {
		foreach (TextMeshProUGUI t in obj.GetComponentsInChildren<TextMeshProUGUI>(true)) {
			if (!contrastTexts.Contains(t))
				contrastTexts.Add(t);

			t.color = currentTextColor;
		}
	}

	IEnumerator PanoramaLoop() {
		while (true) {
			yield return StartCoroutine(MoveImage(rightX));
			yield return new WaitForSeconds(pauseTime);

			yield return StartCoroutine(MoveImage(leftX));
			yield return new WaitForSeconds(pauseTime);
		}
	}

	IEnumerator MoveImage(float targetX) {
		Vector2 pos = panoramaImage.anchoredPosition;
		while (Mathf.Abs(panoramaImage.anchoredPosition.x - targetX) > 0.5f) {
			pos.x = Mathf.MoveTowards(
				panoramaImage.anchoredPosition.x,
				targetX,
				scrollSpeed * Time.deltaTime
			);

			panoramaImage.anchoredPosition = pos;
			yield return null;
		}
	}
}
