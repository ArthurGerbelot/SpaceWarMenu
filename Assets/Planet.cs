using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour {

	public MenuEngine menuEngine;
	private float lastX;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		lastX = Input.mousePosition.x;
	}

	void OnMouseDrag() {
		menuEngine.OnPlanetDrag (lastX - Input.mousePosition.x);
		lastX = Input.mousePosition.x;
	}
}
