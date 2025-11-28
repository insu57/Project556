using System;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float fov = 90f; //시야각 크기
    [SerializeField] private int rayCount = 50; //시야각의 ray개수
    [SerializeField] private float viewDistance = 50f; //시야거리
    [SerializeField] private GameObject arcObject;
    private Mesh _arcMesh; //시야각 Mesh
    [SerializeField] private GameObject circleObject;
    private Mesh _circleMesh;
    [SerializeField] private Transform eyesPos;
    [SerializeField] private LayerMask layerMask; //지면 등 시야를 가리는 오브젝트 레이어
    private PlayerControl _playerControl;
    [SerializeField] private float detectionRadius = 5f; //추후 데이터는 다른 클래스에서
    [SerializeField] private int circleVertexCount = 100;
    
    public void Init(PlayerControl playerControl)
    {
        _playerControl = playerControl;
    }
    
    private void Start()
    {
        _arcMesh = new Mesh();
        arcObject.GetComponent<MeshFilter>().mesh = _arcMesh;
        
        _circleMesh = new Mesh();
        circleObject.GetComponent<MeshFilter>().mesh = _circleMesh;
        
        FOVCircle(); //감지범위 변경 시?
    }

    private void Update()
    {
        FOVArc();
    }

    private void FOVArc() //호 모양(마우스 위치 기준, 사격 방향대로) -> 시야거리, 시야각 기준대로
    {
        Vector3 origin = Vector3.zero;// 원점(로컬기준)
  
        float angleOffset = _playerControl.IsFlipped ? -1 * fov / 2f : fov / 2f;
        float angle = _playerControl.ShootAngle + angleOffset; //사격 각도를 중심으로
        
        float angleIncrease = fov / rayCount; //Ray간의 각도 차이(증분)
        if (_playerControl.IsFlipped) angleIncrease *= -1;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1]; //origin 부터 ray의 끝점까지
        Vector2[] uv = new Vector2[vertices.Length]; //uv 좌표 배열
        int[] triangles = new int[rayCount * 3]; //

        vertices[0] = origin;
        int vertexIndex = 1;
        int triangleIndex = 0;

        Vector3 rayStartPos = transform.position;
        
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            Vector3 rayDirection = _playerControl.IsFlipped  //ray 방향벡터
                ? GetVectorFromAngle(180 - angle) : GetVectorFromAngle(angle);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(rayStartPos, rayDirection, //raycast
                viewDistance, layerMask);
            
            if (!raycastHit2D.collider)
            {
                //no hit
                if(_playerControl.IsFlipped) vertex = origin + GetVectorFromAngle(180 - angle) * viewDistance;
                else vertex = origin + GetVectorFromAngle(angle) * viewDistance; //시야 거리까지
            }
            else
            {
                //Hit Object
                vertex = transform.InverseTransformPoint(raycastHit2D.point);//로컬 좌표 기준으로 변환
            }
            
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3; //삼각형 인덱스
            }
            vertexIndex++; //정점 인덱스 증가
            angle -= angleIncrease; //각도를 감소시켜 부채꼴 생성
        }
        
        _arcMesh.vertices = vertices;
        _arcMesh.uv = uv;
        _arcMesh.triangles = triangles; //Mesh에 적용
    }

    private void FOVCircle() //인식범위 따라 원모양으로
    {
        Vector3 origin = Vector3.zero;
        Vector3[] vertices = new Vector3[circleVertexCount + 1]; //중심점 + 원주상의 점들
        int[] triangles = new int[circleVertexCount * 3]; //삼각형 배열
        
        vertices[0] = origin;//중심점

        float angleCurrent = 0f;
        float angleStep = 360f / circleVertexCount;

        for (int i = 1; i <= circleVertexCount; i++)
        {
            float angleRad = angleCurrent * Mathf.Deg2Rad;
            Vector3 vertex = new Vector3(Mathf.Cos(angleRad) * detectionRadius, Mathf.Sin(angleRad) * detectionRadius);
            vertices[i] = vertex;
            if (i < circleVertexCount)
            {
                triangles[(i - 1) * 3] = 0;
                triangles[(i - 1) * 3 + 1] = i + 1;
                triangles[(i - 1) * 3 + 2] = i;
            }
            else
            {
                triangles[(i - 1) * 3] = 0;
                triangles[(i - 1) * 3 + 1] = 1;
                triangles[(i - 1) * 3 + 2] = i; //첫번째 점과 연결하여 원을 닫음
            }
            
            angleCurrent += angleStep;
        }
        
        _circleMesh.Clear();
        _circleMesh.vertices = vertices;
        _circleMesh.triangles = triangles;
        _circleMesh.RecalculateNormals();
    }
    
    private static Vector3 GetVectorFromAngle(float angle) //각도를 단위벡터로 반환
    {
        //angle = 0 -> 360
        float angleRad = angle * Mathf.Deg2Rad; //라디안 각도
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad)); //삼각함수로 계산
    }
    
}
