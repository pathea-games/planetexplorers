using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;

namespace Behave.Runtime
{
    public interface IBehave : IAgent
    {
        bool BehaveActive { get; }
    }

    public delegate object BTInputDelegate(params object[] args);
    public delegate void BTOutputDelegate(params object[] args);
}
