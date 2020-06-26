using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class RayTracingMaster : MonoBehaviour
{

    public Light DirectionalLight;
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;
    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100.0f;
    public int SphereSeed;
    
    private RenderTexture m_targetTexture;
    private RenderTexture m_convergedTexture;
    private Camera m_camera;

    private uint m_currentSample = 0;
    private Material m_addMaterial = null;

    private ComputeBuffer _sphereBuffer;

    struct Sphere
    {
        public Vector3 ptPosition;
        public float radius;
        public Vector3 cAlbedo;
        public Vector3 cSpecular;
        public float smoothness;
        public Vector3 cEmission;
    }

    private void OnEnable()
    {
        m_currentSample = 0;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
    }

    private void SetUpScene()
    {
        Random.InitState(SphereSeed);

        List<Sphere> spheres = new List<Sphere>();

        // Create a scene of random spheres
        for(int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();

            // Radius
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.ptPosition = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            // Reject spheres that are intersecting
            foreach(Sphere other in spheres)
            {
                float minDistance = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.ptPosition - other.ptPosition) < minDistance * minDistance)
                    goto SkipSphere; // WTFF 
            }

            // Colors
            // Albedo and specular color
            Color color = Random.ColorHSV();
            float chance = Random.value;
            if (chance < 0.8f)
            {
                bool metal = chance < 0.4f;
                sphere.cAlbedo = metal ? Vector4.zero : new Vector4(color.r, color.g, color.b);
                sphere.cSpecular = metal ? new Vector4(color.r, color.g, color.b) : new Vector4(0.04f, 0.04f, 0.04f);
                sphere.smoothness = Random.value;
            }
            else
            {
                Color cEmission = Random.ColorHSV(0, 1, 0, 1, 3.0f, 8.0f);
                sphere.cEmission = new Vector3(cEmission.r, cEmission.g, cEmission.b);
            }

            // Add the sphere
            spheres.Add(sphere);

        SkipSphere:
            continue;
        }

        // Assign to compute shader buffer
        _sphereBuffer = new ComputeBuffer(spheres.Count, 56);   // WTFF
        _sphereBuffer.SetData(spheres);
    }

    private void Update()
    {
        if(transform.hasChanged)
        {
            m_currentSample = 0;
            transform.hasChanged = false;
        }

        if(DirectionalLight.transform.hasChanged)
        {
            DirectionalLight.transform.hasChanged = false;
        }
    }

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetMatrix("mat4CameraToWorld", m_camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("mat4InverseCameraProjection", m_camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));

        // Lights
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        // Spheres
        RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);

        RayTracingShader.SetFloat("_Seed", Random.value);
    }

    private void OnRenderImage(RenderTexture srcTexture, RenderTexture destTexture)
    {
        SetShaderParameters();
        
        Render(destTexture);
    }

    private void Render(RenderTexture destTexture)
    {
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
        if(m_targetTexture == null || m_targetTexture.width != Screen.width || m_targetTexture.height != Screen.height)
        {
            // release the texture if it already exists
            if(m_targetTexture != null)
            {
                m_targetTexture.Release();
            }

            // Get a new render target for the ray tracing compute shader
            m_targetTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_targetTexture.enableRandomWrite = true;
            m_targetTexture.Create();
        }

        if (m_convergedTexture == null || m_convergedTexture.width != Screen.width || m_convergedTexture.height != Screen.height)
        {
            // release the texture if it already exists
            if (m_convergedTexture != null)
            {
                m_convergedTexture.Release();
            }

            // Get a new render target for the ray tracing compute shader
            m_convergedTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_convergedTexture.enableRandomWrite = true;
            m_convergedTexture.Create();
        }
    }
}
