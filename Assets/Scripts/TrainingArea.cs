using UnityEngine;
using Unity.MLAgents;

public class TrainingArea : MonoBehaviour
{
    [Header("Training Settings")]
    public DeadOrReloadAgent[] agents;
    public Transform[] spawnPoints;
    public GameObject[] wallPrefabs;
    public float areaSize = 20f;
    
    [Header("Environment")]
    public GameObject centralObstacle;
    
    private void Start()
    {
        // 훈련 환경 초기화
        SetupTrainingEnvironment();
    }
    
    private void SetupTrainingEnvironment()
    {
        // 중앙 장애물 배치 (ㅁ자형)
        if (centralObstacle != null)
        {
            centralObstacle.transform.position = transform.position;
        }
        
        // 경계 벽 생성
        CreateBoundaryWalls();
        
        // 에이전트 초기 배치
        if (agents != null && agents.Length >= 2 && spawnPoints != null && spawnPoints.Length >= 2)
        {
            for (int i = 0; i < agents.Length && i < spawnPoints.Length; i++)
            {
                agents[i].transform.position = spawnPoints[i].position;
                agents[i].transform.rotation = spawnPoints[i].rotation;
            }
        }
    }
    
    private void CreateBoundaryWalls()
    {
        if (wallPrefabs == null || wallPrefabs.Length == 0) return;
        
        GameObject wallPrefab = wallPrefabs[0];
        float halfSize = areaSize * 0.5f;
        
        // 4개의 경계 벽 생성
        Vector3[] wallPositions = {
            new Vector3(0, 0, halfSize),      // 북쪽
            new Vector3(0, 0, -halfSize),     // 남쪽
            new Vector3(halfSize, 0, 0),      // 동쪽
            new Vector3(-halfSize, 0, 0)      // 서쪽
        };
        
        Vector3[] wallRotations = {
            Vector3.zero,                     // 북쪽
            Vector3.zero,                     // 남쪽
            new Vector3(0, 90, 0),           // 동쪽
            new Vector3(0, 90, 0)            // 서쪽
        };
        
        for (int i = 0; i < wallPositions.Length; i++)
        {
            GameObject wall = Instantiate(wallPrefab, transform.position + wallPositions[i], 
                                        Quaternion.Euler(wallRotations[i]), transform);
            wall.name = $"BoundaryWall_{i}";
        }
    }
    
    public void ResetArea()
    {
        // 에이전트들을 새로운 위치로 이동
        if (agents != null && spawnPoints != null)
        {
            for (int i = 0; i < agents.Length && i < spawnPoints.Length; i++)
            {
                if (agents[i] != null && spawnPoints[i] != null)
                {
                    agents[i].transform.position = spawnPoints[i].position + 
                        new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                    agents[i].transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                }
            }
        }
    }
    
    public Vector3 GetRandomPositionInArea()
    {
        float x = Random.Range(-areaSize * 0.4f, areaSize * 0.4f);
        float z = Random.Range(-areaSize * 0.4f, areaSize * 0.4f);
        return transform.position + new Vector3(x, 0, z);
    }
    
    public bool IsPositionValid(Vector3 position)
    {
        // 위치가 훈련 영역 내부에 있는지 확인
        Vector3 localPos = position - transform.position;
        return Mathf.Abs(localPos.x) < areaSize * 0.5f && Mathf.Abs(localPos.z) < areaSize * 0.5f;
    }
    
    private void OnDrawGizmosSelected()
    {
        // 훈련 영역 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize, 2f, areaSize));
        
        // 스폰 포인트 시각화
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 2f);
                }
            }
        }
    }
}