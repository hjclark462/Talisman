namespace AISystem.BehaviourTrees
{
    public abstract class ConditionalNode : Node
    {
        protected override NodeState Update(float dt) => Compare(dt) ? NodeState.SUCCESS : NodeState.FAILURE;
        protected abstract bool Compare(float dt);
    }
}
