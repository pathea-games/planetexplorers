var fadeInTime:float=0.1;
var stayTime:float=1;
var fadeOutTime:float=0.7;
var thisTrail:TrailRenderer;
private var timeElapsed:float=0;
private var timeElapsedLast:float=0;
private var percent:float;


function Start ()
{
thisTrail.material.SetColor ("_TintColor", Color(0.5,0.5,0.5,1));
if(fadeInTime<0.01) fadeInTime=0.01; //hack to avoid division with zero
percent=timeElapsed/fadeInTime;

}


function Update () {
timeElapsed+=Time.deltaTime;


if(timeElapsed<=fadeInTime) //fade in
{
percent=timeElapsed/fadeInTime;
thisTrail.material.SetColor ("_TintColor", Color(0.5,0.5,0.5, percent));
}

if((timeElapsed>fadeInTime)&&(timeElapsed<fadeInTime+stayTime)) //set the normal color
{
thisTrail.material.SetColor ("_TintColor", Color(0.5,0.5,0.5,1));
}

if(timeElapsed>=fadeInTime+stayTime&&timeElapsed<fadeInTime+stayTime+fadeOutTime) //fade out
{
timeElapsedLast+=Time.deltaTime;
percent=1-(timeElapsedLast/fadeOutTime);
thisTrail.material.SetColor ("_TintColor", Color(0.5,0.5,0.5, percent));
}



}

