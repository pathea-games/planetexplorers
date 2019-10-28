using ScenarioRTL.IO;
using System.IO;

namespace ScenarioRTL
{
	public abstract class Action : StatementObject
    {
		public abstract bool Logic ();

		public virtual void StoreState (BinaryWriter w) {}
		public virtual void RestoreState (BinaryReader r) {}  
    }
}
