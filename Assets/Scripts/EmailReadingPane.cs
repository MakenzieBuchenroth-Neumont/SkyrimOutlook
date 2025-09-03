using System.Diagnostics.Contracts;
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

	public RectTransform webViewParent;

	//private WebViewObject webView;

	void Awake() {
		if (Instance == null) Instance = this;
	}

	public void DisplayEmail(EmailData data) {
		// Update header info
		fromField.text = data.From;
		subjectField.text = data.Subject;
		toField.text = data.To != null ? string.Join(", ", data.To) : "";
		dateField.text = data.Date;

		// Decide whether to use WebView or TMP
		if (data.BodyContentType == "html") {
			//ShowHtmlEmail(data.BodyContent);
		}
		else {
			ShowPlainEmail(data.BodyContent);
		}
	}

	private void ShowPlainEmail(string body) {
		// Enable TMP, disable WebView
		bodyField.gameObject.SetActive(true);

		/*if (webView != null) {
			webView.SetVisibility(false);
		}*/

		bodyField.text = body;
	}

	/*private void ShowHtmlEmail(string html) {
		// Hide TMP, show WebView
		bodyField.gameObject.SetActive(false);

		if (webView == null) {
			// Create the WebViewObject at runtime if it doesn't exist yet
			webView = new GameObject("EmailWebView").AddComponent<WebViewObject>();
			webView.Init();

			// Position it over the parent panel
			webView.transform.SetParent(webViewParent, false);
			webView.SetMargins(0, 0, 0, 0); // fit to parent
		}

		webView.SetVisibility(true);

		// Load HTML into the WebView
		// (baseUrl is needed for resolving inline resources sometimes)
		webView.LoadHTML(html, "https://outlook.office.com/");
	}*/
}