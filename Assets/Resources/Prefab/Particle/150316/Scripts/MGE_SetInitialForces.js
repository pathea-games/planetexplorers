#pragma strict

var relativeForce:boolean=true;

var x:float;
var xDeviation:float; // deviation means how much randomity it has, for example a value of 3, deviation 1 can be anything from 2 to 4

var y:float;
var yDeviation:float;

var z:float;
var zDeviation:float;

var relativeTorque:boolean=true;
var torqueScale:float=100; // scale up torque power

var xRot:float;
var xRotDeviation:float;

var yRot:float;
var yRotDeviation:float;

var zRot:float;
var zRotDeviation:float;

//private var rnd:float; //this variable is just to store temporarly random numbers

function Start () {



if (relativeForce==true) 	GetComponent.<Rigidbody>().AddRelativeForce(Random.Range(x-xDeviation, x+xDeviation), Random.Range(y-yDeviation, y+yDeviation), Random.Range(z-zDeviation, z+zDeviation));
if (relativeForce==false) 			GetComponent.<Rigidbody>().AddForce(Random.Range(x-xDeviation, x+xDeviation), Random.Range(y-yDeviation, y+yDeviation), Random.Range(z-zDeviation, z+zDeviation));

if (relativeTorque==true) 		GetComponent.<Rigidbody>().AddRelativeTorque(Random.Range(xRot-xRotDeviation, xRot+xRotDeviation)*torqueScale, Random.Range(yRot-yRotDeviation, yRot+yRotDeviation)*torqueScale, Random.Range(zRot-zRotDeviation, zRot+zRotDeviation)*torqueScale);
if (relativeTorque==false) 				GetComponent.<Rigidbody>().AddTorque(Random.Range(xRot-xRotDeviation, xRot+xRotDeviation)*torqueScale, Random.Range(yRot-yRotDeviation, yRot+yRotDeviation)*torqueScale, Random.Range(zRot-zRotDeviation, zRot+zRotDeviation)*torqueScale);

}

function FixedUpdate () {

}