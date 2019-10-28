namespace PatheaScript
{
    public abstract class Compare
    {
        public abstract bool Do(VarValue lhs, VarValue rhs);
    }

    public class Greater : Compare
    {
        public override bool Do(VarValue lhs, VarValue rhs)
        {
            return lhs > rhs;
        }
    }

    public class GreaterEqual : Compare
    {
        public override bool Do(VarValue lhs, VarValue rhs)
        {
            return (lhs > rhs) || (lhs == rhs);
        }
    }

    public class Equal : Compare
    {
        public override bool Do(VarValue lhs, VarValue rhs)
        {
            return lhs == rhs;
        }
    }

    public class NotEqual : Compare
    {
        public override bool Do(VarValue lhs, VarValue rhs)
        {
            return lhs != rhs;
        }
    }

    public class Lesser : Compare
    {
        public override bool Do(VarValue lhs, VarValue rhs)
        {
            return lhs < rhs;
        }
    }

    public class LesserEqual : Compare
    {
        public override bool Do(VarValue lhs, VarValue rhs)
        {
            return (lhs < rhs) || (lhs == rhs);
        }
    }
}