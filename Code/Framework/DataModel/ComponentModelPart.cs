using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssGameFramework.DataModel
{
    /// <summary>
    /// ComponentModelPart is used by the Model to contain components that 
    /// don't belong to any particular <see cref="ModelPart"/>
    /// </summary>
    public class ComponentModelPart : ModelPart
    {
        public ComponentModelPart()
        {
            Name = "ComponentModelPart";
        }
    }
}