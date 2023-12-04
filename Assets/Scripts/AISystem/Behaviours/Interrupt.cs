using AISystem.Data;
using AISystem.BehaviourTrees;
using UnityEngine;

namespace AISystem.Behaviours
{
    public class Interrupt : ActionNode
    {
        Animator m_animator;
        float m_startTime;
        public float m_duration;

        // Avtivates the Interruption animation and delays their movement until that is finished.

        protected override void BeginNode()
        {
            base.BeginNode();
            m_startTime = Time.time;
            m_animator = m_input.m_go.GetComponentInChildren<Animator>();
            m_input.m_aIMovement.m_swordCollider.enabled = false;
            m_input.m_aIMovement.SetWarp(false);
            m_animator.SetFloat("ForwardsBackwards", -0.3f);
            m_animator.SetFloat("Sideways", 0);
            m_animator.SetTrigger("Interrupt");
            m_input.m_aIMovement.m_isInterrupted = false;
            m_input.m_aIMovement.Stop();

        }
        protected override NodeState Update(float dt)
        {
            if (Time.time < m_startTime + m_duration)
            {
                m_input.m_aIMovement.m_swordCollider.enabled = false;
                return NodeState.RUNNING;
            }

            m_input.m_aIMovement.SetWarp(true);
            return NodeState.SUCCESS;
        }

    }
}
