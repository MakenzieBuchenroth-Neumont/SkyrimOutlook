using UnityEngine;

public class GlobalClickEffect : MonoBehaviour {
	[Header("Assign particle prefab from the asset pack")]
	public GameObject clickEffectPrefab;

	[Header("Sorting layer for particles (make sure it exists)")]
	public string sortingLayerName = "UI";
	public int sortingOrder = 999;

	private Camera mainCam;

	void Start() {
		mainCam = Camera.main;
	}

	void Update() {
		if (Input.GetMouseButtonDown(0)) // left click
		{
			SpawnClickEffect();
		}
	}

	void SpawnClickEffect() {
		Vector3 mousePos = Input.mousePosition;

		// Spawn just in front of the camera
		mousePos.z = 1f;
		Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);

		GameObject effect = Instantiate(clickEffectPrefab, worldPos, Quaternion.identity);

		// Apply sorting to all particle renderers
		foreach (var psr in effect.GetComponentsInChildren<ParticleSystemRenderer>()) {
			psr.sortingLayerName = sortingLayerName;
			psr.sortingOrder = sortingOrder;
		}

		// Cleanup
		var ps = effect.GetComponent<ParticleSystem>();
		if (ps != null)
			Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
	}

}
