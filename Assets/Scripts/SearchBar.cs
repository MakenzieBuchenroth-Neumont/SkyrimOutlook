using UnityEngine;
using TMPro; // If using TextMeshPro

public class SearchBar : MonoBehaviour {
	public TMP_InputField inputField; // Drag your Input Field here in the Inspector

	void Start() {
		inputField.onValueChanged.AddListener(OnInputChanged);
	}

	void OnInputChanged(string input) {
		Debug.Log("User typed: " + input);
		// Here, you could filter a list of items, show search results, etc.
	}
}
