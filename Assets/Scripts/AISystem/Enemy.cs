using System.Collections.Generic;
using UnityEngine;
using AISystem.BehaviourTrees;
using AISystem.Data;
using AISystem.Sensors;
using AISystem.Systems;
using AISystem.Contracts;
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEditor;
using UnityEngine.Splines;

namespace AISystem
{
    public class Enemy : MonoBehaviour, IBeing
    {
        #region AI Fields
        public Vector3 m_position => transform.position;
        public Vector3 m_forward => transform.forward;
        public Vector3 m_headPosition => m_head.position;

        [field: SerializeField] public EnemySettings AISettings { get; private set; } = new();
        [SerializeField] Animator m_animator;
        [SerializeField] RootMotionSync m_rootMotionSync;
        [SerializeField] Transform m_head;

        Intelligience m_intelligience;
        AIManager m_aiManager;
        #endregion

        #region  Game Fields
        public float m_startingHP;
        public float m_currentHP;
        public EnemyActivator m_activator;
        public float m_damage;
        public CapsuleCollider m_swordCollider;
        PlayerController m_playerController;
        int m_playerMask;                
        Quaternion m_startRotation;

        SkinnedMeshRenderer m_mesh;

        public FMODUnity.EventReference m_stoneAwake;
        public FMODUnity.EventReference m_armourAwake;
        public List<FMODUnity.EventReference> m_gruntsAwake;
        public FMODUnity.EventReference m_takeDamageSound;
        public FMODUnity.EventReference m_deathSound;
        public FMODUnity.EventReference m_playerHitEnemySound;

        public bool m_showDebug;
        public float m_maxDebugDistance;

        public ParticleSystem m_activationParticle;
        public ParticleSystem m_isDamagedParticle;
        public ParticleSystem m_deathParticle;
        public float m_deathTime = 3f;
        public ParticleSystem m_swordTrailParticle;

        public float m_stoneSpeed = 5f;
        public float m_armourDelay = 1;
        public float m_armourSpeed = 5f;
        #endregion

        void Start()
        {
            m_aiManager = GameManager.Instance.m_aiManager;
            m_aiManager.RegisterBeing(this);

            m_animator ??= GetComponentInChildren<Animator>();
            m_mesh = GetComponentInChildren<SkinnedMeshRenderer>();
            m_mesh.materials[0].SetFloat("_EmissiveFreq", 0);
            m_mesh.materials[1].SetFloat("_ArmorFade", 0);

            List<IOptic> optics = CreateOptics();
            AIKnowledge aIKnowledge = new AIKnowledge();
            AIMovement aIMovement = new AIMovement(AISettings.MovementSettings, m_animator, this, m_aiManager, m_rootMotionSync, m_swordCollider, m_mesh, m_stoneSpeed, m_armourDelay, m_armourSpeed);
            BehaviourManager behaviourManager = new BehaviourManager(UnpackBehaviourTree(AISettings.BehaviourTree, new BehaviourInput()
            {
                m_aIKnowledge = aIKnowledge,
                m_aIMovement = aIMovement,
                m_go = gameObject,
            }));

            m_intelligience = new Intelligience(optics, aIKnowledge, aIMovement, behaviourManager);

            m_currentHP = m_startingHP;

            m_playerController = FindObjectOfType<PlayerController>();
            m_playerMask = (int)Mathf.Log(LayerMask.GetMask("Sword"), 2);
           
            m_startRotation = transform.rotation;

            m_animator.enabled = false;

            int grunt = UnityEngine.Random.Range(0, m_gruntsAwake.Count - 1);

            m_intelligience.SetStatue(true, m_stoneAwake, m_armourAwake, m_gruntsAwake[grunt]);
            m_swordCollider.enabled = false;
        }

        void OnDestroy()
        {
            m_intelligience?.DisableIntelligience();
            m_aiManager?.DeregisterBeing(this);            
        }

        List<IOptic> CreateOptics()
        {
            List<IOptic> createdOptics = new List<IOptic>();
            createdOptics.Add(new OpticSensor(AISettings.ObservationSettings, m_aiManager, this));

            return createdOptics;
        }

        BehaviourTree UnpackBehaviourTree(BTAsset asset, BehaviourInput input)
        {
            BTAsset BTInstance = ScriptableObject.Instantiate(asset);
            var bt = BTInstance.m_behaviourTree;
            bt.SetBehaviourInput(input);
            return bt;
        }

        public void ResetToPosition()
        {
            m_intelligience.SetStatue(true, m_stoneAwake, m_armourAwake, m_gruntsAwake[0]);
            m_swordCollider.enabled = false;
            m_animator.rootPosition = m_activator.gameObject.transform.position;                       
            m_animator.enabled = false;
            m_animator.SetFloat("ForwardsBackwards", 0);
            m_animator.SetFloat("Sideways", 0);
            m_animator.Rebind();
            transform.SetPositionAndRotation(m_activator.gameObject.transform.position, m_startRotation);
            m_mesh.materials[0].SetFloat("_Manual", 0);
            m_mesh.materials[1].SetFloat("_ArmorFade", 0);
        }

        public bool IsStatue()
        {
            return m_intelligience.IsStatue();
        }

        public void SetStatue(bool state)
        {
            int grunt = UnityEngine.Random.Range(0, m_gruntsAwake.Count - 1);
            m_intelligience.SetStatue(state, m_stoneAwake, m_armourAwake, m_gruntsAwake[grunt]);
            if(state)
            {
                GameManager.Instance.DeactivateEnemy(this);
            }
            else
            {
                GameManager.Instance.ActivateEnemy(this);
            }
            if (m_activationParticle != null)
            { 
                m_activationParticle.Play(); 
            }
        }

        public void Interrupt()
        {
            m_intelligience.Interrupt();
        }

        public bool TakeHit(float damage, Vector2 angle)
        {
            m_playerController.HitReticle();
            m_swordCollider.enabled = false;
            m_currentHP -= damage;
            m_mesh.materials[1].SetFloat("_ArmorFade", m_currentHP / m_startingHP);
            GameManager.Instance.m_audioManager.PlayOneShot(m_takeDamageSound, gameObject.transform.position);
            bool isDead = m_currentHP <= 0;
            if (isDead)
            {
                Die().Forget();
            }
            else
            {
                m_intelligience.IsHit(angle);
            }
            return isDead;
        }

        protected async UniTask Die()
        {            
            m_activator.EnemyDead();
            GameManager.Instance.DeactivateEnemy(this);
            m_swordCollider.enabled = false;
            if (m_deathParticle != null)
            {                
                m_deathParticle.Play();
            }
            m_animator.SetTrigger("Die");
            m_intelligience.SetStatue(true, m_stoneAwake, m_armourAwake, m_gruntsAwake[0]);
            m_rootMotionSync.SetDead();
            gameObject.GetComponent<Collider>().enabled = false;
            GameManager.Instance.m_audioManager.PlayOneShot(m_deathSound, gameObject.transform.position);
            float time = Time.time;
            while (Time.time < time + m_deathTime)
            {
                await UniTask.Yield();
            }
            Destroy(gameObject);
        }

        protected void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject.layer == m_playerMask && !m_intelligience.IsStatue())
            {
                GameManager.Instance.m_audioManager.PlayOneShot(m_playerHitEnemySound, collision.gameObject.transform.position);
                Vector3 direction = -collision.GetContact(0).normal;
                Vector2 angle = new Vector2(direction.x, direction.z);
                TakeHit(m_playerController.m_meleeDamage, angle);
                if (m_isDamagedParticle != null)
                {
                    m_deathParticle.gameObject.transform.SetPositionAndRotation(collision.GetContact(0).point, Quaternion.Euler(collision.GetContact(0).normal));                 
                    m_isDamagedParticle.Play();
                }
                m_playerController.Rumble(m_playerController.m_hitVibration).Forget();
            }
        }

        /* public void OnDrawGizmos()
         {            
             if(!Application.isPlaying)
             {
                 return; 
             }

             if (m_showDebug)
             {
                 SceneView sceneView = SceneView.lastActiveSceneView;
                 if(sceneView != null)
                 {
                     float distanceToCamera = Vector3.Distance(transform.position, sceneView.camera.transform.position);
                     if(distanceToCamera <= m_maxDebugDistance)
                     {
                         GUIStyle style_AIInfo = new GUIStyle();
                         style_AIInfo.normal.textColor = Color.white;
                         string AIInfo = "Is Statue: " + m_intelligience.IsStatue() + "\nIs At Destination: " + m_intelligience.IsAtDestination() +
                             "\nMovement Enabled: " + m_intelligience.CanMove() + "\nCan See Player: " + m_intelligience.CanSeePlayer();
                         Handles.Label(transform.position + Vector3.up * 3f, AIInfo, style_AIInfo);        
                     }
                 }
             }

             if(!m_intelligience.IsAtDestination())
             {                
                 BezierKnot[] positions =  m_intelligience.GetPath().ToArray();
                 for(int i = 0; i < positions.Length; i++)
                 {
                     if(i == 0)
                     {
                         Debug.DrawLine(transform.position, positions[0].Position, Color.red);
                     }
                     else
                     {
                         Debug.DrawLine(positions[i-1].Position, positions[i].Position, Color.red);
                     }
                 }
                 Debug.DrawLine(transform.position, m_animator.deltaPosition + transform.position, Color.cyan);
                 Debug.DrawLine(transform.position, (transform.rotation * Vector3.forward) + transform.position, Color.magenta);
             }

         }*/
    }

}