using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using AISystem.Data;
using AISystem.Contracts;
using AISystem.Pathing;

namespace AISystem.Systems
{
    public class AIMovement
    {
        const float m_arrivalThreshold = 1f;
        MovementSettings m_settings;
        IManager m_aiManager;
        IBeing m_attachedBeing;
        RootMotionSync m_rootMotionSync;
        Animator m_animator;

        public Path m_currentPath;

        bool m_movementEnabled;
        float m_speed = 0;

        public bool m_atDestination { get; private set; } = true;

        public bool m_isStatue = true;

        public bool m_isInterrupted = false;
        public bool m_isHit = false;
        public float m_stoneSpeed = 5f;
        public float m_armourDelay = 1f;
        public float m_armourSpeed = 5f;
        public Vector2 m_hitDirection;
        public CapsuleCollider m_swordCollider;
        public SkinnedMeshRenderer m_mesh;

        public AIMovement(MovementSettings settings, [CanBeNull] Animator animator, IBeing attachedBeing, IManager manager, 
            RootMotionSync rootMotionSync, CapsuleCollider swordCollider, SkinnedMeshRenderer mesh, float stoneSpeed, float armourDelay, float armourSpeed)
        {
            m_settings = settings;
            m_animator = animator;
            m_attachedBeing = attachedBeing;
            m_aiManager = manager;
            m_rootMotionSync = rootMotionSync;
            m_swordCollider = swordCollider;
            m_rootMotionSync.m_movement = this;
            m_mesh = mesh;
            m_stoneSpeed = stoneSpeed;
            m_armourDelay = armourDelay;    
            m_armourSpeed = armourSpeed;
        }

        public void EnableMovement()
        {
            m_movementEnabled = true;
            MovementLoop().Forget();
        }

        public void DisableMovement() => m_movementEnabled = false;

        public bool CanMove()
        {
            return m_movementEnabled;
        }

        public void Stop()
        {
            m_speed = 0;
            m_currentPath = Path.Empty;
            m_atDestination = true;
        }

        public void SetWarp(bool rootMotionOn) => m_rootMotionSync.SetWarp(rootMotionOn);

        public void SetDestination(Vector3 dest)
        {
            if (Vector3.SqrMagnitude(m_attachedBeing.m_position - dest) <= m_arrivalThreshold * m_arrivalThreshold)
            {
                m_atDestination = true;
                return;
            }

            Path path = m_aiManager.GeneratePath(new PathRequest()
            {
                m_destination = dest,
                m_destinationDirection = Vector3.Normalize(dest - m_attachedBeing.m_position),
                m_origin = m_attachedBeing.m_position,
                m_originDirection = m_attachedBeing.m_forward,
            });

            if (path.m_isEmpty)
            {
                return;
            }

            m_currentPath = path;
            m_atDestination = false;
        }

        async UniTask MovementLoop()
        {
            while (m_movementEnabled)
            {
                if (m_currentPath.m_isEmpty == false && m_animator != null)
                {
                    if (!m_atDestination)
                    {
                        UpdateForwardBackwards();
                        UpdateSideWays();
                    }
                    else
                    {
                        m_speed = 0;
                        m_animator.SetFloat("ForwardsBackwards", m_speed);
                        m_rootMotionSync.SetTurnWarp(0);
                        m_animator.SetFloat("Sideways", 0);
                    }

                    float sqrmag = Vector3.SqrMagnitude(m_attachedBeing.m_position - (Vector3)m_currentPath.m_destination);
                    m_atDestination = sqrmag <= m_arrivalThreshold;
                }

                await UniTask.Yield();
            }
        }

        void UpdateSideWays()
        {
            m_currentPath.GetRelativePoint(m_attachedBeing.m_position, m_settings.m_distance, out float3 predictPos, out float3 predictTan);
            m_rootMotionSync.SetYPos(predictPos.y);

            predictTan.y = 0;

            float angle = Vector3.SignedAngle(m_attachedBeing.m_forward, predictTan, Vector3.up);
            m_rootMotionSync.SetTurnWarp(angle);
            m_animator.SetFloat("Sideways", angle * Mathf.Deg2Rad);
        }

        void UpdateForwardBackwards()
        {
            float distToDest = Vector3.Distance(m_currentPath.m_destination, m_attachedBeing.m_position);

            if (distToDest >= m_arrivalThreshold * m_arrivalThreshold)
            {
                m_speed = Mathf.Lerp(m_speed, m_settings.m_run, Time.deltaTime * m_settings.m_acceleration);
                float speed = math.remap(0, m_settings.m_run, 0, 1, m_speed);
                m_animator.SetFloat("ForwardsBackwards", speed);
            }
            else
            {
                m_speed = 0;
                m_animator.SetFloat("ForwardsBackwards", m_speed);
            }
        }

        public async UniTask AwakenStatue(FMODUnity.EventReference stoneAwake, FMODUnity.EventReference armour, FMODUnity.EventReference grunt)
        {
            m_animator.enabled = true;
            float alpha = 0;
            GameManager.Instance.m_audioManager.PlayOneShot(grunt, m_mesh.gameObject.transform.position);
            GameManager.Instance.m_audioManager.PlayOneShot(stoneAwake, m_mesh.gameObject.transform.position);
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * m_stoneSpeed;
                m_mesh.materials[0].SetFloat("_Manual", alpha);                
                await UniTask.Yield();
            }
            float time = Time.time;
            while(Time.time <= time + m_armourDelay)
            {
                await UniTask.Yield();
            }
            alpha = 0;
            GameManager.Instance.m_audioManager.PlayOneShot(armour, m_mesh.gameObject.transform.position);
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * m_armourSpeed;                
                m_mesh.materials[1].SetFloat("_ArmorFade", alpha);
                await UniTask.Yield();
            }
            Enemy e = m_attachedBeing as Enemy;
            m_mesh.materials[0].SetFloat("_Manual", 1);
            m_mesh.materials[1].SetFloat("_ArmorFade", e.m_currentHP / e.m_startingHP);
        }

    }
}