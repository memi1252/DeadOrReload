using StarterAssets;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private DeadOrReloadAgent shooter;
    private MonoBehaviour shooterEntity; // 실제 총을 쏜 엔티티 (ThirdPersonController 또는 기타)
    private float speed;
    private float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(DeadOrReloadAgent shooter, float speed)
    {
        this.shooter = shooter;
        this.shooterEntity = shooter; // DeadOrReloadAgent는 MonoBehaviour를 상속함
        this.speed = speed;
        
        // 총알에 물리 적용
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        
        // 일정 시간 후 자동 삭제
        Destroy(gameObject, lifetime);
    }
    
    // ThirdPersonController 등 다른 엔티티를 위한 초기화
    public void Initialize(MonoBehaviour shooterEntity, float speed)
    {
        this.shooter = null;
        this.shooterEntity = shooterEntity;
        this.speed = speed;
        
        // 총알에 물리 적용
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        
        // 일정 시간 후 자동 삭제
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 벽에 맞으면 총알 삭제
        if (other.CompareTag("Wall"))
        {
            // 벽 충돌 사운드
            if (SoundManager.Instance != null)
            {
                //SoundManager.Instance.PlaySoundAtPosition("BulletImpact", transform.position);
            }
            Destroy(gameObject);
            return;
        }
        
        // 자기 자신인지 확인 (자신의 총알에 맞지 않도록)
        if (shooterEntity != null && other.gameObject == shooterEntity.gameObject)
        {
            return; // 자기 자신은 무시
        }
        
        // 플레이어에 맞으면 (ThirdPersonController)
        StarterAssets.ThirdPersonController hitPlayer = other.GetComponent<StarterAssets.ThirdPersonController>();
        if (hitPlayer != null)
        {
            // 자신이 쏜 총알인지 다시 한번 확인
            if (shooterEntity == hitPlayer)
            {
                return; // 자기 자신의 총알이면 무시
            }
            
            hitPlayer.OnHit();
            if (shooter != null)
            {
                shooter.OnEnemyHit();
            }
            
            // 적중 사운드
            if (SoundManager.Instance != null)
            {
                //SoundManager.Instance.PlaySoundAtPosition("BulletHit", transform.position);
            }
            
            if(ScoreUI.Instance!= null)
                ScoreUI.Instance.AddAIScore();
            Destroy(gameObject);
            return;
        }
        
        // 에이전트에 맞으면
        AIController hitAgent = other.GetComponent<AIController>();
        if (hitAgent != null && hitAgent != shooter)
        {
            // 자신이 쏜 총알인지 확인
            if (shooterEntity == hitAgent)
            {
                return; // 자기 자신의 총알이면 무시
            }
            
            // 명중 처리
            hitAgent.OnHit();
            if (shooter != null)
            {
                shooter.OnEnemyHit();
            }
            
            // 적중 사운드
            if (SoundManager.Instance != null)
            {
                //SoundManager.Instance.PlaySoundAtPosition("BulletHit", transform.position);
            }
            
            Destroy(gameObject);
        }
    }
    
    private void CreateImpactEffect()
    {
        // 총알 충돌 이펙트 생성 (나중에 파티클 시스템 추가 가능)
        // 현재는 간단한 로그만
        Debug.Log("Bullet Impact at: " + transform.position);
    }
    
    private void OnTriggerExit(Collider other)
    {
        // 총알이 에이전트 근처를 지나갈 때 회피 보상
        DeadOrReloadAgent agent = other.GetComponent<DeadOrReloadAgent>();
        if (agent != null && agent != shooter)
        {
            // 자신의 총알에 대해서는 회피 보상을 주지 않음
            if (shooterEntity == agent)
            {
                return;
            }
            
            float distance = Vector3.Distance(transform.position, agent.transform.position);
            if (distance < 2f) // 2미터 이내로 지나가면 회피로 간주
            {
                agent.OnBulletDodged();
            }
        }
    }
}