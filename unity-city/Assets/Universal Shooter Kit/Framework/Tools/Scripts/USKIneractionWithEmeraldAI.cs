using UnityEngine;

// This script is needed for interaction with Emerald AI system.

namespace GercStudio.USK.Scripts
{
    public class USKIneractionWithEmeraldAI : MonoBehaviour
    {

        [HideInInspector] public float damageAmount;

        public enum ColliderType
        {
            Null,
            Enemy,
            Smoke,
            Melee,
            Fire
        }

        public ColliderType colliderType;
         public Controller controller;

        [HideInInspector] public float grenadeEffectDeactivationTimeout;
        [HideInInspector] public float grenadeEffectDeactivationTimer;

#if USK_EMERALDAI_INTEGRATION
       public EmeraldAI.EmeraldAISystem.RelationType relationType;

        private void Update()
        {
            if (colliderType == ColliderType.Enemy)
            {
                grenadeEffectDeactivationTimer += Time.deltaTime;

                if (grenadeEffectDeactivationTimer >= grenadeEffectDeactivationTimeout)
                {
                    if (gameObject.GetComponent<EmeraldAI.EmeraldAIEventsManager>())
                    {
                        var em = gameObject.GetComponent<EmeraldAI.EmeraldAIEventsManager>();
                        em.SetPlayerRelation(ConvertRelationTypes(relationType));
                        em.ResumeMovement();
                    }

                    Destroy(this);
                }
            }
        }

        EmeraldAI.EmeraldAISystem.PlayerFactionClass.RelationType ConvertRelationTypes(EmeraldAI.EmeraldAISystem.RelationType relationType)
        {
            switch (relationType)
            {
                case EmeraldAI.EmeraldAISystem.RelationType.Enemy:
                    return EmeraldAI.EmeraldAISystem.PlayerFactionClass.RelationType.Enemy;
                case EmeraldAI.EmeraldAISystem.RelationType.Neutral:
                    return EmeraldAI.EmeraldAISystem.PlayerFactionClass.RelationType.Neutral;
                case EmeraldAI.EmeraldAISystem.RelationType.Friendly:
                    return EmeraldAI.EmeraldAISystem.PlayerFactionClass.RelationType.Friendly;
            }

            return EmeraldAI.EmeraldAISystem.PlayerFactionClass.RelationType.Friendly;
        }

        
        void OnTriggerEnter(Collider other)
        {
            if (colliderType == ColliderType.Smoke)
            {
                if (other.gameObject.GetComponent<EmeraldAI.EmeraldAIEventsManager>())
                {
                    var em = other.gameObject.GetComponent<EmeraldAI.EmeraldAIEventsManager>();
                    
                    var interactionScript = !other.gameObject.GetComponent<USKIneractionWithEmeraldAI>() ? other.gameObject.AddComponent<USKIneractionWithEmeraldAI>() : other.gameObject.GetComponent<USKIneractionWithEmeraldAI>();

                    if (interactionScript.colliderType == ColliderType.Null)
                    {
                        if (em.GetPlayerRelation() != EmeraldAI.EmeraldAISystem.RelationType.Friendly)
                        {
                            interactionScript.relationType = em.GetPlayerRelation();
                            em.SetPlayerRelation(EmeraldAI.EmeraldAISystem.PlayerFactionClass.RelationType.Friendly);
                            em.ClearTarget();
                            em.StopMovement();
                            interactionScript.colliderType = ColliderType.Enemy;
                            interactionScript.grenadeEffectDeactivationTimeout = 0.1f;
                        }
                    }
                    else
                    {
                        interactionScript.grenadeEffectDeactivationTimer = 0;
                    }
                }
            }
            else if (colliderType == ColliderType.Melee)
            {
                if (other.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>())
                {
                    AIHelper.DamageEmeraldAI((int) damageAmount, other.gameObject, controller.transform);
                    // enemy.Damage(, EmeraldAI.EmeraldAISystem.TargetType.Player, );
                    // EmeraldAI.CombatTextSystem.Instance.CreateCombatText((int) damageAmount, enemy.HitPointTransform.position, false, false, false);
                }
            }
        }


        void OnTriggerStay(Collider other)
        {
            if (colliderType == ColliderType.Smoke)
            {
                if (other.gameObject.GetComponent<USKIneractionWithEmeraldAI>())
                {
                    var script = other.gameObject.GetComponent<USKIneractionWithEmeraldAI>().grenadeEffectDeactivationTimer = 0;
                }

            }
            else if (colliderType == ColliderType.Fire)
            {
                if (other.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>())
                {
                    damageAmount *= Time.deltaTime;
                    
                    if (damageAmount < 1)
                        damageAmount = 1;
                    
                    AIHelper.DamageEmeraldAI((int) damageAmount, other.gameObject, controller.transform);

                    // var enemy = other.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>();
                    // enemy.Damage((int) damageAmount, EmeraldAI.EmeraldAISystem.TargetType.Player, controller.transform);
                    // EmeraldAI.CombatTextSystem.Instance.CreateCombatText((int) damageAmount, enemy.HitPointTransform.position, false, false, false);
                }
            }
        }
#endif
    }
}
