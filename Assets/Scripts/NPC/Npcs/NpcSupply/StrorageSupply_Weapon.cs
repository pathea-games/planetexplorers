using UnityEngine;
using Pathea;
using System.Collections;
using System.Collections.Generic;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;
using ItemAsset;

namespace Pathea
{
    public  class StrorageSupply_Weapon : StrorageSupply
    {
        public override ESupplyType Type
        {
            get { return ESupplyType.Weapon; }
        }

       public override bool DoSupply (PeEntity entity, CSAssembly CsAssembly)
		{
			throw new System.NotImplementedException ();
		}
    }
}
   

