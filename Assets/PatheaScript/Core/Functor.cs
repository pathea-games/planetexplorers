using Vector3 = UnityEngine.Vector3;

namespace PatheaScript
{
    public abstract class Functor
    {
        protected Variable mTarget;
        protected Variable mArg;

        public Variable Target
        {
            get
            {
                return mTarget;
            }
        }

        public void Set(Variable target, Variable arg)
        {
            mTarget = target;
            mArg = arg;
        }

        public virtual void Do()
        {
            throw new System.NotImplementedException(this.ToString());
        }

        public override string ToString()
        {
            return string.Format("Func:{0}, Target:{1}, Arg:{2}", this.GetType().ToString(), mTarget, mArg);
        }
    }

    public class FunctorSet : Functor
    {
        public override void Do()
        {
            mTarget.Value = mArg.Value;
        }
    }

    public class FunctorPlus : Functor
    {
        public override void Do()
        {
            mTarget.Value = mTarget.Value + mArg.Value;
        }
    }

    public class FunctorMinus : Functor
    {
        public override void Do()
        {
            mTarget.Value = mTarget.Value - mArg.Value;
        }
    }

    public class FunctorMultiply : Functor
    {
        public override void Do()
        {
            mTarget.Value = mTarget.Value * mArg.Value;
        }
    }

    public class FunctorDivide : Functor
    {
        public override void Do()
        {
            mTarget.Value = mTarget.Value / mArg.Value;
        }
    }

    public class FunctorMod : Functor
    {
        public override void Do()
        {
            mTarget.Value = mTarget.Value % mArg.Value;
        }
    }

    public class FunctorPower : Functor
    {
        public override void Do()
        {
            mTarget.Value = VarValue.Power(mTarget.Value, mArg.Value);
        }
    }

    public class FunctorNot : Functor
    {
        public override void Do()
        {
            mTarget.Value = mTarget.Value ^ mArg.Value;
        }
    }
}