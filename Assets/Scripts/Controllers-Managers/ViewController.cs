using UnityEngine;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public sealed class ViewController : MonoBehaviour {
	[Header("References")]
	[SerializeField] private Transform _anchor;

	[Header("Constants")]
	[SerializeField] private float _scrollScale = 0.15f;
	[SerializeField] private float _positionScale = 0.005f;
	[SerializeField] private float _maxRange = 19.9f;
	[SerializeField] private float _minRange = 4f;
	[SerializeField] private float _positiveRotationBound = 89f;
	[SerializeField] private float _negativeRotationBound = 271f;
	
	private Transform cameraTransform;

	private void Awake() {
		cameraTransform = transform;
	}

	private void Update() {
		//if the y position is past the floor set the y position to 0
		if (cameraTransform.position.y <= 0) {
			Vector3 temp = cameraTransform.position;
			temp.y = 0;
			cameraTransform.position = temp;
		}

		//horizontal input from user
		float horizontal = Input.GetAxis("Horizontal");

		//rotate around the x-axis
		VerticalRotation();

		//rotate around the y-axis
		cameraTransform.Translate(_positionScale * horizontal * Vector3.Distance(cameraTransform.position, _anchor.position) * Vector3.right);

		//look at the center of the board
		cameraTransform.LookAt(_anchor);
	}

	private void OnGUI() {
		//scroll input from the user
		float scroll = Input.mouseScrollDelta.y;

		//distance between the camera and the anchor
		float _anchorDistance = Vector3.Distance(cameraTransform.position, _anchor.position);

		//prevent zooming past the boundaries of the game
		if (_anchorDistance >= _maxRange && scroll >= 0) return;

		//prevent zooming into the center of the board
		if (_anchorDistance <= _minRange && scroll <= 0) return;

		//prevent zooming out past the floor
		if (cameraTransform.position.y <= 0 && scroll >= 0) return;

		//zoom the camera in and out
		cameraTransform.Translate(scroll * _scrollScale * Vector3.back);
	}

	///<summary>
	///Rotates the camera around the x-axis
	///</summary>
	private void VerticalRotation() {
		//vertical input from user
		float vertical = Input.GetAxis("Vertical");

		//prevent rotating past the positive rotation boundary
		if (cameraTransform.position.y > _anchor.position.y && cameraTransform.rotation.eulerAngles.x >= _positiveRotationBound && vertical > 0) return;

		//prevent rotating past the negative rotation boundary
		if (cameraTransform.position.y < _anchor.position.y && cameraTransform.rotation.eulerAngles.x <= _negativeRotationBound && vertical < 0) return;

		//rotate around the x-axis
		cameraTransform.Translate(_positionScale * vertical * Vector3.Distance(cameraTransform.position, _anchor.position) * Vector3.up);
	}
}