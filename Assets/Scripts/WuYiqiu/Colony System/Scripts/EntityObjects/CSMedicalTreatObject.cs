using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CSMedicalTreatObject:CSCommonObject
{
    public CSMedicalTreat m_Treat { get { return m_Entity == null ? null : m_Entity as CSMedicalTreat; } }

    #region ENTITY_OBJECT_FUNC

    #endregion
}
