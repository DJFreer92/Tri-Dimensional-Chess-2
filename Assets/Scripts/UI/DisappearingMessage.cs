using UnityEngine;

[DisallowMultipleComponent]
public sealed class DisappearingMessage : MonoBehaviour {
	[SerializeField] private float _displayTime = 5f;
	private float _timeDisplayed;

	private void Update() {
		if (_timeDisplayed >= _displayTime) Disapper();
		_timeDisplayed += Time.deltaTime;
	}

	public void Disapper() {
		Destroy(gameObject);
	}
}