// Stub: Behave proprietary runtime types
// BehaveResult has 2110 references -- the most-used stub type in the project.
// Only stubs types from the proprietary Behave DLL. In-house code (BTAgent, BTResolver,
// BTLauncher, BTAction, BehaveAction, BehaveAttribute, Reflecter, BTCoroutine,
// BTExtension, BTInterface, BehaveAgent, BehaveLibrary) lives in
// Assets/Scripts/BehaviorTree/Scripts/ and is NOT duplicated here.

using System;

namespace Behave.Runtime
{
    /// <summary>
    /// Result of a behavior tree tick. 2110 total references across the codebase.
    /// </summary>
    public enum BehaveResult
    {
        Success,
        Failure,
        Running
    }

    /// <summary>
    /// Agent interface for behavior tree callbacks. Implemented by BehaveAgent (in-house)
    /// and by game entity AI components via IBehave.
    /// </summary>
    public interface IAgent
    {
        void Reset(Tree sender);
        int SelectTopPriority(Tree sender, params int[] IDs);
        BehaveResult Tick(Tree sender);
    }

    /// <summary>
    /// Delegate for tick/init forwards on individual tree actions.
    /// Signature: (Tree sender) -> BehaveResult. Used via reflection in BTAgent.SetTreeForward.
    /// </summary>
    public delegate BehaveResult TickForward(Tree sender);

    /// <summary>
    /// Delegate for reset forwards on individual tree actions.
    /// Signature: (Tree sender) -> void. Used via reflection in BTAgent.SetTreeForward.
    /// </summary>
    public delegate void ResetForward(Tree sender);

    /// <summary>
    /// Proprietary behavior tree runtime. Instantiated via Reflecter from compiled Behave libraries.
    /// Game code accesses Frequency, LibraryActions, LastTickedAction, Tick, Reset,
    /// SetInitForward, SetTickForward, SetResetForward.
    /// </summary>
    public class Tree
    {
        /// <summary>
        /// Tick rate in Hz. BTAgent uses 1/Frequency as WaitForSeconds interval.
        /// </summary>
        public float Frequency { get; set; }

        /// <summary>
        /// Enum type containing all action names for this tree's library.
        /// Used for Enum.Parse to map action names to integer indices.
        /// </summary>
        public Type LibraryActions { get; set; }

        /// <summary>
        /// Index of the last action that was ticked. Used for profiling/debug.
        /// </summary>
        public int LastTickedAction { get; set; }

        /// <summary>
        /// The currently active agent being ticked. Used by action implementations
        /// to access the owning entity without explicit parameter passing.
        /// </summary>
        public object ActiveAgent { get; set; }

        /// <summary>
        /// A string parameter passed to the active action. Used for data lookup keys.
        /// </summary>
        public string ActiveStringParameter { get; set; }

        /// <summary>
        /// Tick the tree without arguments. Used by AiBehave.cs.
        /// </summary>
        public BehaveResult Tick()
        {
            return BehaveResult.Success;
        }

        /// <summary>
        /// Tick the tree with an agent and optional data context.
        /// </summary>
        public BehaveResult Tick(IAgent agent, object data)
        {
            return BehaveResult.Success;
        }

        /// <summary>
        /// Parameterless reset -- resets all tree state.
        /// </summary>
        public void Reset() { }

        /// <summary>
        /// Reset with full context -- agent, tree, and data.
        /// Called by BTAgent.Stop().
        /// </summary>
        public void Reset(IAgent agent, Tree tree, object data) { }

        /// <summary>
        /// Register an init forward delegate for the action at the given index.
        /// </summary>
        public void SetInitForward(int actionIndex, TickForward forward) { }

        /// <summary>
        /// Register a tick forward delegate for the action at the given index.
        /// </summary>
        public void SetTickForward(int actionIndex, TickForward forward) { }

        /// <summary>
        /// Register a reset forward delegate for the action at the given index.
        /// </summary>
        public void SetResetForward(int actionIndex, ResetForward forward) { }

        /// <summary>
        /// Utility to convert a boolean to BehaveResult (true=Success, false=Failure).
        /// </summary>
        public static BehaveResult Result(bool value)
        {
            return value ? BehaveResult.Success : BehaveResult.Failure;
        }
    }
}

namespace Behave
{
    /// <summary>
    /// Priority levels for NPC behaviors. Referenced as Behave.EPriority.p_5 etc.
    /// </summary>
    public enum EPriority
    {
        p_1,
        p_2,
        p_3,
        p_4,
        p_5
    }

    /// <summary>
    /// Behavior tree execution states. Referenced as Behave.EState.Preparing etc.
    /// </summary>
    public enum EState
    {
        Preparing,
        Pause,
        Finished
    }
}

namespace Behave.Assets
{
    /// <summary>
    /// Base component in a Behave asset tree structure.
    /// </summary>
    public class Component
    {
    }

    /// <summary>
    /// Action node in a Behave asset tree.
    /// </summary>
    public class Action : Component
    {
    }

    /// <summary>
    /// Decorator node in a Behave asset tree.
    /// </summary>
    public class Decorator : Component
    {
    }

    /// <summary>
    /// Reference node in a Behave asset tree (cross-tree reference).
    /// </summary>
    public class Reference : Component
    {
    }

    /// <summary>
    /// Asset representation of a behavior tree. Different from Runtime.Tree.
    /// Used in editor code (AiBehaveEditor) for tree inspection.
    /// </summary>
    public class Tree
    {
        public Component[] Components = new Component[0];
    }
}
