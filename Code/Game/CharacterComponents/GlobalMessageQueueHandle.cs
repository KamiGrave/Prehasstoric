using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using AssGameFramework.Events;
using Godot;

namespace ProjectPrehasstoric
{
    public class GlobalMessage : ModelMessage
    {
        public CharacterMessage Message {get;set;} = null;

        public GlobalMessage(CharacterMessage message)
        {
            Message = message;
        }
    }
    public class GlobalMessageQueueHandle : NodeComponent, IModelMessageHandler<GlobalMessage>
    {
        public GlobalMessageQueueHandle()
        {
            GlobalMessageQueue.Instance.MessagePostedEvent += ReceieveGlobalMessage;
        }
        public void HandleMessage(GlobalMessage message)
        {
            GlobalMessageQueue.Instance?.PostMessage(message.Message);
        }

        public void ReceieveGlobalMessage(CharacterMessage message)
        {
            Model.AddModelMessage(message);
        }
    }
}