using UnityEngine;

namespace AISystem.BehaviourTrees
{
    public abstract class DecoratorNode : Node
    {
        [field: SerializeReference, HideInInspector] public Node m_child { get; set; }
        public void SetChild(Node child) => m_child = child;
    }
}
