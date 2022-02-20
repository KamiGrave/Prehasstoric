using System;
using System.Diagnostics;
using AssGameFramework.Components;
using AssGameFramework.DataModel;
using Godot;

namespace AssGameFramework.Components
{
    /// <summary>
    /// an EntityComponent class that manipulates an <see cref="Entity"/> object.
    /// </summary>
    public abstract class NodeComponent : Godot.Node
    {
        private ModelPart _modelPart = null;
        /// <summary>
        /// Target <see cref="ModelPart"/> that owns the <see cref="NodeComponent"/>.
        /// </summary>
        public ModelPart ModelPart
        {
            get { return _modelPart; }
            internal set
            {
                /* Handling old ModelPart is done at the ModelPart level
                so just handle activation with the new part */
                _modelPart = value;
            }
        }

        /// <summary>
        /// The <see cref="EntityModel"/> that owns the 
        /// <see cref="ModelPart"/> that owns this component
        /// </summary>
        public DataModel.Model Model { get { return ModelPart?.Model; } }

        /// <summary>
        /// The target <see cref="Entity"/> that this component manipulates
        /// </summary>
        public Node Target { get { return ModelPart?.Model.GetParent(); } }

        /// <summary>
        /// Creates a component.
        /// </summary>
        public NodeComponent()
        {
        }

        internal virtual void OnAttachedToModel()
        {
            Debug.Assert(Target != null, "Cannot subscribe to null Target");
        }


        internal virtual void OnRemovedFromModel()
        {
            Debug.Assert(Target != null, "Cannot unsubscribe from null Target");
        }
    }
}