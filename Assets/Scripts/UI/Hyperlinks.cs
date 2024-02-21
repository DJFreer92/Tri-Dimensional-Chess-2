using UnityEngine;

[RequireComponent(typeof(Canvas))]
[DisallowMultipleComponent]
public sealed class Hyperlinks : MonoSingleton<Hyperlinks> {
	public void OpenURL(string url) {
		Application.OpenURL(url);
	}
}