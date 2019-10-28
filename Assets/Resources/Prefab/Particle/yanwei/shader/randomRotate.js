#pragma strict
private var rotTarget:Quaternion;
var rotateEverySecond:float = 1;

function Start () {
	randomRot ();
	InvokeRepeating("randomRot", 0,rotateEverySecond);
}

function Update(){
	transform.rotation = Quaternion.Lerp(transform.rotation, rotTarget, Time.time*Time.deltaTime);

}

function randomRot () {
	 rotTarget = Random.rotation;

}