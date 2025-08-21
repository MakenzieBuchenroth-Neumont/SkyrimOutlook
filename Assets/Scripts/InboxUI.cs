using UnityEngine;
using Microsoft.Identity.Client;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Handles authentication, fetching inbox messages, and creating message UI prefabs.
/// </summary>
public class InboxUI : MonoBehaviour {
	[Header("MSAL Config")]
	[SerializeField] private string clientId = "16527e35-0698-46b3-a890-562410a382ed";
	private string[] scopes = new string[] { "User.Read", "Mail.Read" };
	private IPublicClientApplication _pca;

	[Header("UI Prefabs/Parents")]
	public GameObject messagePrefab; // Your Message prefab
	public Transform contentParent;   // The ScrollView Content object
	[SerializeField] private TextMeshProUGUI emailName; // top-bar label for user

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

	// --------------------------
	// MSAL Token cache handling
	// --------------------------
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

	// --------------------------
	// Fetch inbox + build UI
	// --------------------------
	private async Task FetchInbox(string accessToken) {
		// Add toRecipients + bodyPreview for reading pane
		string url = "https://graph.microsoft.com/v1.0/me/mailFolders/inbox/messages?$top=10&$select=id,subject,from,toRecipients,receivedDateTime,bodyPreview";

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

		// Clear old items
		foreach (Transform child in contentParent) {
			Destroy(child.gameObject);
		}

		// Build inbox
		foreach (var msg in data["value"]) {
			EmailData email = new EmailData {
				Id = msg["id"]?.ToString(),
				From = msg["from"]?["emailAddress"]?["address"]?.ToString() ?? "(unknown)",
				To = msg["toRecipients"] != null
					? string.Join(", ", msg["toRecipients"].Select(r => r["emailAddress"]?["address"]?.ToString()))
					: "",
				Subject = msg["subject"]?.ToString() ?? "(no subject)",
				Date = msg["receivedDateTime"]?.ToString(),
				Body = msg["bodyPreview"]?.ToString() ?? ""
			};

			// Instantiate prefab
			GameObject go = Instantiate(messagePrefab, contentParent);
			MessageUI ui = go.AddComponent<MessageUI>();
			ui.SetData(email);
		}
	}
}
