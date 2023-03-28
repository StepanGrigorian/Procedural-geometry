using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PlaneGenerator : MonoBehaviour
{
    [SerializeField] int x;
    [SerializeField] int y;
    [SerializeField] float step;
    [Space]
    [SerializeField] Material standartMaterial;
    [SerializeField] Material customMaterial;

    [SerializeField] float textureScale;

    [SerializeField] Vector2 textureScrollingSpeed;
    [Range(-10, 10)][SerializeField] float waveSpeed;
    [Range(-10, 10)][SerializeField] float waveIntensity;
    [Range(1, 10)][SerializeField] float wavePeriod;
    [Space]
    [SerializeField] RenderType RenderType;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }
    private void Start()
    {
        meshFilter.mesh = Generate(x, y);
        customMaterial.mainTextureScale = new Vector2(x / textureScale, y / textureScale);
        if(RenderType == RenderType.CPU)
        {
            meshRenderer.material = standartMaterial;
            StartCoroutine(WaveAndScrollAnimation(meshFilter.mesh));
        }
        else
        {
            meshRenderer.material = customMaterial;
        }
    }
    private Mesh Generate(int a, int b)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(a + 1) * (b + 1)];
        Vector2[] uvs = new Vector2[(a + 1) * (b + 1)];
        int[] triangles = new int[a * b * 6];

        for (int i = 0, y = 0; y <= b; y++)
        {
            for (int x = 0; x <= a; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
                uvs[i] = new Vector2((float)x / a, (float)y / b);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;

        int ti = 0, vi = 0;
        for (int y = 0; y < b; y++, vi++)
        {
            for (int x = 0; x < a; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = triangles[ti + 4] = vi + a + 1;
                triangles[ti + 2] = triangles[ti + 3] = vi + 1;
                triangles[ti + 5] = vi + a + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }
    private IEnumerator WaveAndScrollAnimation(Mesh mesh)
    {
        while(true)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3 v;
            for (int i = 0; i < vertices.Length; i++)
            {
                v = vertices[i];
                vertices[i].z = Mathf.Sin((v.x - Time.timeSinceLevelLoad * waveSpeed) / wavePeriod) * waveIntensity;
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            customMaterial.mainTextureOffset += new Vector2(textureScrollingSpeed.x, textureScrollingSpeed.y) * Time.deltaTime;
            if (customMaterial.mainTextureOffset.x > x / textureScale)
            {
                customMaterial.mainTextureOffset = new Vector2(0, customMaterial.mainTextureOffset.y);
            }
            if (customMaterial.mainTextureOffset.y > y / textureScale)
            {
                customMaterial.mainTextureOffset = new Vector2(customMaterial.mainTextureOffset.x, 0);
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
public enum RenderType
{
    CPU,
    GPU
}