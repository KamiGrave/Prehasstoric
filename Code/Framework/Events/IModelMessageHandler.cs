using System;
using System.Collections.Generic;
using System.Linq;

namespace AssGameFramework.Events
{
    public interface IModelMessageHandler<in MessageType> where MessageType : ModelMessage
    {
        void HandleMessage(MessageType mdlmsg);
    }

    public class MessageHandlerHelper
    {
        public static bool TestType(object obj)
        {
             return obj.GetType().GetInterfaces().Any(x =>
                            x.IsGenericType &&
                            x.GetGenericTypeDefinition() == typeof(IModelMessageHandler<>));
        }

        public static Type[] GetTypes(object messageHandler)
        {
            return messageHandler.GetType().GetInterfaces().Where(x => x.IsGenericType &&
                                                                x.GetGenericTypeDefinition() == typeof(IModelMessageHandler<>)).ToArray();
        }

        public static IModelMessageHandler<T> CastTo<T>(object messageHandler) where T : ModelMessage
        {
            return (IModelMessageHandler<T>)messageHandler;
        }
    }
}