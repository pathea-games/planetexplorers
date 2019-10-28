using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class CSPersonnel
{
    public bool IsTraining
    {
        get { return Data.m_IsTraining; }
        set
        {
            Data.m_IsTraining = value;
            UpdateNpcCmptTraining();
        }
    }

    public ETrainingType trainingType{
        get{return (ETrainingType)Data.m_TrainingType;}
        set { Data.m_TrainingType = (int)value; UpdateNpcCmptTraining(); }
    }

    public ETrainerType trainerType{

        get { return (ETrainerType)Data.m_TrainerType; }
        set { Data.m_TrainerType = (int)value; UpdateNpcCmptTraining(); }
    }

}
