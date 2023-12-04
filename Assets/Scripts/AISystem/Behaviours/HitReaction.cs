using AISystem.Data;
using AISystem.BehaviourTrees;
using UnityEngine;

namespace AISystem.Behaviours
{
    public class HitReaction : ActionNode
    {
        Animator m_animator;
        float m_startTime;
        public float m_duration;

        // When is hit activate the animation based off of the direction it was hit
        protected override void BeginNode()
        {
            base.BeginNode();
            m_startTime = Time.time;
            m_animator = m_input.m_go.GetComponentInChildren<Animator>();
            m_input.m_aIMovement.SetWarp(false);
            m_input.m_aIMovement.m_swordCollider.enabled = false;
            m_animator.SetFloat("HitX", m_input.m_aIMovement.m_hitDirection.x);
            m_animator.SetFloat("HitY", m_input.m_aIMovement.m_hitDirection.y);
            m_animator.SetFloat("ForwardsBackwards", m_input.m_aIMovement.m_hitDirection.x / 10);
            m_animator.SetFloat("Sideways", m_input.m_aIMovement.m_hitDirection.y / 10);
            m_animator.SetTrigger("Hit");
            m_input.m_aIMovement.m_isHit = false;
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
