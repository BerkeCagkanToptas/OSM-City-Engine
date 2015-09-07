using UnityEngine;
using System.Collections;

public class GizmoSelection : MonoBehaviour {

	private Material StartMaterial;
	private Material SelectedMaterial;
	private GameObject SelectedGameObject;
	private TransformGizmos SelectedTransformGizmos;

	[Tooltip("Key to press to activate Transformation Option.")]
	public KeyCode TransformationOption = KeyCode.Alpha1;
	[Tooltip("Key to press to activate Rotation Option.")]
	public KeyCode RotationOption = KeyCode.Alpha2;
	[Tooltip("Key to press to activate Scale Option.")]
	public KeyCode ScaleOption = KeyCode.Alpha3;
	[Tooltip("Add gizmo code dynamically.")]
	public bool AddDynamically = true;
	public LayerMask DynamicLayer;
	private DynamicTransformGizmos _gizmo;
	// Use this for initialization
	void Start () {
		SelectedMaterial = new Material (Shader.Find("Transparent/Diffuse"));

		if (AddDynamically) {
			_gizmo = gameObject.GetComponent<DynamicTransformGizmos>();
		}

	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (AddDynamically && _gizmo) {
			Dynamically ();
		} 
		else {
			NonDynamically();
		}

		if (AddDynamically && !_gizmo) {
			Debug.LogError("No 'DynamicTransformGizmos' was found on object. Selection set back to Non-Dynamic.");
			AddDynamically = false;
		}

				
		}
	void Dynamically(){
		Ray CameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Input.GetMouseButtonDown (0)) {
			if(SelectedGameObject!=null && _gizmo.SelectedType == DynamicTransformGizmos.MOVETYPE.NONE){
				SelectedGameObject.GetComponent<Renderer>().material = StartMaterial;
				_gizmo.TurnOffGizmos();
				SelectedGameObject = null;
			}

			if (Physics.Raycast (CameraRay, out hit, Mathf.Infinity, DynamicLayer)) {
				if(_gizmo.SelectedType == DynamicTransformGizmos.MOVETYPE.NONE){
				_gizmo.Item = SelectedGameObject = hit.collider.gameObject;
				_gizmo.TurnOnTransformationOptionGizmo();
				StartMaterial = SelectedGameObject.GetComponent<Renderer>().material;
				SelectedMaterial.color =StartMaterial.color;
				SelectedGameObject.GetComponent<Renderer>().material = SelectedMaterial;
			}
			}

		
		}


		if (SelectedGameObject != null) {
			
			if(Input.GetKeyDown(TransformationOption)){
				_gizmo.TurnOnTransformationOptionGizmo();
			}
			if(Input.GetKeyDown(RotationOption)){
				_gizmo.TurnOnRotationOptionGizmo();
			}
			
			if(Input.GetKeyDown(ScaleOption)){
				_gizmo.TurnOnScaleOptionGizmo();
			}
			
		}

	}

	void NonDynamically(){
		Ray CameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Input.GetMouseButtonDown (0)){
			
			if(SelectedGameObject!=null){
				
				if(SelectedTransformGizmos !=null){
					if(SelectedTransformGizmos.SelectedType == TransformGizmos.MOVETYPE.NONE){
						SelectedTransformGizmos.TurnOffGizmos();
						SelectedGameObject.GetComponent<Renderer>().material = StartMaterial;
						SelectedTransformGizmos = null;
						SelectedGameObject = null;
					}
				}
				
			}
			if (Physics.Raycast (CameraRay, out hit, 50)) {
				if (hit.collider.gameObject != null && hit.collider.gameObject != SelectedGameObject) {
					
					SelectedGameObject = hit.collider.gameObject;
					if (SelectedGameObject.GetComponent<TransformGizmos> ()) {
						SelectedTransformGizmos = SelectedGameObject.GetComponent<TransformGizmos> ();
						SelectedTransformGizmos.TurnOnTransformationOptionGizmo();
						StartMaterial = SelectedGameObject.GetComponent<Renderer>().material;
						SelectedMaterial.color =StartMaterial.color;
						SelectedGameObject.GetComponent<Renderer>().material = SelectedMaterial;
						
					}
					
				}
				
				
				
			}
		}
		
		if (SelectedGameObject != null && SelectedTransformGizmos !=null) {
			
			if(Input.GetKeyDown(TransformationOption)){
				SelectedTransformGizmos.TurnOnTransformationOptionGizmo();
			}
			if(Input.GetKeyDown(RotationOption)){
				SelectedTransformGizmos.TurnOnRotationOptionGizmo();
			}
			
			if(Input.GetKeyDown(ScaleOption)){
				SelectedTransformGizmos.TurnOnScaleOptionGizmo();
			}
			
		}
	}
	
	
	
	
	
}
