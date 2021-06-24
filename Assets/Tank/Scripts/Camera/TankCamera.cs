using UnityEngine;
using System.Collections;

public class TankCamera : MonoBehaviour {

	public Transform target;

	void LateUpdate () {
		if (!target)
			return;
		
		transform.position = target.position;
	}
}
