using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Attached to each message prefab instance
/// </summary>
public class MessageUI : MonoBehaviour {
	private EmailData emailData;
	private string accessToken;

	[SerializeField] RightUISliderMover mover;

	public void SetData(EmailData data, string token, RightUISliderMover m) {
		emailData = data;
		accessToken = token;
		mover = m;

		// update inbox preview fields
		transform.Find("From/From (1)")?.GetComponent<TextMeshProUGUI>().SetText(data.From);
		transform.Find("Subject")?.GetComponent<TextMeshProUGUI>().SetText(data.Subject);
		transform.Find("DateSent")?.GetComponent<TextMeshProUGUI>().SetText(data.Date);

		// cache mover if one exists
		mover = GetComponent<RightUISliderMover>();

		// hook up click
		var btn = GetComponent<Button>();
		if (btn != null) btn.onClick.AddListener(OnClick);
	}

	private async void OnClick() {
		// move highlight bars
		if (mover != null) {
			mover.MoveToTarget(GetComponent<RectTransform>());
		}

		// load full message
		await LoadFullMessage();
	}

	private async Task LoadFullMessage() {
		string url = $"https://graph.microsoft.com/v1.0/me/messages/{emailData.Id}";

		UnityWebRequest request = UnityWebRequest.Get(url);
		request.SetRequestHeader("Authorization", "Bearer " + accessToken);

		var op = request.SendWebRequest();
		while (!op.isDone) await Task.Yield();

		if (request.result != UnityWebRequest.Result.Success) {
			Debug.LogError("Graph API error: " + request.error);
			return;
		}

		JObject msg = JObject.Parse(request.downloadHandler.text);
		string body = msg["body"]?["content"]?.ToString() ?? "(empty body";
		string contentType = msg["body"]?["contentType"]?.ToString();

		if (contentType == "html") {
			body = CleanHTMLForTMP(body);
		}

		emailData.BodyContent = body;

		// Send to the reading pane
		EmailReadingPane.Instance.DisplayEmail(emailData);
	}

	private string CleanHTMLForTMP(string html) {
		if (string.IsNullOrEmpty(html)) return string.Empty;

		// Remove comments like <!-- ... -->
		html = Regex.Replace(html, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

		// Remove <style>...</style>, <script>...</script>, <head>...</head>
		html = Regex.Replace(html, @"<style.*?>.*?</style>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
		html = Regex.Replace(html, @"<script.*?>.*?</script>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
		html = Regex.Replace(html, @"<head.*?>.*?</head>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

		// Convert line breaks
		html = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
		html = Regex.Replace(html, @"</p>", "\n\n", RegexOptions.IgnoreCase);

		// Keep TMP-supported tags: <b>, <i>, <u>, <color=...>
		string allowedTagsPattern = @"</?(b|i|u|color.*?)*?>";
		string stripped = Regex.Replace(html, allowedTagsPattern, m => m.Value, RegexOptions.IgnoreCase);

		// Remove everything else
		stripped = Regex.Replace(stripped, "<.*?>", string.Empty);

		// Decode HTML entities (&nbsp; → space, &amp; → & etc.)
		stripped = System.Net.WebUtility.HtmlDecode(stripped);

		return stripped.Trim();
	}
}