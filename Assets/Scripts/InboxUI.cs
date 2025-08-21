using UnityEngine;
using Microsoft.Identity.Client;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System.Runtime.Serialization;

public class InboxUI : MonoBehaviour {
	[Header("MSAL Config")]
	[SerializeField] private string clientId = "16527e35-0698-46b3-a890-562410a382ed";
	private string[] scopes = new string[] { "User.Read", "Mail.Read" };
	private IPublicClientApplication _pca;

	[Header("UI Prefabs/Parents")]
	public GameObject messagePrefab; // Your Message prefab
	public Transform contentParent;   // The ScrollView Content object
	[SerializeField] private TextMeshProUGUI emailName;

	private string userEmail;
	private string cacheFilePath;

	async void Start() {
		cacheFilePath = System.IO.Path.Combine(Application.persistentDataPath, "msal_cache.json");
		_pca = PublicClientApplicationBuilder.Create(clientId)
			.WithRedirectUri("http://localhost")
			.Build();

		ConfigureTokenCache(_pca.UserTokenCache);

		string token = await SignInAndGetTokenAsync();
		if (!string.IsNullOrEmpty(token)) {
			userEmail = await GetUserEmail(token);
			if (emailName != null) {
				emailName.text = userEmail;
			}
			await FetchInbox(token);
		}
	}

	private void ConfigureTokenCache(ITokenCache tokenCache) {
		tokenCache.SetBeforeAccess(args => {
			if (System.IO.File.Exists(cacheFilePath)) {
				byte[] data = System.IO.File.ReadAllBytes(cacheFilePath);
				args.TokenCache.DeserializeMsalV3(data);
			}
		});

		tokenCache.SetAfterAccess(args => {
			if (args.HasStateChanged) {
				byte[] data = args.TokenCache.SerializeMsalV3();
				System.IO.File.WriteAllBytes(cacheFilePath, data);
			}
		});
	}

	private async Task<string> SignInAndGetTokenAsync() {
		try {
			var accounts = await _pca.GetAccountsAsync();
			var result = await _pca.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
				.ExecuteAsync();
			return result.AccessToken;
		}
		catch (MsalUiRequiredException) {
			var result = await _pca.AcquireTokenInteractive(scopes)
				.WithPrompt(Prompt.SelectAccount)
				.ExecuteAsync();
			return result.AccessToken;
		}
	}

	private async Task<string> GetUserEmail(string accessToken) {
		string url = "https://graph.microsoft.com/v1.0/me?$select=mail,userPrincipalName";
		UnityWebRequest request = UnityWebRequest.Get(url);
		request.SetRequestHeader("Authorization", "Bearer " + accessToken);

		var op = request.SendWebRequest();
		while (!op.isDone)
			await Task.Yield();

		if (request.result != UnityWebRequest.Result.Success) {
			Debug.LogError("Graph API error: " + request.error);
			return null;
		}

		var json = JObject.Parse(request.downloadHandler.text);
		string mail = json["mail"]?.ToString();
		if (string.IsNullOrEmpty(mail))
			mail = json["userPrincipalName"]?.ToString();

		return mail;
	}


	private async Task FetchInbox(string accessToken) {
		string url = "https://graph.microsoft.com/v1.0/me/mailFolders/inbox/messages?$top=10&$select=subject,from,receivedDateTime";

		UnityWebRequest request = UnityWebRequest.Get(url);
		request.SetRequestHeader("Authorization", "Bearer " + accessToken);

		var operation = request.SendWebRequest();
		while (!operation.isDone)
			await Task.Yield();

		if (request.result != UnityWebRequest.Result.Success) {
			Debug.LogError("Graph API error: " + request.error);
			return;
		}

		string json = request.downloadHandler.text;
		JObject data = JObject.Parse(json);

		foreach (Transform child in contentParent) {
			Destroy(child.gameObject); // clear old messages
		}

		foreach (var msg in data["value"]) {
			string sender = msg["from"]?["emailAddress"]?["address"]?.ToString() ?? "(unknown)";
			string subject = msg["subject"]?.ToString() ?? "(no subject)";
			string date = msg["receivedDateTime"]?.ToString() ?? "";

			GameObject go = Instantiate(messagePrefab, contentParent);

			var fromField = go.transform.Find("From/From (1)")?.GetComponent<TextMeshProUGUI>();
			var subjectField = go.transform.Find("Subject")?.GetComponent<TextMeshProUGUI>();
			var dateField = go.transform.Find("DateSent")?.GetComponent<TextMeshProUGUI>();

			if (fromField != null) fromField.text = sender;
			if (subjectField != null) subjectField.text = subject;
			if (dateField != null) dateField.text = date;

			var mover = go.AddComponent<RightUISliderMover>();
			mover.Initialize(
				SceneRefs.Instance.chevron,
				SceneRefs.Instance.topLine,
				SceneRefs.Instance.bottomLine,
				SceneRefs.Instance.highlight
			);

			// Hook up the button click at runtime
			Button btn = go.GetComponent<Button>();
			if (btn != null) {
				btn.onClick.AddListener(() => mover.MoveToTarget(go.GetComponent<RectTransform>()));
			}
		}
	}
}
