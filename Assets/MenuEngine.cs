using UnityEngine;
using System.Collections;

public class MenuEngine : MonoBehaviour {

	public GameObject world;
	public Camera camera;
	public GameObject activeMenu;

	float _worldRotationUpSpeed = 1f;
	float _worldRotationLeftSpeed = -1f;
	float _planetRotationSpeed = 2f;
	float _canvasRotationSpeed = 5f; 
	float _dragRotationSpeed = 20f; 
	bool _canvasRotationIsClockwise = true;
	bool _planetPositionIsLeft = true;

	float _canvasRotationHide = -90f;
	float _canvasRotationInit = 25f;

	GameObject _cameraTarget;
	GameObject _center;
	GameObject _planet;
	GameObject _canvas;
	GameObject _title;
	GameObject _panel;

	bool _cameraMoving = false;
	Vector3 _cameraMovingPositionStart;
	Vector3 _cameraMovingRotateStart;
	float _cameraMoving_t = 1f; // 0 -> 1
	float _cameraMovingTime = 1f; // 1.5s

	bool _canvasRotate = false;
	float _canvasRotationStart;
	float _canvasRotationTarget;
	float _canvasRotation_t = 0f; // 0 -> 1
	float _canvasRotationTime = .75f; // 1.5s

	bool _centerTranslate = false;
	float _centerTranslateStart; 
	float _centerTranslateTarget;
	float _centerTranslate_t = 0f; // 0 -> 1
	float _centerTranslateTime = .25f; // 1.5s
	float _centerTranslatePanelRotateStart;
	float _centerTranslatePanelRotateTarget;

	float _canvas_planetLeft_middle_angle = 70f; // To manually config :/
	float _canvas_planetRight_middle_angle = 110f; // To manually config :/

	float _min_delta = 30f;
	float _max_delta = 70f;


	// Use this for initialization
	void Start () {
		setActiveMenu (activeMenu);
	}
	
	// Update is called once per frame
	void Update () {
		MakeRotations ();

		if (_cameraMoving) {
			_cameraMoving_t += Time.deltaTime / _cameraMovingTime;

			camera.transform.position = Vector3.Lerp (_cameraMovingPositionStart, _cameraTarget.transform.position, _cameraMoving_t);
			// WTF ? camera.transform.eulerAngles = Vector3.Lerp (_cameraMovingRotateStart, _cameraTarget.transform.eulerAngles, _cameraMoving_t);

			if (_cameraMoving_t > 1) {
				_cameraMoving = false;

				// Start displaying menu
				_canvas.SetActive (true);
				_canvasRotation_t = 0f;
				_canvasRotationStart = _canvasRotationHide;
				_canvasRotationTarget = _canvasRotationInit;
				_canvasRotate = true;
			}
		}

		// If canvas is animated
		else if (_canvasRotate) {
			_canvasRotation_t += Time.deltaTime / _canvasRotationTime;

			float angle = Mathf.Lerp (_canvasRotationStart, _canvasRotationTarget, _canvasRotation_t);
			_canvas.transform.localEulerAngles = new Vector3 (0, angle, 0);

			if (_canvasRotation_t > 1) {
				_canvasRotate = false;
			}
		}

		// If center is animated
		else if (_centerTranslate) {
			_centerTranslate_t += Time.deltaTime / _centerTranslateTime;

			float position = Mathf.Lerp (_centerTranslateStart, _centerTranslateTarget, _centerTranslate_t);
			float angle = Mathf.Lerp (_canvasRotationStart, _canvasRotationTarget, _centerTranslate_t);
			float panel_angle = Mathf.Lerp (_centerTranslatePanelRotateStart, _centerTranslatePanelRotateTarget, _centerTranslate_t);

			_center.transform.localPosition = new Vector3 (position, 0, 0);
			_canvas.transform.localEulerAngles = new Vector3 (0, angle, 0);
			_title.transform.localEulerAngles = new Vector3 (0, panel_angle, 0);
			_panel.transform.localEulerAngles = new Vector3 (0, panel_angle, 0);

			if (_centerTranslate_t > 1) {
				_centerTranslate = false;
			}
		}

		else {
			float delta = GetDeltaAngle ();
			//Debug.Log (_canvas.transform.localEulerAngles.y + " --> " + delta);
			bool is_forward = (_canvasRotationIsClockwise == _planetPositionIsLeft);
			if (is_forward && (delta < _min_delta) || !is_forward && (delta > _max_delta)) {
				Switch();
			}
		}		
	}

	void MakeRotations() {
		// Turn Planet 
		_planet.transform.localEulerAngles = new Vector3 (0, _planet.transform.localEulerAngles.y + _planetRotationSpeed * Time.deltaTime, 0);

		// Turn World
		world.transform.localEulerAngles = new Vector3 (
			world.transform.localEulerAngles.x + _worldRotationLeftSpeed * Time.deltaTime,
			world.transform.localEulerAngles.y + _worldRotationUpSpeed * Time.deltaTime,
			0
		);

		// Turn Canvas
		float canvas_angle = _canvasRotationSpeed * Time.deltaTime;
		if (!_canvasRotationIsClockwise) {
			canvas_angle *= -1; // reverse
		}
		_canvas.transform.localEulerAngles = new Vector3 (0, _canvas.transform.localEulerAngles.y + canvas_angle, 0);
	}

	public void setActiveMenu(GameObject menu) {
		if (_canvas) {
			_canvas.SetActive (false);
		}
		_cameraTarget = _planet = _canvas = null;

		foreach (Transform child in menu.transform) {
			if (child.name == "CameraTarget") {
				_cameraTarget = child.gameObject;
			} else if (child.name == "Center") {
				_center = child.gameObject;
				foreach (Transform subchild in child.transform) {
					if (subchild.name == "Planet") {
						_planet = subchild.gameObject;
					} else if (subchild.name == "Canvas") {
						_canvas = subchild.gameObject;
						foreach (Transform subsubchild in subchild.transform) {
							if (subsubchild.name == "Title") {
								_title = subsubchild.gameObject;
							}
							else if (subsubchild.name == "Panel") {
								_panel = subsubchild.gameObject;
							}
						}
					}
				}
			}
		}

		// Give a ref to `this` to planet for Events
		_planet.GetComponent<Planet>().menuEngine = this;

		_cameraMoving_t = 0f; 
		_cameraMoving = true;
		_cameraMovingPositionStart = camera.transform.position;

		_cameraMovingRotateStart = new Vector3 (
			(camera.transform.eulerAngles.x > 180) ? camera.transform.eulerAngles.x - 360 : camera.transform.eulerAngles.x,
			(camera.transform.eulerAngles.y > 180) ? camera.transform.eulerAngles.y - 360 : camera.transform.eulerAngles.y,
			(camera.transform.eulerAngles.z > 180) ? camera.transform.eulerAngles.z - 360 : camera.transform.eulerAngles.z
		);
		_cameraMovingRotateStart = camera.transform.eulerAngles;

		// RESET the menu setup !
		_canvasRotationIsClockwise = true;
		_planetPositionIsLeft = true;

		_canvas.transform.localEulerAngles = new Vector3(0f, _canvasRotationHide, 0f);
		_center.transform.localPosition = new Vector3 (-75, 0, 0); 
		_title.transform.localEulerAngles = new Vector3 (0, 0, 0); 
		_panel.transform.localEulerAngles = new Vector3 (0, 0, 0); // Planet is left at start
	}

	public void OnPlanetDrag(float delta) {
		_canvas.transform.localEulerAngles = new Vector3 (0, _canvas.transform.localEulerAngles.y + delta * _dragRotationSpeed * Time.deltaTime, 0);
		_planet.transform.localEulerAngles = new Vector3 (0, _planet.transform.localEulerAngles.y + delta * _dragRotationSpeed * Time.deltaTime, 0);

		if (delta != 0) {
			_canvasRotationIsClockwise = (delta > 0);	
		}
	}
		
	public void Switch() {
		// Translate planet
		_centerTranslate_t = 0;
		_centerTranslate = true;
		_centerTranslateStart = _center.transform.localPosition.x;
		_centerTranslateTarget = -_center.transform.localPosition.x;

		// Delta angle with middle
		float current_canvas_angle = _canvas.transform.localEulerAngles.y;
		if (current_canvas_angle > 360) { current_canvas_angle -= 360; } 
		if (current_canvas_angle < 0) { current_canvas_angle += 360; } 

		float delta = GetDeltaAngle ();

		// Rotate panel and canvas
		_canvasRotationStart = _canvas.transform.localEulerAngles.y;
		if (_planetPositionIsLeft) {
			_centerTranslatePanelRotateStart = 0;
			_centerTranslatePanelRotateTarget = -180;

			// Use the same angle but the other side
			_canvasRotationTarget = _canvas_planetRight_middle_angle + delta;
			if (!_canvasRotationIsClockwise) {
				_canvasRotationTarget = _canvas_planetRight_middle_angle - delta + 360;
				_centerTranslatePanelRotateTarget = 180;
			}
		} else  {
			_centerTranslatePanelRotateStart = -180;
			_centerTranslatePanelRotateTarget = 0;

			// Use the same angle but the other side
			_canvasRotationTarget = _canvas_planetLeft_middle_angle - delta;
			if (_canvasRotationIsClockwise) {
				// If it's backward, add 360 
				_canvasRotationTarget += 360;
				_centerTranslatePanelRotateTarget = -360;
			}
		}

		_planetPositionIsLeft = !_planetPositionIsLeft;
	}

	float GetDeltaAngle() {
		float current_canvas_angle = _canvas.transform.localEulerAngles.y;
		if (current_canvas_angle > 360) { current_canvas_angle -= 360; } 
		if (current_canvas_angle < 0) { current_canvas_angle += 360; } 
		float delta = 0f;
		if (_planetPositionIsLeft) { 
			delta = Mathf.Abs (current_canvas_angle - _canvas_planetLeft_middle_angle);
		} else {
			delta = Mathf.Abs (current_canvas_angle - _canvas_planetRight_middle_angle);
		}
		return delta;
	}
}
