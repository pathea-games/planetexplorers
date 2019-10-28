using UnityEngine;
using Pathea;
using System.Collections;
using System.Collections.Generic;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;
using ItemAsset;

namespace Pathea
{
    public abstract  class  StrorageSupply
    {
        public abstract ESupplyType Type { get; }

        public abstract bool DoSupply(PeEntity entity,CSAssembly CsAssembly);

    }
}

