using UnityEngine;
using System.Collections;

public class TransformGizmos : MonoBehaviour {

	public enum      MOVETYPE{NONE,X,X2,Y,Y2,Z,Z2,XZ,YZ,XY,RX,RY,RZ,TX,TY,TZ,TX2,TY2,TZ2,TXY,TXZ,TYZ}; // all the ways an object can be altered
	public MOVETYPE  SelectedType; // current alteration

	private Material  lineMaterial; // same as SelectedObjOutline
	private Color     selX,selY,selZ,panY,panX,panZ,circleX,circleY,circleZ;
	public Color SelectedColor = new Color(1,1,0,0.5f);

	private Plane     planeXZ,planeYZ,planeXY; // all form of rotation planes

	private float     SelectedSencsitivity=0.1f; //sencsitivity
	public float     Sencsitivity=1f; //sencsitivity
	public float    Spread=3.0f;  //Spread of lines
	private float     rotaCal; // how the roation is calculated
	private Vector3   matScal = new Vector3(1,1,1); // the alteration of how to scale this can be changed
	private Vector3   MouseDown  = Vector3.zero;
	private Vector3   Drag  = Vector3.zero;  
	private Vector3   uForward  = Vector3.zero;
	private Vector3 rForward = Vector3.zero; //current x 
	private Vector3 fForward = Vector3.zero; // current z
	private Vector3 Dir = Vector3.zero; //The current derections of the Gizmos and object
	private Vector3   CurrentScale; // current scale
	
	private Matrix4x4 locklocalToWorldMatrix; // how to draw the vectors
	private Matrix4x4 rotationMatrix;  // draw the rotation cercal

	public  bool TransformationOption = false; //choose to move object around
	public  bool RotationOption = false; // to rotate object
	public  bool  ScaleOption = false; //to scale object
	public int RotationSpeed = 25; // how fast the object rotates around in the scene

	public bool UseOwnColor = false;     //For Editor
	public bool AdvancedSettings = false;  // For Editor
	public bool DebugOption = false;  // For Editor
	public bool ShowColOutline = true; //For Editor

	public Color LineColor;         // if "UseOwnColor" is true then set the color of the line to this
	private  Bounds   ColliderBounds; // Storing the Coliders bounds
	public float lines=1f;           //spread of lines

	public bool ScaleWithCamera = true; // makes gizmo bigger as Camera moves away

	public bool ShowColOutlineDuringScale = true; //Show Outline during Transform
	public bool ShowColOutlineDuringRotate = true;//Show Outline during Rotation
	public bool ShowColOutlineDurtingTransform = true;//Show Outline during Scale

	public bool NeedContactToTurn =false;
	private float SetSpread;
	private float distanceMovement;
	void Awake(){
		//Creating the material for the Gizmo
		SetMaterial ();
		SetSpread = Spread;
		//Start with all things cleared
	    rotationMatrix=Matrix4x4.TRS(transform.position,transform.localRotation,matScal);
		locklocalToWorldMatrix=rotationMatrix;
		CurrentScale=transform.localScale; 
	}
	
	void LateUpdate(){
		if (ScaleWithCamera) {
			float DistanceToCam = Vector3.Distance (Camera.main.transform.position, transform.position) / 10;
			SetSpread= Mathf.Clamp (DistanceToCam, Spread, Spread*2 );

		}
		distanceMovement = Vector3.Distance (Camera.main.transform.position, transform.position) /2;
	}
	void SetMaterial(){
		lineMaterial=new Material(Shader.Find("UI/Unlit/Detail"));
		lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
	}
	//use this to turn off all gizmos 
	public void TurnOffGizmos(){
	TransformationOption = false; 
	RotationOption = false; 
		ScaleOption = false;
	}

	public void TurnOnTransformationOptionGizmo(){
		RotationOption = false; 
		ScaleOption = false;
		TransformationOption = true; 
	}

	public void TurnOnRotationOptionGizmo(){
		TransformationOption = false; 
		ScaleOption = false;
		RotationOption = true; 
	}

	public void TurnOnScaleOptionGizmo(){
		TransformationOption = false; 
		RotationOption = false; 
		ScaleOption = true;
	}
	void NothingSelected(){
		panY=Color.green;
		panX=Color.red;
		panZ=Color.blue;
		selX=Color.red;
		selY=Color.green;
		selZ=Color.blue;
		circleX=Color.red;
		circleY=Color.green;
		circleZ=Color.blue;
		CurrentScale=transform.localScale;
	}
	
	void OnRenderObject(){
		if (lines < 1) {
			lines = 1;		
		}
		if (ScaleOption && ShowColOutlineDuringScale) {
			TurnOnOutline();	
		}

		if (RotationOption && ShowColOutlineDuringRotate) {
			TurnOnOutline();	
		}
		if (TransformationOption && ShowColOutlineDurtingTransform) {
			TurnOnOutline();	
		}


		
		if(SelectedType==MOVETYPE.NONE){
			NothingSelected();
		}
	    if(!Input.GetMouseButton(0)){
		SelectedType=MOVETYPE.NONE;

			if(	SelectedType==MOVETYPE.NONE){
			RenderGizmos();
			}
		}
	    
	    planeXZ.SetNormalAndPosition(transform.up,transform.position);
		planeXY.SetNormalAndPosition(transform.forward,transform.position);
		planeYZ.SetNormalAndPosition(transform.right,transform.position);		
	


		Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
	    float enter=0.0f;
		float rlens=0.0f;
		planeXZ.Raycast(ray,out enter);
		Vector3 hit=ray.GetPoint(enter);

		if (SelectedType == MOVETYPE.RY) {
						hit = locklocalToWorldMatrix.inverse.MultiplyPoint (hit);
				} else {hit = rotationMatrix.inverse.MultiplyPoint (hit);}
	
		if(TransformationOption ){
		if(SelectedType==MOVETYPE.NONE&& hit.x>0f && hit.x<=0.3f*SetSpread && hit.z>0 && hit.z<=0.3f*SetSpread){
			panY=SelectedColor;
			SelectedType=MOVETYPE.XZ;
			MouseDown=hit;
		}

		if(SelectedType==MOVETYPE.XZ   && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f) ){
		    Drag=hit-MouseDown;
			Drag.y=0;
		    transform.position+=transform.localRotation*Drag;
		}
		
		if(SelectedType==MOVETYPE.NONE && hit.x>0f && hit.x<=SetSpread && Mathf.Abs(hit.z)<SelectedSencsitivity ){ //case x
		   selX=SelectedColor;
		   SelectedType=MOVETYPE.X;
		   MouseDown=hit;
				
		}
		if(SelectedType==MOVETYPE.X && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		    Drag=hit-MouseDown;
			Drag.y=0;
		    Drag.z=0;
		    transform.position+=transform.localRotation*Drag;
		    
		}
		if(SelectedType==MOVETYPE.NONE && hit.z>0f && hit.z<=SetSpread && Mathf.Abs(hit.x)<SelectedSencsitivity ){//case  z
		    selZ=SelectedColor;
		    SelectedType=MOVETYPE.Z;
			MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.Z && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		    Drag=hit-MouseDown;
			Drag.y=0;
		    Drag.x=0;
		    transform.position+=transform.localRotation*Drag;

		}
		} //TransformationOption end
		if(ScaleOption){
			if(SelectedType==MOVETYPE.NONE&& hit.x>0f && hit.x<=0.3f*SetSpread && hit.z>0 && hit.z<=0.3f*SetSpread && hit.x+hit.z-0.3f*SetSpread<=0f){
			
			panY=SelectedColor;
				
	
			SelectedType=MOVETYPE.TXZ;
		
		

			MouseDown=hit;
		
			
		}
		if(SelectedType==MOVETYPE.TXZ   && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f) ){
			
		
		    Drag=hit-MouseDown;
			Drag.y=0;
		   
			float dm =  Drag.x+Drag.z;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(dchg,0,dchg);
		
		   
		}
		
		if(SelectedType==MOVETYPE.NONE && hit.x>0f && hit.x<=SetSpread && Mathf.Abs(hit.z)<SelectedSencsitivity ){ //case x
		   selX=SelectedColor;
		   SelectedType=MOVETYPE.TX;
		   MouseDown=hit;
		 
		}
		if(SelectedType==MOVETYPE.TX && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.y=0;
		    Drag.z=0;
	
			float dm =  Drag.x;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(dchg,0,0);
		    
		}
		if(SelectedType==MOVETYPE.NONE && hit.z>0f && hit.z<=SetSpread && Mathf.Abs(hit.x)<SelectedSencsitivity ){//case  z
		    selZ=SelectedColor;
		    SelectedType=MOVETYPE.TZ;
			MouseDown=hit;
		
		}
		if(SelectedType==MOVETYPE.TZ && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.y=0;
		    Drag.x=0;
	
			float dm =  Drag.z;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(0,0,dchg);
		  
		}//ScaleOption end
			
			
		}
		rlens=(Mathf.Sqrt(hit.x*hit.x+hit.z*hit.z)/3) * distanceMovement;
		
		
		
		if(RotationOption){
		if( Vector3.Dot(hit,uForward)>=.2f)
			if(SelectedType==MOVETYPE.NONE&& rlens>(1-(Sencsitivity*100) ) && rlens<((4+(Sencsitivity) )) ){
				circleX=Color.red;
				circleY=Color.green;
				circleZ=Color.blue;
		   circleY=SelectedColor;
		   SelectedType=MOVETYPE.RY;
		   MouseDown=hit;
		
			
			
		locklocalToWorldMatrix=rotationMatrix;
	

	    rotaCal=0.0f;
	  
		} 
	    if(SelectedType==MOVETYPE.RY ){  //rotate y
		    
		    Drag= hit;
		    

			float angledown	=Vector3.Dot(MouseDown.normalized, Vector3.right); 
	        float anglemove =Vector3.Dot(Drag.normalized, Vector3.right); 
			float angledeta =Vector3.Dot(MouseDown.normalized,Drag.normalized);
		
		   angledown= SnapDot(angledown,MouseDown.x,MouseDown.z);	
			
		   anglemove= SnapDot(anglemove,Drag.x,Drag.z);	
		
		   angledeta=Mathf.Acos(angledeta);
		   
				if(angledown==anglemove){ RenderGizmos(); return;}
		   	
		   float judge=angledown+Mathf.PI;
			
		   if(judge>2*Mathf.PI){
		      if(anglemove>=0f && anglemove<=judge-2*Mathf.PI || anglemove>angledown){     
				
				}
			  else
				
					angledeta=-angledeta;
				
			  
				
		   }else{
			   if(anglemove>=angledown && anglemove<=judge){
				}
				else
					angledeta=-angledeta;
					
			
		   }
		   
		   rotaCal=angledeta-rotaCal;
		   
			
			
		 
           DrawCam(Color.white,angledown,angledeta,MOVETYPE.Y);
		   
				if(NeedContactToTurn == true){
				transform.Rotate(new Vector3(0,-rotaCal,0)*RotationSpeed);
				}else{
				transform.Rotate(Vector3.up*(-Input.GetAxis("Mouse X")+Input.GetAxis("Mouse Y"))*(RotationSpeed/2));
				}
		  	
		  
		   rotaCal=angledeta;  
		  
		  
		}
		}//RotationOption end

		
		enter=0.0f;
		planeXY.Raycast(ray,out enter);
		hit=ray.GetPoint(enter);
		
		
		if(SelectedType==MOVETYPE.RZ){
		hit=locklocalToWorldMatrix.inverse.MultiplyPoint(hit);
		}
		else
		hit=rotationMatrix.inverse.MultiplyPoint(hit);
	
		
	    if(TransformationOption){
		if(SelectedType==MOVETYPE.NONE&&hit.x>0f && hit.x<=0.3f*SetSpread && hit.y>0 && hit.y<=0.3f*SetSpread){
			panZ=SelectedColor;
			SelectedType=MOVETYPE.XY;
			MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.XY ){
			
		    Drag=hit-MouseDown;
			
		
			Drag.z=0;
		    transform.position+=transform.localRotation*Drag;
			
			
		   
		}
		if(SelectedType==MOVETYPE.NONE && hit.x>0f && hit.x<=SetSpread && Mathf.Abs(hit.y)<SelectedSencsitivity ){ //case x
		   selX=SelectedColor;
		   SelectedType=MOVETYPE.X2;
	       
		   MouseDown=hit;
			
				
		}
		if(SelectedType==MOVETYPE.X2 && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.y=0;
		    Drag.z=0;
		    transform.position+=transform.localRotation*Drag;
		  
		}
		if(SelectedType==MOVETYPE.NONE && hit.y>0f && hit.y<=SetSpread && Mathf.Abs(hit.x)<SelectedSencsitivity){ //case y
		   selY=SelectedColor;
		   SelectedType=MOVETYPE.Y;
		   MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.Y && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.z=0;
		    Drag.x=0;
		    transform.position+=transform.localRotation*Drag;
		  
		}
		}//TransformationOption end
		if(ScaleOption){
			if(SelectedType==MOVETYPE.NONE&&hit.x>0f && hit.x<=0.3f*SetSpread && hit.y>0 && hit.y<=0.3f*SetSpread&& hit.x+hit.y-0.3f*SetSpread<=0f){
			panZ=SelectedColor;
			SelectedType=MOVETYPE.TXY;
			MouseDown=hit;
			
		}
		if(SelectedType==MOVETYPE.TXY ){
			
		    Drag=hit-MouseDown;
			
		
			Drag.z=0;
	
		
			float dm =  Drag.x+Drag.y;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(dchg,dchg,0f);
		    
		}
		if(SelectedType==MOVETYPE.NONE && hit.x>0f && hit.x<=SetSpread && Mathf.Abs(hit.y)<SelectedSencsitivity){ //case x
		   selX=SelectedColor;
		   SelectedType=MOVETYPE.TX2;
		   MouseDown=hit;
		   
		}
		if(SelectedType==MOVETYPE.TX2 && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.y=0;
		    Drag.z=0;
		
			float dm =  Drag.x;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(dchg,0,0);
		    
		}
		if(SelectedType==MOVETYPE.NONE && hit.y>0f && hit.y<=SetSpread && Mathf.Abs(hit.x)<SelectedSencsitivity){ //case y
		   selY=SelectedColor;
		   SelectedType=MOVETYPE.TY;
		   MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.TY && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.z=0;
		    Drag.x=0;
	
			float dm =  Drag.y;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(0,dchg,0);
		  
		}
		}//ScaleOption end
		rlens=(Mathf.Sqrt(hit.x*hit.x+hit.y*hit.y)/3) * distanceMovement;
		
		if(RotationOption){
		if( Vector3.Dot(hit,fForward)>=.2f)
			if(SelectedType==MOVETYPE.NONE&& rlens>(1-(Sencsitivity*100) )&& rlens<((4+Sencsitivity )) ){
				circleX=Color.red;
				circleY=Color.green;
				circleZ=Color.blue;
		   circleZ=SelectedColor;
		   SelectedType=MOVETYPE.RZ;
		   MouseDown=hit;	
		   locklocalToWorldMatrix=rotationMatrix;
	       rotaCal=0.0f;
		}
		if(SelectedType==MOVETYPE.RZ ){
		    Drag= hit;
		   

			float angledown	=Vector3.Dot(MouseDown.normalized, Vector3.up); 
	        float anglemove =Vector3.Dot(Drag.normalized, Vector3.up); 
			float angledeta =Vector3.Dot(MouseDown.normalized,Drag.normalized);
		
		   angledown= SnapDot(angledown,MouseDown.y,MouseDown.x);	
			
		   anglemove= SnapDot(anglemove,Drag.y,Drag.x);	
		 
		   angledeta=Mathf.Acos(angledeta);
		   
				if(angledown==anglemove){  RenderGizmos(); return;}
		   	
		   float judge=angledown+Mathf.PI;
			
		   if(judge>2*Mathf.PI){
		      if(anglemove>=0f && anglemove<=judge-2*Mathf.PI || anglemove>angledown){     
				
				}
			  else
				
					angledeta=-angledeta;
				
			  
				
		   }else{
			   if(anglemove>=angledown && anglemove<=judge){
				}
				else
					angledeta=-angledeta;
					
			
		   }
		   
		   rotaCal=angledeta-rotaCal;
		   
           DrawCam(Color.white,angledown,angledeta,MOVETYPE.Z);
		
		   
				if(NeedContactToTurn == true){
				transform.Rotate(new Vector3(0,0,-rotaCal)*RotationSpeed);
				}else{
					transform.Rotate(Vector3.forward*(-Input.GetAxis("Mouse X")+Input.GetAxis("Mouse Y"))*RotationSpeed);
				}
		  	
		  
		   rotaCal=angledeta;  
		}
		}//RotationOption end
		enter=0.0f;
		planeYZ.Raycast(ray,out enter);
		hit=ray.GetPoint(enter);
		
		
		
		if(SelectedType==MOVETYPE.RX){
		hit=locklocalToWorldMatrix.inverse.MultiplyPoint(hit);
		}
		else
		hit=rotationMatrix.inverse.MultiplyPoint(hit);

		if(TransformationOption){
		if(SelectedType==MOVETYPE.NONE&& hit.z>0 && hit.z<=0.3f*SetSpread && hit.y>0 && hit.y<=0.3f*SetSpread ){
			panX=SelectedColor;
			SelectedType=MOVETYPE.YZ;
			MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.YZ){
			
		    Drag=hit-MouseDown;
			
			
			Drag.x=0;
		
		    transform.position+=transform.localRotation*Drag;
		
		   
		}
		if(SelectedType==MOVETYPE.NONE && hit.z>0f && hit.z<=SetSpread && Mathf.Abs(hit.y)<SelectedSencsitivity*SetSpread){//case  z
		   selZ=SelectedColor;
		   SelectedType=MOVETYPE.Z2;
		   MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.Z2 && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.y=0;
		    Drag.x=0;
		    transform.position+=transform.localRotation*Drag;
		  
		}
		if(SelectedType==MOVETYPE.NONE && hit.y>0f && hit.y<=SetSpread && Mathf.Abs(hit.z)<SelectedSencsitivity*SetSpread ){ //case y
		   selY=SelectedColor;
		   SelectedType=MOVETYPE.Y2;
		   MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.Y2 && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.z=0;
		    Drag.x=0;
		    transform.position+=transform.localRotation*Drag;
		  
		}
	    }//TransformationOption end
		if(ScaleOption){
		if(SelectedType==MOVETYPE.NONE&& hit.z>0 && hit.z<=0.3f*SetSpread && hit.y>0 && hit.y<=0.3f*SetSpread && hit.y+hit.z-0.3f*SetSpread<=0f){
			panX=SelectedColor;
			SelectedType=MOVETYPE.TYZ;
			MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.TYZ){
			
		    Drag=hit-MouseDown;
			
			
			Drag.x=0;
		
	
			float dm =  Drag.z+Drag.y;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(0,dchg,dchg);
		
		   
		}
		if(SelectedType==MOVETYPE.NONE && hit.z>0f && hit.z<=SetSpread && Mathf.Abs(hit.y)<SelectedSencsitivity*SetSpread){//case  z
		   selZ=SelectedColor;
		   SelectedType=MOVETYPE.TZ2;
		   MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.TZ2 && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.y=0;
		    Drag.x=0;
			
			float dm =  Drag.z;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(0,0,dchg);
		  
		}
		if(SelectedType==MOVETYPE.NONE && hit.y>0f && hit.y<=SetSpread && Mathf.Abs(hit.z)<SelectedSencsitivity*SetSpread ){ //case y
		   selY=SelectedColor;
		   SelectedType=MOVETYPE.TY2;
		   MouseDown=hit;
		}
		if(SelectedType==MOVETYPE.TY2 && ( Input.GetAxis("Mouse X")!=0f || Input.GetAxis("Mouse Y")!=0f)){
		   	
		    Drag=hit-MouseDown;
			Drag.z=0;
		    Drag.x=0;
		
			float dm =  Drag.y;
			float dchg= (int)((dm)*10)/10f;

			transform.localScale=CurrentScale+ new Vector3(0,dchg,0);
		  
		}
			
			
		}//ScaleOption end
		rlens=(Mathf.Sqrt(hit.z*hit.z+hit.y*hit.y)/3) * distanceMovement;
		if(RotationOption){
		if( Vector3.Dot(hit,rForward)>=.2f)
		if(SelectedType==MOVETYPE.NONE&& rlens>(1-(Sencsitivity*100) )&& (rlens<((4+Sencsitivity ))) ){
				circleX=Color.red;
				circleY=Color.green;
				circleZ=Color.blue;
		   circleX=SelectedColor;
           SelectedType=MOVETYPE.RX;
		   MouseDown=hit;	
		   locklocalToWorldMatrix=rotationMatrix;
	       rotaCal=0.0f;
		  
		}
		if(SelectedType==MOVETYPE.RX ){
		    Drag= hit;
		   

			float angledown	=Vector3.Dot(MouseDown.normalized, Vector3.forward); 
	        float anglemove =Vector3.Dot(Drag.normalized, Vector3.forward); 
			float angledeta =Vector3.Dot(MouseDown.normalized,Drag.normalized);
		
		   angledown= SnapDot(angledown,MouseDown.z,MouseDown.y);	
			
		   anglemove= SnapDot(anglemove,Drag.z,Drag.y);	
	
		   angledeta=Mathf.Acos(angledeta);
		   
				if(angledown==anglemove){  RenderGizmos();} else{
		   	
		   float judge=angledown+Mathf.PI;
			
		   if(judge>2*Mathf.PI){
		      if(anglemove>=0f && anglemove<=judge-2*Mathf.PI || anglemove>angledown){     
				
				}
			  else
				
					angledeta=-angledeta;
				
			  
				
		   }else{
			   if(anglemove>=angledown && anglemove<=judge){
				}
				else
					angledeta=-angledeta;
					
			
		   }
		   
		   rotaCal=angledeta-rotaCal;
		   
           DrawCam(Color.white,angledown,angledeta,MOVETYPE.X);
					if(NeedContactToTurn == true){
			transform.Rotate( new Vector3(-rotaCal,0,0)*RotationSpeed);
					}else{
			transform.Rotate(Vector3.right*(Input.GetAxis("Mouse X")+Input.GetAxis("Mouse Y"))*RotationSpeed);
					}
		  	
		  
		   rotaCal=angledeta; 
			}
		}
		}//RotationOption end

	 RenderGizmos();
	  
	}

	void RenderGizmos(){
		
		Dir=Camera.main.transform.position-transform.position;
		
		Dir.Normalize();
		
		
		GL.PushMatrix(); 
		rotationMatrix = transform.localToWorldMatrix;
		
		rotationMatrix=Matrix4x4.TRS(transform.position,transform.localRotation,matScal);
		GL.MultMatrix(rotationMatrix); 
		lineMaterial.SetPass(0);
		
		
		
		
		if(RotationOption){
			//project Dir on planeXZ
			Vector3 camdir = transform.InverseTransformPoint(Camera.main.transform.position);  
			camdir = new Vector3(camdir.x +1,camdir.y,camdir.z +1);
			
			Vector3 prjleft =Vector3.Cross((new Vector3(10,0,0)),camdir)* 90;
			prjleft.Normalize();
			prjleft*=SetSpread;
			Vector3 prjfwrd=Vector3.Cross(prjleft,new Vector3(10,0,0));
			prjfwrd.Normalize();	
			prjfwrd*=SetSpread;
			DrawCircle(circleX,prjleft,prjfwrd);
			rForward=prjfwrd;	
			
			
			
			prjleft =Vector3.Cross((new Vector3(0,10,0)),camdir);
			prjleft.Normalize();
			prjleft*=SetSpread;
			prjfwrd=Vector3.Cross(prjleft,(new Vector3(0,10,0)));
			prjfwrd.Normalize();
			prjfwrd*=SetSpread;
			DrawCircle(circleY,prjleft,prjfwrd);
			uForward=prjfwrd;	
			
			prjleft =Vector3.Cross((new Vector3(0,0,10)),camdir) * 15;
			prjleft.Normalize();
			prjleft*=SetSpread;
			prjfwrd=Vector3.Cross(prjleft,new Vector3(0,0,10)) * 10;
			prjfwrd.Normalize();
			prjfwrd*=SetSpread;
			DrawCircle(circleZ,prjleft,prjfwrd);
			fForward=prjfwrd;
		}
		
		if(TransformationOption){	
			DrawAxis(Vector3.right*SetSpread,Vector3.up,Vector3.forward,0.04f*SetSpread,0.9f,selX);	
			DrawAxis(Vector3.up*SetSpread,Vector3.right,Vector3.forward,0.04f*SetSpread,0.9f,selY);
			DrawAxis(Vector3.forward*SetSpread,Vector3.right,Vector3.up,0.04f*SetSpread,0.9f,selZ);
			
			
			DrawQuad(0.3f*SetSpread,false,Vector3.forward,Vector3.up,panX);
			DrawQuad(0.3f*SetSpread,false,Vector3.right,Vector3.forward,panY);
			DrawQuad(0.3f*SetSpread,false,Vector3.right,Vector3.up,panZ);
			
			
		}
		
		if(ScaleOption){
			DrawShpere(Vector3.right*SetSpread,Vector3.up,Vector3.forward,0.04f*SetSpread,0.9f,selX);	
			DrawShpere(Vector3.up*SetSpread,Vector3.right,Vector3.forward,0.04f*SetSpread,0.9f,selY);
			DrawShpere(Vector3.forward*SetSpread,Vector3.right,Vector3.up,0.04f*SetSpread,0.9f,selZ);
			
			
			DrawScale(0.3f*SetSpread,false,Vector3.forward,Vector3.up,panX);
			DrawScale(0.3f*SetSpread,false,Vector3.right,Vector3.forward,panY);
			DrawScale(0.3f*SetSpread,false,Vector3.right,Vector3.up,panZ);
		}
		
		GL.PopMatrix();
		
	}
	
	void DrawCam(Color col, float sang,float eng ,MOVETYPE dtype){
		GL.PushMatrix(); 
		rotationMatrix = locklocalToWorldMatrix;
		
		
		
		GL.MultMatrix(rotationMatrix); 
		lineMaterial.SetPass(0);
		
		
		GL.Begin(GL.TRIANGLES);
		GL.Color(col);
		
		float ang;
		
		switch(dtype){
			
			
		case MOVETYPE.X:
			for(int i=0;i<20;i++){
				
				ang=sang+(eng)*i/20;
				
				GL.Vertex3(0,0,0);
				GL.Vertex3( 0,Mathf.Sin(ang)*SetSpread,Mathf.Cos(ang)*SetSpread);
				ang=sang+(eng)*(i+1)/20;
				GL.Vertex3( 0,Mathf.Sin(ang)*SetSpread,Mathf.Cos(ang)*SetSpread);
				
			}  
			break;
			
		case MOVETYPE.Y:
			for(int i=0;i<20;i++){
				
				ang=sang+(eng)*i/20;
				
				GL.Vertex3(0,0,0);
				GL.Vertex3( Mathf.Cos(ang)*SetSpread,0,Mathf.Sin(ang)*SetSpread);
				ang=sang+(eng)*(i+1)/20;
				GL.Vertex3( Mathf.Cos(ang)*SetSpread,0,Mathf.Sin(ang)*SetSpread);
				
			}  
			break;
		case MOVETYPE.Z:
			for(int i=0;i<20;i++){
				
				ang=sang+(eng)*i/20;
				
				GL.Vertex3(0,0,0);
				GL.Vertex3( Mathf.Sin(ang)*SetSpread,Mathf.Cos(ang)*SetSpread,0);
				ang=sang+(eng)*(i+1)/20;
				GL.Vertex3( Mathf.Sin(ang)*SetSpread,Mathf.Cos(ang)*SetSpread,0);
				
			}  
			break;
		}
		
		
		
		
		
		GL.End();
		GL.PopMatrix();
		
	}
	
	
	float  SnapDot(float dot,float x,float y){
		float thd=0.0f;  
		if(dot>=0){
			
			if(y>0)
				thd=Mathf.Acos( dot);
			else
				thd=2*Mathf.PI-Mathf.Acos( dot);
		}else{
			if(y>0)
				thd=Mathf.Acos( dot);
			else
				thd=2*Mathf.PI-Mathf.Acos( dot);
			
			
		}
		return thd;
	}
	
	
	void DrawCircle(Color col, Vector3 vtx, Vector3 vty){
		
		
		GL.Begin(GL.LINES);
		GL.Color(col);
		if (NeedContactToTurn == false) {
			for (int i = 0; i < 200; i++) {
				if (true) {
					Vector3 vt;
					vt = vtx * Mathf.Cos ((Mathf.PI / 100) * i);
					vt += vty * Mathf.Sin ((Mathf.PI / 100) * i);
					GL.Vertex3 (vt.x, vt.y, vt.z);
					vt = vtx * Mathf.Cos ((Mathf.PI / 100) * (i + 1));
					vt += vty * Mathf.Sin ((Mathf.PI / 100) * (i + 1));
					
					GL.Vertex3 (vt.x, vt.y, vt.z);
				}
			}
		} else {
			for (int i = 0; i < 100; i++) {
				if (true) {
					Vector3 vt;
					vt = vtx * Mathf.Cos ((Mathf.PI / 100) * i);
					vt += vty * Mathf.Sin ((Mathf.PI / 100) * i);
					GL.Vertex3 (vt.x, vt.y, vt.z);
					vt = vtx * Mathf.Cos ((Mathf.PI / 100) * (i + 1));
					vt += vty * Mathf.Sin ((Mathf.PI / 100) * (i + 1));
					
					GL.Vertex3 (vt.x, vt.y, vt.z);
				}
			}
		}
		GL.End();
	}
	void DrawAxis(Vector3 axis, Vector3 vtx,Vector3 vty, float fct,float fct2,Color col){
		GL.Begin(GL.LINES);
		GL.Color(col);
		GL.Vertex3(0,0,0);
		
		GL.Vertex(axis);
		GL.End();
		
		
		GL.Begin(GL.TRIANGLES);
		GL.Color(col);
		for (int i=0;i<=30;i++)
		{
			
			Vector3 pt;
			pt = vtx * Mathf.Cos(((2*Mathf.PI)/20.0f)*i)*fct*2;
			pt+= vty * Mathf.Sin(((2*Mathf.PI)/20.0f)*i)*fct*2;
			pt+=axis*fct2;
			
			GL.Vertex(pt);
			pt = vtx * Mathf.Cos(((2*Mathf.PI)/20.0f)*(i+1))*fct *2;
			pt+= vty * Mathf.Sin(((2*Mathf.PI)/20.0f)*(i+1))*fct*2;
			pt+=axis*fct2;
			
			GL.Vertex(pt);
			GL.Vertex(axis);
			
			
			
		}
		GL.End();
	}
	
	void DrawShpere(Vector3 axis, Vector3 vtx,Vector3 vty, float fct,float fct2,Color col){
		
		
		
		
		GL.Begin(GL.LINES);
		GL.Color(col);
		GL.Vertex3(0,0,0);
		
		GL.Vertex(axis);
		GL.End();
		
		
		GL.Begin(GL.TRIANGLE_STRIP);
		GL.Color(col);
		for (int i=0;i<=30;i++)
		{
			
			Vector3 pt;
			pt = vtx * Mathf.Cos(((5*Mathf.PI)/20.0f)*i)*fct;
			pt+= vty * Mathf.Sin(((5*Mathf.PI)/20.0f)*i)*fct;
			pt+=axis*fct2*1.2f;
			GL.Vertex(pt);
			
			pt = vtx * Mathf.Cos(((5*Mathf.PI)/20.0f)*(i+1))*fct;
			pt+= vty * Mathf.Sin(((5*Mathf.PI)/20.0f)*(i+1))*fct;
			pt+=axis*fct2 * 1.2f;
			
			GL.Vertex(pt);
			
			pt = vtx * Mathf.Cos(((5*Mathf.PI)/20.0f)*i)*fct;
			pt+= vty * Mathf.Sin(((5*Mathf.PI)/20.0f)*i)*fct;
			pt+=axis*fct2;
			
			
			GL.Vertex(pt);
			pt = vtx * Mathf.Cos(((5*Mathf.PI)/20.0f)*(i+1))*fct;
			pt+= vty * Mathf.Sin(((5*Mathf.PI)/20.0f)*(i+1))*fct;
			pt+=axis*fct2;
			
			
			GL.Vertex(pt);
			GL.Vertex(new Vector3(axis.x,axis.y,axis.z));
			
			
			
		}
		GL.End();
	}
	
	void DrawQuad(float size, bool bSelected, Vector3 axisU,Vector3 axisV,Color col){
		
		Vector3 []pts=new Vector3[4];
		pts[1] = (axisU * size);
		pts[2] = (axisU + axisV)*size;
		pts[3] = (axisV * size);
		
		
		GL.Begin(GL.QUADS);
		col.a=0.15f;
		if (!bSelected)
			GL.Color(col);
		else
			GL.Color(Color.white);
		GL.End();
		
		
		GL.Begin(GL.QUADS);
		col.a=0.15f;
		if (!bSelected)
			GL.Color(col);
		else
			GL.Color(Color.white);
		GL.Vertex(pts[0]);
		GL.Vertex(pts[1]);
		GL.Vertex(pts[1]);
		GL.Vertex(pts[2]);
		GL.Vertex(pts[2]);
		GL.Vertex(pts[3]);
		GL.Vertex(pts[3]);
		GL.Vertex(pts[0]);
		
		GL.End();
		
	}
	
	void DrawScale(float size, bool bSelected, Vector3 axisU,Vector3 axisV,Color col){
		
		Vector3 []pts=new Vector3[3];
		pts[1] = (axisU * size);
		pts[2] = (axisV * size);
		
		
		
		GL.Begin(GL.QUADS);
		col.a=0.15f;
		if (!bSelected)
			GL.Color(col);
		else
			GL.Color(Color.white);
		GL.End();
		
		
		GL.Begin(GL.QUADS);
		col.a=0.15f;
		if (!bSelected)
			GL.Color(col);
		else
			GL.Color(Color.white);
		GL.Vertex(pts[0]);
		GL.Vertex(pts[1]);
		GL.Vertex(pts[2]);
		GL.Vertex(pts[1]);
		GL.Vertex(pts[2]);
		GL.Vertex(pts[1]);
		GL.Vertex(pts[2]);
		GL.Vertex(pts[0]);
		
		GL.End();
		
	}
	void TurnOnOutline(){
        //if (gameObject.GetComponent<Collider>() == null || gameObject.GetComponent<Collider>().enabled == false) {
        //    Debug.LogError("You Cannot use Code('SelectedObjOutline') without a Collider");
        //    return;
        //}
		

		
		
        //GL.PushMatrix(); // Save Matrix
		
        //lineMaterial.SetPass(0); // set layer of rendering
		
        //DrawOutline(); // Draws the primitives
		
        //GL.PopMatrix(); // Restore Matrix
	}

	void DrawOutline()
	{
		if (AdvancedSettings == false) {
			lines = 1;
		}
		ColliderBounds = gameObject.GetComponent<Collider>().bounds;
		
		
		GL.Begin(GL.LINES); 
		if (UseOwnColor == false) {
			GL.Color (new Color (1f, 1f, 1f, 0.8f));
		} else {
			GL.Color(LineColor);		
		}

		/* This section of the code makes sure that the box ajusts to the rotation of the object*/
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2-ColliderBounds.size.x/lines,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2-ColliderBounds.size.y/lines,transform.position.z+ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2-ColliderBounds.size.z/lines);
		
		
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2+ColliderBounds.size.x/lines,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2-ColliderBounds.size.y/lines,transform.position.z+ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2-ColliderBounds.size.z/lines);
		
		
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2-ColliderBounds.size.x/lines,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2+ColliderBounds.size.y/lines,transform.position.z+ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2-ColliderBounds.size.z/lines);
		
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2+ColliderBounds.size.x/lines,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2+ColliderBounds.size.y/lines,transform.position.z+ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z+ColliderBounds.size.z/2-ColliderBounds.size.z/lines);
		
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2+ColliderBounds.size.x/lines,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2+ColliderBounds.size.y/lines,transform.position.z-ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2+ColliderBounds.size.z/lines);
		
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2+ColliderBounds.size.x/lines,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2-ColliderBounds.size.y/lines,transform.position.z-ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x-ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2+ColliderBounds.size.z/lines);
		
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2-ColliderBounds.size.x/lines,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2-ColliderBounds.size.y/lines,transform.position.z-ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y+ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2+ColliderBounds.size.z/lines);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2-ColliderBounds.size.x/lines,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2+ColliderBounds.size.y/lines,transform.position.z-ColliderBounds.size.z/2);
		
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2);
		GL.Vertex3(transform.position.x+ColliderBounds.size.x/2,transform.position.y-ColliderBounds.size.y/2,transform.position.z-ColliderBounds.size.z/2+ColliderBounds.size.z/lines);
		GL.End();
		
	}
	
}
