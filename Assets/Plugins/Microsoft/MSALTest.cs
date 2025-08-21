using UnityEngine;
using Microsoft.Identity.Client;
using System.Linq;
using System.Threading.Tasks;

public class MSALTest : MonoBehaviour {
	// Replace this with your Azure App Registration client ID
	private string clientId = "16527e35-0698-46b3-a890-562410a382ed";

	// Microsoft Graph scopes you want to request
	private string[] scopes = new string[] { "User.Read" };

	private IPublicClientApplication _pca;

	async void Start() {
		_pca = PublicClientApplicationBuilder.Create(clientId)
			.WithRedirectUri("http://localhost")
			.Build();

		string token = await SignInAndGetTokenAsync();
		if (!string.IsNullOrEmpty(token)) {
			Debug.Log("Access Token:\n" + token);
		}
		else {
			Debug.LogError("Failed to acquire token.");
		}
	}

	private async Task<string> SignInAndGetTokenAsync() {
		try {
			// Try silent login (cached token)
			var accounts = await _pca.GetAccountsAsync();
			var result = await _pca.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
				.ExecuteAsync();
			return result.AccessToken;
		}
		catch (MsalUiRequiredException) {
			// Interactive login required
			var result = await _pca.AcquireTokenInteractive(scopes)
				.WithPrompt(Prompt.SelectAccount)
				.ExecuteAsync();
			return result.AccessToken;
		}
		catch (System.Exception ex) {
			Debug.LogError("MSAL Error: " + ex.Message);
			return null;
		}
	}
}
