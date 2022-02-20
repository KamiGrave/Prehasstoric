using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using AssGameFramework.DataModel;
using AssGameFramework.Events;
using AssGameFramework.StateMachines;

namespace ProjectPrehasstoric
{
    public class StateMachineModelManager
    {
        static private StateMachineModelManager _s3m;

        static public StateMachineModelManager Instance
        {
            get
            {
                if(_s3m == null) _s3m = new StateMachineModelManager();

                return _s3m;
            }
        }

        private Dictionary<Type, object> ModelLookup {get;set;} = new Dictionary<Type, object>();

        public FSMModel GetModel<FSMModel>() where FSMModel : class, new()
        {
            Type modelType = typeof(FSMModel);
            if(!IsSubclassOfRawGeneric(typeof(StateMachineModel<,,>), modelType))
            {
                Debug.Fail("Attempt to get a non-StateMachineModel from StateMachineModelManager.");
                return null;
            }

            if(ModelLookup.ContainsKey(modelType))
            {
                FSMModel model = ModelLookup[modelType] as FSMModel;
                return model;
            }
            else
            {
                FSMModel model = new FSMModel();
                ModelLookup[modelType] = model;
                return model;
            }
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}