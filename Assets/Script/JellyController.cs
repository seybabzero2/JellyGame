using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

public class JellyController : MonoBehaviour
{
    [Header("Jelly Squeeze Settings")]
    public float influenceRadius = 1.5f;
    public float pressForce = 0.5f;

    private NativeArray<Vector3> baseVertices;
    private NativeArray<Vector3> currentVertices;
    private NativeArray<Vector3> previousVertices;

    private NativeArray<int> triangles;
    private NativeArray<Vector3> normals; 

    private MeshFilter mf;
    private Mesh mesh;
    private Vector3[] vertices;
    public float ySeed = 2f;

    public float damping = 0.05f;
    public float stiffness = 40.0f;

    private void Start() {
        mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
        vertices = mesh.vertices;

        Debug.Log($"Кількість вершин: {mesh.vertexCount}");
        Debug.Log($"Перша вершина: {vertices[0]}");

        baseVertices = new NativeArray<Vector3>(mesh.vertexCount, Allocator.Persistent);
        baseVertices.CopyFrom(vertices);        
        currentVertices = new NativeArray<Vector3>(mesh.vertexCount, Allocator.Persistent);
        currentVertices.CopyFrom(vertices);       
        previousVertices = new NativeArray<Vector3>(mesh.vertexCount, Allocator.Persistent);

        int[] meshTriangles = mesh.triangles;
        triangles = new NativeArray<int>(meshTriangles.Length, Allocator.Persistent);
        triangles.CopyFrom(meshTriangles);
        
        normals = new NativeArray<Vector3>(mesh.vertexCount, Allocator.Persistent);
    }

    public void Update()
    {
        JellyPhysicsJob jobData;

        jobData.baseVertices = baseVertices;
        jobData.currentVertices = currentVertices;
        jobData.previousVertices = previousVertices;
        jobData.time = Time.time;
        jobData.ySeed = ySeed;
        jobData.damping = damping;
        jobData.stiffness = stiffness;
        jobData.deltaTime = Time.deltaTime;
        

        JobHandle handle = jobData.Schedule(mesh.vertexCount, 64);
        handle.Complete();

        Vector3 targetScale = new Vector3(1.0f, 1.0f, 1.0f);
        if (UnityEngine.InputSystem.Keyboard.current.upArrowKey.isPressed) 
        {
            targetScale = new Vector3(0.5f, 2.0f, 0.5f);
        }
        else if (UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed) 
        {
            targetScale = new Vector3(2.0f, 0.5f, 2.0f);
        }

        Matrix4x4 scaleMatrix = Matrix4x4.Scale(targetScale);

        for (int i = 0; i < vertices.Length; i++)
        {
            baseVertices[i] = scaleMatrix.MultiplyPoint3x4(vertices[i]);
        }

        JellyNormalsJob normalsJob;
        normalsJob.vertices = currentVertices;
        normalsJob.triangles = triangles;
        normalsJob.normals = normals;

        JobHandle normalsHandle = normalsJob.Schedule(handle);
        normalsHandle.Complete();

        mesh.SetVertices(currentVertices);
        mesh.RecalculateNormals();


    }
        
    public void OnDestroy() {
        if (baseVertices.IsCreated){
            baseVertices.Dispose(); 
            }           
        if (currentVertices.IsCreated){
            currentVertices.Dispose();            
        }
        if (previousVertices.IsCreated){
            previousVertices.Dispose();            
        }
        if (triangles.IsCreated) triangles.Dispose();
if (normals.IsCreated) normals.Dispose();
    }


}
[BurstCompile]
public struct JellyPhysicsJob : IJobParallelFor 
{
    [ReadOnly] public NativeArray<Vector3> baseVertices;
    public NativeArray<Vector3> previousVertices;
    public NativeArray<Vector3> currentVertices;

    public float time;
    public float ySeed;
    public float damping;
    public float stiffness;
    public float deltaTime;

    public void Execute(int i)
{
    Vector3 curr = currentVertices[i];
    Vector3 prev = previousVertices[i];
    Vector3 basePos = baseVertices[i];

    float2 voronoi = noise.cellular(new float3(
        basePos.x * 4f, 
        basePos.y * 4f - time * 2f, 
        basePos.z * 4f
    ));

    float strangeBump = voronoi.x * math.sin(time * 5f + basePos.y * 10f) * 0.1f;

    Vector3 targetPos = basePos;
    targetPos.x += basePos.x * strangeBump;
    targetPos.y += basePos.y * strangeBump;
    targetPos.z += basePos.z * strangeBump;
    Vector3 force = -stiffness * (curr - targetPos);
    
    Vector3 nextPos = curr + (curr - prev) * (1f - damping) + force * (deltaTime * deltaTime);
    
    previousVertices[i] = curr;
    currentVertices[i] = nextPos;
}
    
}

[BurstCompile]
public struct JellyNormalsJob : IJob {
    [ReadOnly] public NativeArray<Vector3> vertices;
    [ReadOnly] public NativeArray<int> triangles;
    public NativeArray<Vector3> normals;

    public void Execute() {
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.zero;
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int iA = triangles[i];
            int iB = triangles[i + 1];
            int iC = triangles[i + 2];
            Vector3 posA = vertices[iA];
            Vector3 posB = vertices[iB];
            Vector3 posC = vertices[iC];
            Vector3 sideAB = posB - posA;
            Vector3 sideAC = posC - posA;

            Vector3 triangleNormal = math.cross(sideAB, sideAC);

            normals[iA] += triangleNormal;
            normals[iB] += triangleNormal;
            normals[iC] += triangleNormal;
        }
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = math.normalize(normals[i]);
        }
    }
}
