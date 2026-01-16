using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public HandSignReceiver receiver; // 拖入 HandSignReceiver
    public PlayerCombat combatState;  // 拖入 Player

    public Material laserMaterial;    // 拖入一个发光的材质
    public Material defaultMaterial;
    private Renderer rend;
    private float resetTimer = 0f;

    // 技能效果预制体 (后期填坑)
    // public GameObject dragonSkillPrefab;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if(defaultMaterial == null) defaultMaterial = rend.material;
    }

    void Update()
    {
        // 只有在领域展开且接收到新信号时才处理
        if (combatState.isDomainActive && receiver.hasNewInput)
        {
            ProcessHandSign(receiver.latestSignID, receiver.latestSignName);
            
            // 消费掉这个信号，防止连续触发
            receiver.hasNewInput = false; 
        }

        if (resetTimer >0 )
            {
                resetTimer = resetTimer - Time.unscaledDeltaTime;
                if (resetTimer <= 0)
                {
                    rend.material = defaultMaterial;
                }
            }
    }

    void ProcessHandSign(int id, string name)
    {
        // 这里对应 labels.csv
        // 6: Tatsu(Dragon) -> 辰
        if (id == 5 || name.Contains("Dragon") || name.Contains("辰")) 
        {
            CastDragonSkill();

        }

        if (id == 11 || name.Contains("Inazuma") || name.Contains("雷"))
        {
            Debug.Log(">>> 雷符咒发动！全场电击！ <<<");
            rend.material = laserMaterial;
            resetTimer = 0.2f; // 0.2秒后恢复
        }

        if (id == 13 || name.Contains("Gassho") || name.Contains("祈"))
        {
            Debug.Log("<<< 术式终止 >>>");
            combatState.DeactivateDomain();
        }
    }

    void CastDragonSkill()
    {
        rend.material = laserMaterial;
        resetTimer = 0.2f; // 0.2秒后恢复
        Debug.Log("离火龙咒·龙符咒发动！");
        
        // 1. 生成特效 (暂略)
        // Instantiate(dragonSkillPrefab, ...);

        // 2. 释放完大招后，通常直接结束领域，或者保持领域直到时间结束？
        // 策划案里写的是“顺发1~2个符咒”，这里演示释放后立即结束领域
        //combatState.DeactivateDomain();
    }
}