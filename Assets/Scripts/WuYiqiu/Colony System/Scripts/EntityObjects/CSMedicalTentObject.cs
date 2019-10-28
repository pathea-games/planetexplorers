using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CSMedicalTentObject:CSCommonObject
{
    public CSMedicalTent m_Tent { get { return m_Entity == null ? null : m_Entity as CSMedicalTent; } }

    #region ENTITY_OBJECT_FUNC

    #endregion
}