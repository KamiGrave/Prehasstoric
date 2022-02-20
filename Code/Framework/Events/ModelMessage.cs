using AssGameFramework.StateMachines;

namespace AssGameFramework.Events
{
    public interface IModelMessage {}
    public abstract class ModelMessage : IModelMessage, IStateMachineMessage
    {
        public virtual bool SupressLogs => true;
    }

    public abstract class DebugMessage : IModelMessage
    {
        public class PrintState : DebugMessage {}
    }
}