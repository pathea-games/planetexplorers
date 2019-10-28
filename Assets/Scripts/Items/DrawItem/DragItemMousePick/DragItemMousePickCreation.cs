
public class DragItemMousePickCreation : DragItemMousePick 
{
	protected override void OnStart()
	{
		GetComponentInParent<WhiteCat.CreationController>().AddBuildFinishedListener(CollectColliders);
	}
}
