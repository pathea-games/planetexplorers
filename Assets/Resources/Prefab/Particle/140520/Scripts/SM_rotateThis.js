var rotationSpeedX:float=90;
var rotationSpeedY:float=0;
var rotationSpeedZ:float=0;
var local:boolean=true;
private var rotationVector:Vector3=Vector3(rotationSpeedX,rotationSpeedY,rotationSpeedZ);



function Update () {

if (local==true) transform.Rotate(Vector3(rotationSpeedX,rotationSpeedY,rotationSpeedZ)*Time.deltaTime);
if (local==false) transform.Rotate(Vector3(rotationSpeedX,rotationSpeedY,rotationSpeedZ)*Time.deltaTime, Space.World);

}