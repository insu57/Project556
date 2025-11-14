using Player;
using UnityEngine;
using UnityEngine.UIElements;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float fov = 90f; //시야각 크기
    [SerializeField] private int rayCount = 50; //시야각의 ray개수
    [SerializeField] private float viewDistance = 50f; //시야거리
    private Mesh _mesh; //시야각 Mesh
    [SerializeField] private Transform eyesPos;
    [SerializeField] private LayerMask layerMask; //지면 등 시야를 가리는 오브젝트 레이어
    private PlayerControl _playerControl;

    public void Init(PlayerControl playerControl)
    {
        _playerControl = playerControl;
    }
    
    private void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    private void Update()
    {
        Vector3 origin = Vector3.zero;// 원점(로컬기준)
  
        float angleOffset = _playerControl.IsFlipped ? -1 * fov / 2f : fov / 2f;
        float angle = _playerControl.ShootAngle + angleOffset; //사격 각도를 중심으로
        
        
        Debug.Log("Angle: " + angle);
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
        
        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles; //Mesh에 적용
    }
    
    private static Vector3 GetVectorFromAngle(float angle) //각도를 단위벡터로 반환
    {
        //angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f); //라디안 각도
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad)); //삼각함수로 계산
    }
    
}
