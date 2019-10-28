using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class CSMedicalCheckObject : CSCommonObject
{
    public CSMedicalCheck m_Check { get { return m_Entity == null ? null : m_Entity as CSMedicalCheck; } }

    #region ENTITY_OBJECT_FUNC

    #endregion

}
