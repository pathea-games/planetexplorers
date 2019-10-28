
public class ItemScript_Bed : ItemScript
{
    Pathea.Operate.PESleep mSleep;
    public Pathea.Operate.PESleep peSleep
    {
        get
        {
            return mSleep;
        }
    }

    public override void OnConstruct()
    {
        base.OnConstruct();

        mSleep = GetComponentInChildren<Pathea.Operate.PESleep>();

        if (mSleep == null)
        {
            Pathea.Operate.PEBed bed = GetComponentInParent<Pathea.Operate.PEBed>();
            if (bed == null || bed.sleeps == null || bed.sleeps.Length == 0)
            {
                return;
            }
            mSleep = bed.sleeps[0];
        }
    }
}
