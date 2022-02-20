using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Events;

namespace ProjectPrehasstoric
{
    public class GlobalMessageQueue
    {
        private static GlobalMessageQueue _gmq = null;

        public static GlobalMessageQueue Instance
        {
            get
            {
                if(_gmq == null) _gmq = new GlobalMessageQueue();
                return _gmq;
            }
        }

        public delegate void MessagePostedDelegate(CharacterMessage message);

        public event MessagePostedDelegate MessagePostedEvent;

        private GlobalMessageQueue()
        {
        }


        public void PostMessage(CharacterMessage message)
        {
            MessagePostedEvent?.Invoke(message);
        }
    }
}