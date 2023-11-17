using AISystem;
using System.Diagnostics;

public class EnemyActivator : Puzzle
{
    public Enemy m_enemy;
    public Puzzle m_puzzle;
    public TutorialTrigger m_tutorial;
    public bool m_isLastSwordInRoom;

    private new void Start()
    {
        base.Start();
        m_canInteract = false;
    }

    void Update()
    {
        if (m_updateMana && m_enemy != null && m_enemy.IsStatue())
        {
            GameManager.Instance.m_audioManager.PlayOneShot(m_manaFlowOn, transform.position);
            m_updateMana = false;
            m_enemy.SetStatue(false);            
        }     
        else if (m_updateMana && m_enemy == null)
        {
            m_updateMana = false;
            EnemyDead();
        }
    }

    public override void RotatePuzzle() { }

    public void EnemyDead()
    {
        m_unlocked = true;
        if (m_puzzle != null)
        {
            m_puzzle.m_updateMana = true;
        }
        foreach (Door door in m_doors)
        {
            door.CheckState();
        }
        
        if(m_tutorial != null)
        {
            m_tutorial.SecondTutorial();
        }
        if(m_isLastSwordInRoom)
        {
            GameManager.Instance.m_audioManager.PlaySwordRoomEndDialogue();
        }
        if(m_bridge != null)
        {
            m_bridge.SetBridgeState(true);
        }
    }
}
