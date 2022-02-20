using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using AssGameFramework.DataModel;
using AssGameFramework.StateMachines;
using Godot;

namespace AssGameFramework.ASSDebug
{
    public class StateMachineDebugHelper
    {
        public static void PrintAllStateMachineStates(Model dataModel)
        {
            foreach(ModelPart part in dataModel.PartLookup.Values)
            {
                foreach(NodeComponent comp in part.Components.List)
                {
                        if(comp is IDebuggable debugComp)
                        {
                            GD.Print(debugComp.DebugString);
                        }
                }
            }
        }
    }
}