using UnityEngine;

public class NewMessageButton : MonoBehaviour {
	[SerializeField] GameObject readingPanel;
	[SerializeField] GameObject newMessagePanel;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void OnClick() {
		readingPanel.SetActive(false);
		newMessagePanel.SetActive(true);
	}
}
