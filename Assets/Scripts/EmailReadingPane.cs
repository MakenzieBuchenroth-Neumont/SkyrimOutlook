using TMPro;
using UnityEngine;

/// <summary>
/// Controls the right-hand reading pane
/// </summary>
public class EmailReadingPane : MonoBehaviour {
	public static EmailReadingPane Instance;

	[Header("UI References")]
	public TextMeshProUGUI fromField;
	public TextMeshProUGUI toField;
	public TextMeshProUGUI subjectField;
	public TextMeshProUGUI dateField;
	public TextMeshProUGUI bodyField;

	void Awake() {
		Instance = this;
		gameObject.SetActive(false);
	}

	public void DisplayEmail(EmailData email) {
		fromField.SetText(email.From);
		toField.SetText(email.To);
		subjectField.SetText(email.Subject);
		dateField.SetText(email.Date);
		bodyField.SetText(email.Body);

		gameObject.SetActive(true);
	}
}