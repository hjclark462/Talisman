using AISystem.Data;
using AISystem.BehaviourTrees;

namespace AISystem.Behaviours
{
    public class IsTargetDead : ConditionalNode
    {
        public bool soIcanfind;
        protected override bool Compare(float dt)
        {
            UnityEngine.Debug.Log("IsTargetDead node");
            if (m_input.m_aIKnowledge.CanSeeBeing(out BeingKnowledge info))
            {
                m_input.m_blackboard.m_target = info.m_being;
                PlayerController pc = (PlayerController)m_input.m_blackboard.m_target;
                UnityEngine.Debug.Log(pc.m_currentHealth <= 0);
                return pc.m_currentHealth <= 0;
            }
            return false;
        }
    }
}
