using System;
using Tango;
using UnityEngine;

public class EnvironmentalLighting : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle
{
    public bool m_enableDebugUI = false;
    
    public bool m_enableEnvironmentalLighting = true;
    
    // Constants used to compute the spherical harmonic diffuse lighting
    private const float C1 = 0.492043f;
    private const float C2 = 0.511664f;
    private const float C3 = 0.743125f;
    private const float C4 = 0.886227f;
    private const float C5 = 0.247708f;
    
    private const int EMULATED_CAMERA_WIDTH = 1280;
    private const int EMULATED_CAMERA_HEIGHT = 720;

    private const int SQRT_N_SAMPLES = 50;
    
    // The number of spherical harmonic bands used to approximate the diffuse lighting.
    private const int LEVELS = 3;
    
    // The texture used for the specular lighting.
    private Texture m_environmentMap;

    // The diffuse lighting samples with (theta, phi) coordinates and weighted coefficients for sampling.
    private SphericalHarmonicSample[] m_samples;

    // The diffuse spherical harmonic coefficients for each band and order.
    private Vector3[] m_coefficients;

    // Displays the button for toggling environmental lighting if debug is enabled.
    public void OnGUI()
    {
        if (m_enableDebugUI)
        {
            if (GUI.Button(new Rect(10, 10, 600, 100),
                    "<size=40>Toggle Environmental Lighting</size>"))
            {
                m_enableEnvironmentalLighting = !m_enableEnvironmentalLighting;
            }
        }
    }
    
    // Awake for TangoEnvironmentalLighting. Compute the coefficients, polar coordinates, and Cartesian coordinates to be sampled.
    public void Awake()
    {
        m_samples = new SphericalHarmonicSample[SQRT_N_SAMPLES * SQRT_N_SAMPLES];
        int numCoefficients = LEVELS * LEVELS;
        m_coefficients = new Vector3[numCoefficients];
        for (int n = 0; n < numCoefficients; ++n)
        {
            m_coefficients[n] = Vector3.zero;
        }

        int i = 0;
        float oneOverN = 1.0f / SQRT_N_SAMPLES;
        for (int a = 0; a < SQRT_N_SAMPLES; ++a)
        {
            for (int b = 0; b < SQRT_N_SAMPLES; ++b)
            {
                SphericalHarmonicSample sample;
                float x = a * oneOverN;
                float y = b * oneOverN;
                float theta = 2.0f * Mathf.Cos(Mathf.Sqrt(1.0f - x));
                float phi = 2.0f * Mathf.PI * y;
                sample.sph = new Vector2(theta, phi);

                // Convert spherical coords to unit vector.
                Vector3 vec = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi),
                                          Mathf.Sin(theta) * Mathf.Sin(phi),
                                          Mathf.Cos(theta));
                sample.vec = vec;

                // Precompute all SH coefficients for this sample.
                sample.coeff = new float[numCoefficients];
                for (int l = 0; l < LEVELS; ++l)
                {
                    for (int m = -l; m <= l; ++m)
                    {
                        int index = (l * (l + 1)) + m;
                        sample.coeff[index] = SH(l, m, theta, phi);
                    }
                }

                m_samples[i] = sample;
                ++i;
            }
        }
    }

    // Initialize the Environmental Lighting controller.
    public void Start()
    {
        TangoApplication tangoApplication = FindObjectOfType<TangoApplication>();
        if (tangoApplication != null)
        {
            tangoApplication.Register(this);
        }
    }
    
    public void Update()
    {
        if (m_enableEnvironmentalLighting && m_environmentMap != null)
        {
            API.TangoUnity_updateEnvironmentMap(m_environmentMap.GetNativeTexturePtr().ToInt32(),
                                                m_environmentMap.width,
                                                m_environmentMap.height);

            // Rendering the latest frame changes a bunch of OpenGL state.  Ensure Unity knows the current OpenGL state.
            GL.InvalidateState();

            Shader.SetGlobalTexture("_TangoLightingEnvironmentMap", m_environmentMap);
        }
        else
        {
            Shader.SetGlobalFloat("_TangoLightingExposure", 0);
        }
    }
    
    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        _ComputeDiffuseCoefficients(imageBuffer);
    }

    /// This is called when the permission-granting process is finished.
    public void OnTangoPermissions(bool permissionsGranted)
    {
    }
    
    // This is called when successfully connected to the Tango Service.
    public void OnTangoServiceConnected()
    {
        TangoCameraIntrinsics intrinsics = new TangoCameraIntrinsics();
        VideoOverlayProvider.GetIntrinsics(TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR, intrinsics);
        m_environmentMap = new Texture2D((int)intrinsics.width, (int)intrinsics.height, TextureFormat.RGBA32, false);
    }
    
    // This is called when disconnected from the Tango service.
    public void OnTangoServiceDisconnected()
    {
    }
    
    // Computes the spherical harmonic diffuse coefficients for a given TangoImageBuffer.
    private void _ComputeDiffuseCoefficients(TangoUnityImageData imageBuffer)
    {
        if (m_enableEnvironmentalLighting)
        {
            // Compute SH Coefficients.
            float weight = 4.0f * Mathf.PI;
            int numSamples = m_samples.Length;
            int numCoefficients = m_coefficients.Length;
            for (int coeffIdx = 0; coeffIdx < numCoefficients; ++coeffIdx)
            {
                m_coefficients[coeffIdx] = Vector3.zero;
            }

            for (int sampleIdx = 0; sampleIdx < numSamples; ++sampleIdx)
            {
                float theta = m_samples[sampleIdx].sph.x;
                float phi = m_samples[sampleIdx].sph.y;

                // Normalize between 0 and 1.
                float x = 1.0f - Mathf.Pow(Mathf.Cos(theta / 2.0f), 2.0f);
                float y = phi / Mathf.PI / 2.0f;

                int i = (int)(imageBuffer.height * x);
                int j = (int)(imageBuffer.width * y);

                Vector3 rgb = _GetRgbFromImageBuffer(imageBuffer, i, j);
                for (int coeffIdx = 0; coeffIdx < numCoefficients; ++coeffIdx)
                {
                    m_coefficients[coeffIdx] += rgb * m_samples[sampleIdx].coeff[coeffIdx];
                }
            }

            // Divide the result by weight and number of samples.
            float factor = weight / numSamples;
            for (int coeffIdx = 0; coeffIdx < numCoefficients; ++coeffIdx)
            {
                m_coefficients[coeffIdx] *= factor;
            }

            Shader.SetGlobalMatrix("_TangoLightingSphericalHarmonicMatrixR", _SetShmMatrix(0));
            Shader.SetGlobalMatrix("_TangoLightingSphericalHarmonicMatrixG", _SetShmMatrix(1));
            Shader.SetGlobalMatrix("_TangoLightingSphericalHarmonicMatrixB", _SetShmMatrix(2));
            Shader.SetGlobalFloat("_TangoLightingExposure", m_coefficients[0].magnitude);
        }
    }

    // Returns the _Factorial at an index num.
    private float _Factorial(int num)
    {
        float result = 1.0f;
        for (int i = 2; i <= num; ++i)
        {
            result *= i;
        }

        return result;
    }

    // Evaluate an Associated Legendre Polynomial P(l, m, x) at x.
    // Referenced from the following paper:
    // http://www.research.scea.com/gdc2003/spherical-harmonic-lighting.pdf
    private float P(int l, int m, float x)
    {
        float pmm = 1.0f;
        if (m > 0)
        {
            float somx2 = Mathf.Sqrt((1.0f - x) * (1.0f + x));
            float fact = 1.0f;
            for (int i = 1; i <= m; ++i)
            {
                pmm *= (-fact) * somx2;
                fact += 2.0f;
            }
        }

        if (l == m)
        {
            return pmm;
        }

        float pmmp1 = x * ((2.0f * m) + 1.0f) * pmm;
        if (l == m + 1)
        {
            return pmmp1;
        }

        float pll = 0.0f;
        for (int ll = m + 2; ll <= l; ++ll)
        {
            pll = ((((2.0f * ll) - 1.0f) * x * pmmp1) - ((ll + m - 1.0f) * pmm)) / (ll - m);
            pmm = pmmp1;
            pmmp1 = pll;
        }

        return pll;
    }
    
    // Renormalization constant for SH function.
    // Referenced from the following paper:
    // http://www.research.scea.com/gdc2003/spherical-harmonic-lighting.pdf
    private float K(int l, int m)
    {
        float temp = (((2.0f * l) + 1.0f) * _Factorial(l - m)) / (4.0f * Mathf.PI * _Factorial(l + m));
        return Mathf.Sqrt(temp);
    }
    
    // Return a point sample of a Spherical Harmonic basis function.
    // Referenced from the following paper:
    // http://www.research.scea.com/gdc2003/spherical-harmonic-lighting.pdf
    private float SH(int l, int m, float theta, float phi)
    {
        float sqrt2 = Mathf.Sqrt(2.0f);
        if (m == 0)
        {
            return K(l, 0) * P(l, m, Mathf.Cos(theta));
        }

        if (m > 0)
        {
            return sqrt2 * K(l, m) * Mathf.Cos(m * phi) * P(l, m, Mathf.Cos(theta));
        }

        return sqrt2 * K(l, -m) * Mathf.Sin(-m * phi) * P(l, -m, Mathf.Cos(theta));
    }
    
    // Returns the RGB value at a given theta and phi given a TangoImageBuffer.
    private Vector3 _GetRgbFromImageBuffer(Tango.TangoUnityImageData buffer, int i, int j)
    {
        int width = (int)buffer.width;
        int height = (int)buffer.height;
        int uv_buffer_offset = width * height;

        int x_index = j;
        if (j % 2 != 0)
        {
            x_index = j - 1;
        }

        // Get the YUV color for this pixel.
        int yValue = buffer.data[(i * width) + j];
        int uValue = buffer.data[uv_buffer_offset + ((i / 2) * width) + x_index + 1];
        int vValue = buffer.data[uv_buffer_offset + ((i / 2) * width) + x_index];

        // Convert the YUV value to RGB.
        float r = yValue + (1.370705f * (vValue - 128));
        float g = yValue - (0.689001f * (vValue - 128)) - (0.337633f * (uValue - 128));
        float b = yValue + (1.732446f * (uValue - 128));
        Vector3 result = new Vector3(r / 255.0f, g / 255.0f, b / 255.0f);

        // Gamma correct color to linear scale.
        result.x = Mathf.Pow(Mathf.Max(0.0f, result.x), 2.2f);
        result.y = Mathf.Pow(Mathf.Max(0.0f, result.y), 2.2f);
        result.z = Mathf.Pow(Mathf.Max(0.0f, result.z), 2.2f);
        return result;
    }

    // Compute the spherical harmonic matrix representation of a given RGB channel
    // https://cseweb.ucsd.edu/~ravir/papers/envmap/envmap.pdf
    private Matrix4x4 _SetShmMatrix(int i)
    {
        Matrix4x4 matrix = new Matrix4x4();
        Vector4 col0 = new Vector4(C1 * m_coefficients[8][i],
                                   C1 * m_coefficients[4][i],
                                   C1 * m_coefficients[7][i],
                                   C2 * m_coefficients[3][i]);
        Vector4 col1 = new Vector4(C1 * m_coefficients[4][i],
                                   -C1 * m_coefficients[8][i],
                                   C1 * m_coefficients[5][i],
                                   C2 * m_coefficients[1][i]);
        Vector4 col2 = new Vector4(C1 * m_coefficients[7][i],
                                   C1 * m_coefficients[5][i],
                                   C3 * m_coefficients[6][i],
                                   C2 * m_coefficients[2][i]);
        Vector4 col3 = new Vector4(C2 * m_coefficients[3][i],
                                   C2 * m_coefficients[1][i],
                                   C2 * m_coefficients[2][i],
                                   (C4 * m_coefficients[0][i]) - (C5 * m_coefficients[6][i]));
        matrix.SetColumn(0, col0);
        matrix.SetColumn(1, col1);
        matrix.SetColumn(2, col2);
        matrix.SetColumn(3, col3);
        return matrix;
    }
    
    private struct API
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport(Common.TANGO_UNITY_DLL)]
        public static extern void TangoUnity_updateEnvironmentMap(Int32 glTextureId, int width, int height);
#else
        public static void TangoUnity_updateEnvironmentMap(Int32 glTextureId, int width, int height)
        {
        }
#endif
    }
    
    private struct SphericalHarmonicSample
    {
        public Vector2 sph;
        public Vector3 vec;
        public float[] coeff;
    }
}
