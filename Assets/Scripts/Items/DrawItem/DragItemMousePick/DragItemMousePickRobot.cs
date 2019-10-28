using WhiteCat;

public class DragItemMousePickRobot : DragItemMousePickCreation
{
	RobotController _controller;


	public void Init(RobotController controller)
	{
		_controller = controller;
	}


	public override bool CanCmd()
    {
        if (!base.CanCmd())
        {
            return false;
        }

		return !_controller.isActive;
	}


    protected override void InitCmd(CmdList cmdList)
    {
        cmdList.Add("Get", OnGetBtn);
    }


    protected override string tipsText
    {
        get
        {
            return base.tipsText;
        }
    }
}