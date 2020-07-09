using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class RayTracingMaster : MonoBehaviour
{

    public Light DirectionalLight;
    public ComputeShader RayTracingShader;
    
    public Texture SkyboxTexture;
    public Texture textureAlbedo;

    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100.0f;
    public int SphereSeed;
    
    private RenderTexture m_targetTexture;
    private RenderTexture m_convergedTexture;
    private Camera m_camera;

    private uint m_currentSample = 0;
    private Material m_addMaterial = null;


    private static bool m_fMeshObjectsNeedRebuilding = false;
    private static List<RayTracingObject> m_rtObjects = new List<RayTracingObject>();

    struct Sphere
    {
        public Vector3 ptPosition;
        public float radius;
        public Vector3 cAlbedo;
        public Vector3 cSpecular;
        public float smoothness;
        public Vector3 cEmission;
    }

    struct MeshObject
    {
        public Matrix4x4 mat4LocalToWorld;
        public int indices_offset;
        public int indices_count;
        
    }

    private static List<MeshObject> m_meshObjects = new List<MeshObject>();
    private static List<Vector3> m_Vertices = new List<Vector3>();
    private static List<Vector3> m_Normals = new List<Vector3>();
    private static List<Vector2> m_UVs = new List<Vector2>();
    private static List<int> m_Indices = new List<int>();

    private ComputeBuffer m_computerBufferMeshObject;
    private ComputeBuffer m_computeBufferVertexBuffer;
    private ComputeBuffer m_computeBufferNormalBuffer;
    private ComputeBuffer m_computeBufferUVBuffer;
    private ComputeBuffer m_computeBufferIndexBuffer;
    private ComputeBuffer m_computeBufferSpheres;

    public static int SizeOf<T>()
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
    }

    private void OnEnable()
    {
        m_currentSample = 0;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (m_computeBufferSpheres != null)
            m_computeBufferSpheres.Release();
    }

    public static void RegisterObject(RayTracingObject rtObj)
    {
        m_rtObjects.Add(rtObj);
        m_fMeshObjectsNeedRebuilding = true;
    }

    public static void UnregisterObject(RayTracingObject rtObj)
    {
        m_rtObjects.Remove(rtObj);
        m_fMeshObjectsNeedRebuilding = true;
    }

    private void RebuildMeshObjectBuffers()
    {
        if (m_fMeshObjectsNeedRebuilding == false)
            return;

        m_fMeshObjectsNeedRebuilding = false;
        m_currentSample = 0;

        // Clear all
        m_meshObjects.Clear();
        m_Vertices.Clear();
        m_Indices.Clear();

        foreach(RayTracingObject rtObj in m_rtObjects)
        {
            Mesh mesh = rtObj.GetComponent<MeshFilter>().sharedMesh;

            // Add vertex data to shared buffer
            int firstVertex = m_Vertices.Count;
            m_Vertices.AddRange(mesh.vertices);
            m_Normals.AddRange(mesh.normals);

            List<Vector2> objUVs = new List<Vector2>();
            mesh.GetUVs(0, objUVs);

            m_UVs.AddRange(objUVs);

            // Add index data to the shared buffer
            int firstIndex = m_Indices.Count;
            var indices = mesh.GetIndices(0);
            m_Indices.AddRange(indices.Select(index => index + firstVertex));

            // Create a new Mesh Object and add it to the list
            m_meshObjects.Add(new MeshObject() {
                mat4LocalToWorld = rtObj.transform.localToWorldMatrix,
                indices_offset = firstIndex,
                indices_count = indices.Length
            });;

            textureAlbedo = rtObj.GetComponent<Renderer>().material.mainTexture;
        }

        // Compute Buffers
        CreateComputeBuffer(ref m_computerBufferMeshObject, m_meshObjects, SizeOf<MeshObject>());
        CreateComputeBuffer(ref m_computeBufferVertexBuffer, m_Vertices, SizeOf<Vector3>());
        CreateComputeBuffer(ref m_computeBufferNormalBuffer, m_Normals, SizeOf<Vector3>());
        CreateComputeBuffer(ref m_computeBufferUVBuffer, m_UVs, SizeOf<Vector2>());
        CreateComputeBuffer(ref m_computeBufferIndexBuffer, m_Indices, SizeOf<int>());
    }

    private static void CreateComputeBuffer<T>(ref ComputeBuffer rBuffer, List<T> data, int stride) where T : 
        struct 
    {
        if(rBuffer != null) {
            if(data.Count == 0 || rBuffer.count != data.Count || rBuffer.stride != stride) {
                rBuffer.Release();
                rBuffer = null;
            }
        }

        if(data.Count != 0) {
            if(rBuffer == null)
                rBuffer = new ComputeBuffer(data.Count, stride);

            rBuffer.SetData(data);
        }
    }

    private void SetComputeBuffer(string strName, ComputeBuffer buffer) {
        if(buffer != null)
            RayTracingShader.SetBuffer(0, strName, buffer);
    }

    private void SetUpScene() {
        Random.InitState(SphereSeed);

        List<Sphere> spheres = new List<Sphere>();

        // Create a scene of random spheres
        for(int i = 0; i < SpheresMax; i++) {
            Sphere sphere = new Sphere();

            // Radius
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.ptPosition = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            // Reject spheres that are intersecting
            foreach(Sphere other in spheres) {
                float minDistance = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.ptPosition - other.ptPosition) < minDistance * minDistance)
                    goto SkipSphere; // WTFF 
            }

            // Colors
            // Albedo and specular color
            Color color = Random.ColorHSV();
            float chance = Random.value;
            if (chance < 0.8f) {
                bool metal = chance < 0.4f;
                sphere.cAlbedo = metal ? Vector4.zero : new Vector4(color.r, color.g, color.b);
                sphere.cSpecular = metal ? new Vector4(color.r, color.g, color.b) : new Vector4(0.04f, 0.04f, 0.04f);
                sphere.smoothness = Random.value;
            }
            else {
                Color cEmission = Random.ColorHSV(0, 1, 0, 1, 3.0f, 8.0f);
                sphere.cEmission = new Vector3(cEmission.r, cEmission.g, cEmission.b);
            }

            // Add the sphere
            spheres.Add(sphere);

        SkipSphere:
            continue;
        }

        // Assign to compute shader buffer
        m_computeBufferSpheres = new ComputeBuffer(spheres.Count, SizeOf<Sphere>());   // WTFF
        m_computeBufferSpheres.SetData(spheres);
    }

    private void Update() {
        if(transform.hasChanged) {
            m_currentSample = 0;
            transform.hasChanged = false;
        }

        if(DirectionalLight.transform.hasChanged) {
            DirectionalLight.transform.hasChanged = false;
        }
    }

    private void Awake() {
        m_camera = GetComponent<Camera>();
    }

    private void SetShaderParameters() {
        RayTracingShader.SetMatrix("mat4CameraToWorld", m_camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("mat4InverseCameraProjection", m_camera.projectionMatrix.inverse);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));

        // Lights
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        RayTracingShader.SetFloat("_Seed", Random.value);

        // Compute buffers
        SetComputeBuffer("_Spheres", m_computeBufferSpheres);
        SetComputeBuffer("_MeshObjects", m_computerBufferMeshObject);
        SetComputeBuffer("_Vertices", m_computeBufferVertexBuffer);
        SetComputeBuffer("_Normals", m_computeBufferNormalBuffer);
        SetComputeBuffer("_UVs", m_computeBufferUVBuffer);
        SetComputeBuffer("_Indices", m_computeBufferIndexBuffer);

        // Textures 
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);

        if (textureAlbedo != null)
        {
            RayTracingShader.SetTexture(0, "textureAlbedo", textureAlbedo);
        }
    }

    private void OnRenderImage(RenderTexture srcTexture, RenderTexture destTexture) {
        RebuildMeshObjectBuffers();

        SetShaderParameters();
        
        Render(destTexture);
    }

    private void Render(RenderTexture destTexture) {
        // Ensure current render target
        InitRenderTexture();

        // Set target and dispatch compute shader
        RayTracingShader.SetTexture(0, "Result", m_targetTexture);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the resulting texture to the screen
        if (m_addMaterial == null)
            m_addMaterial = new Material(Shader.Find("Hidden/AddShader"));

        m_addMaterial.SetFloat("_Sample", m_currentSample);

        Graphics.Blit(m_targetTexture, m_convergedTexture, m_addMaterial);
        Graphics.Blit(m_convergedTexture, destTexture);

        m_currentSample++;
    }

    private void InitRenderTexture()
    {
        if(m_targetTexture == null || m_targetTexture.width != Screen.width || m_targetTexture.height != Screen.height) {
            
            // release the texture if it already exists
            if(m_targetTexture != null)
                m_targetTexture.Release();

            // Get a new render target for the ray tracing compute shader
            m_targetTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_targetTexture.enableRandomWrite = true;
            m_targetTexture.Create();
        }

        if (m_convergedTexture == null || m_convergedTexture.width != Screen.width || m_convergedTexture.height != Screen.height) {
            
            // release the texture if it already exists
            if (m_convergedTexture != null)
                m_convergedTexture.Release();

            // Get a new render target for the ray tracing compute shader
            m_convergedTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_convergedTexture.enableRandomWrite = true;
            m_convergedTexture.Create();
        }
    }
}
