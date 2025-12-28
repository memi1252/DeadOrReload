using UnityEngine;

public class VisualEffects : MonoBehaviour
{
    [Header("Neon Effects")]
    public Material playerNeonMaterial;  // 네온 블루
    public Material aiNeonMaterial;      // 네온 레드
    public Material bulletTrailMaterial;
    
    [Header("Particle Effects")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletImpactPrefab;
    public GameObject dashEffectPrefab;
    
    [Header("Screen Effects")]
    public GameObject digitalNoisePrefab;
    public float noiseIntensity = 0.1f;
    
    private ParticleSystem dashParticles;
    private LineRenderer bulletTrail;
    
    private void Start()
    {
        SetupVisualEffects();
    }
    
    private void SetupVisualEffects()
    {
        // 에이전트 타입에 따른 네온 머티리얼 적용
        DeadOrReloadAgent agent = GetComponent<DeadOrReloadAgent>();
        PlayerController player = GetComponent<PlayerController>();
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (agent != null)
            {
                // AI 에이전트는 빨간 네온
                renderer.material = aiNeonMaterial;
            }
            else if (player != null)
            {
                // 플레이어는 파란 네온
                renderer.material = playerNeonMaterial;
            }
        }
        
        // 대시 이펙트 설정
        if (dashEffectPrefab != null)
        {
            GameObject dashEffect = Instantiate(dashEffectPrefab, transform);
            dashParticles = dashEffect.GetComponent<ParticleSystem>();
        }
    }
    
    public void PlayMuzzleFlash(Vector3 position, Vector3 direction)
    {
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, position, Quaternion.LookRotation(direction));
            Destroy(flash, 0.5f);
        }
    }
    
    public void PlayBulletImpact(Vector3 position)
    {
        if (bulletImpactPrefab != null)
        {
            GameObject impact = Instantiate(bulletImpactPrefab, position, Quaternion.identity);
            Destroy(impact, 2f);
        }
    }
    
    public void PlayDashEffect()
    {
        if (dashParticles != null)
        {
            dashParticles.Play();
        }
    }
    
    public void CreateBulletTrail(Vector3 startPos, Vector3 endPos)
    {
        if (bulletTrail == null)
        {
            GameObject trailObj = new GameObject("BulletTrail");
            bulletTrail = trailObj.AddComponent<LineRenderer>();
            bulletTrail.material = bulletTrailMaterial;
            bulletTrail.startWidth = 0.05f;
            bulletTrail.endWidth = 0.01f;
            bulletTrail.positionCount = 2;
        }
        
        bulletTrail.SetPosition(0, startPos);
        bulletTrail.SetPosition(1, endPos);
        
        // 트레일 페이드 아웃
        StartCoroutine(FadeTrail());
    }
    
    private System.Collections.IEnumerator FadeTrail()
    {
        float fadeTime = 0.2f;
        float elapsed = 0f;
        Color startColor = bulletTrail.material.color;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            Color newColor = startColor;
            newColor.a = alpha;
            bulletTrail.material.color = newColor;
            yield return null;
        }
        
        if (bulletTrail != null)
        {
            Destroy(bulletTrail.gameObject);
        }
    }
}