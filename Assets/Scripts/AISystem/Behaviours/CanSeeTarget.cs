using AISystem.Data;
using AISystem.BehaviourTrees;

namespace AISystem.Behaviours
{
    public class CanSeeTarget : ConditionalNode
    {
        protected override bool Compare(float dt)
        {
            UnityEngine.Debug.Log("Can see target node");
            if (m_input.m_aIKnowledge.CanSeeBeing(out BeingKnowledge info))
            {
                m_input.m_blackboard.m_target = info.m_being;
                return true;
            }
            return false;
        }
    }
}
