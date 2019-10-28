using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
//  Schedule For soldier. when the personnel is a soldier,
//		
//   	The most function in this partial class will be invoked.
//

public partial class  CSPersonnel : PersonnelBase 
{

    public List<CSEntity> guardEntities;
    public List<CSEntity> GuardEntities
    {
        get { return guardEntities; }
        set
        {
            guardEntities = value;
            UpdateNpcCmptGuardEntities();
        }
    }

}
