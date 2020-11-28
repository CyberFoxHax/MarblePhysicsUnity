using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OrbitCamera : MonoBehaviour {

	public float distance;

	public float yaw;
	public float pitch;

	public float cameraSpeed;
	public bool invertX;
	public bool invertY;

	public Transform target;

	public RangeAttribute pitchRange;
	public RangeAttribute yawRange;

	// Use this for initialization
	void Start() {
        if (target == null)
            target = FindObjectOfType<Movement>()?.transform;
	}
	
	// Update is called once per frame
	void LateUpdate() {
		if (target == null)
			return;
		
		if (Input.GetMouseButtonDown(0)) {
			Cursor.lockState = CursorLockMode.Locked;
		}

		if (Cursor.lockState == CursorLockMode.Locked) {
			//Get updates from the input
			yaw += cameraSpeed * Input.GetAxis("Mouse X") * (invertX ? -1 : 1);
			pitch += cameraSpeed * Input.GetAxis("Mouse Y") * (invertY ? 1 : -1);
		}

		float actualDistance = distance;// + (target.GetComponent<Marble>().radius * 2.0f);

		//Easy lock to the object
		Vector3 position = target.position;

		//Rotate by pitch and yaw (and not roll, oh god my stomach)
		Quaternion rotation = Quaternion.AngleAxis(yaw, Vector3.up);
		rotation *= Quaternion.AngleAxis(pitch, Vector3.right);

		//Offset for orbiting
		position += rotation * new Vector3(0.0f, 0.0f, -actualDistance);

		//Lame way of updating the transform
		transform.rotation = rotation;
		transform.position = position;
	}
}
