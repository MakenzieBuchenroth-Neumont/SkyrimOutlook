using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to each message prefab instance
/// </summary>
public class MessageUI : MonoBehaviour {
	private EmailData emailData;

	public void SetData(EmailData data) {
		emailData = data;

		// update inbox preview fields
		transform.Find("From/From (1)")?.GetComponent<TextMeshProUGUI>().SetText(data.From);
		transform.Find("Subject")?.GetComponent<TextMeshProUGUI>().SetText(data.Subject);
		transform.Find("DateSent")?.GetComponent<TextMeshProUGUI>().SetText(data.Date);

		// hook up highlight + reading pane open
		Button btn = GetComponent<Button>();
		if (btn != null) {
			btn.onClick.AddListener(() => {
				// highlight
				var mover = gameObject.AddComponent<RightUISliderMover>();
				mover.Initialize(
					SceneRefs.Instance.chevron,
					SceneRefs.Instance.topLine,
					SceneRefs.Instance.bottomLine,
					SceneRefs.Instance.highlight
				);

				mover.MoveToTarget(GetComponent<RectTransform>());

				// open reading pane
				EmailReadingPane.Instance.DisplayEmail(emailData);
			});
		}
	}
}