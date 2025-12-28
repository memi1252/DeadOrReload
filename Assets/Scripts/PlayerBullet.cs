using UnityEngine;
using StarterAssets;

public class PlayerBullet : MonoBehaviour
{
    private MonoBehaviour shooter; // PlayerController 또는 ThirdPersonController
    private float speed;
    private float lifetime = 5f;
    
    public void Initialize(PlayerController shooter, float speed)
    {
        this.shooter = shooter;
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
    
    // ThirdPersonController용 초기화 오버로드
    public void Initialize(ThirdPersonController shooter, float speed)
    {
        this.shooter = shooter;
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
            CreateImpactEffect();
            
            // 벽 충돌 사운드
            if (SoundManager.Instance != null)
            {
                //SoundManager.Instance.PlaySoundAtPosition("BulletImpact", transform.position);
            }
            
            Destroy(gameObject);
            return;
        }
        
        // AI 에이전트에 맞으면
        AIController hitAgent = other.GetComponent<AIController>();
        if (hitAgent != null)
        {
            // 명중 처리
            hitAgent.OnHit();
            
            // 슈터 타입에 따라 적절한 메서드 호출
            if (shooter is PlayerController playerController)
            {
                playerController.OnEnemyHit();
            }
            else if (shooter is ThirdPersonController thirdPersonController)
            {
                thirdPersonController.OnEnemyHit();
            }
            
            CreateImpactEffect();
            
            // 적중 사운드
            if (SoundManager.Instance != null)
            {
                //SoundManager.Instance.PlaySoundAtPosition("BulletHit", transform.position);
            }
            
            if(ScoreUI.Instance!= null)
                ScoreUI.Instance.AddPlayerScore();
            Destroy(gameObject);
        }
    }
    
    private void CreateImpactEffect()
    {
        // 총알 충돌 이펙트 생성
        Debug.Log("Player Bullet Impact at: " + transform.position);
    }
}