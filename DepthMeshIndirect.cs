using System;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngineInternal;


public class DepthMeshIndirect : MonoBehaviour
{
    public Texture Input;
    public Material mat;
    public Mesh mesh;
    public ComputeShader cs;

    TextureDepthGPUInstancing textureDepthGPUInstancing;
    void Start()
    {
        textureDepthGPUInstancing = new TextureDepthGPUInstancing(Input, mat, mesh, cs);
    }

    void Update()
    {
        textureDepthGPUInstancing.Updateposition();
        textureDepthGPUInstancing.draw();
    }
    void OnDestroy()
    {

        textureDepthGPUInstancing.Release();
    }
}

public class TextureDepthGPUInstancing
{
    // Initialize
    Texture DepthTex;
    Material material;
    Mesh mesh;
    ComputeShader cs;
    // const
    ComputeBuffer positionsBuffer;
    static readonly int
        widthId = Shader.PropertyToID("width"),
        heightId = Shader.PropertyToID("height"),
        positionId = Shader.PropertyToID("_Positions"),
        DepthId = Shader.PropertyToID("DepthTexture");
    int groupX,groupY;
    Bounds bounds;

    public TextureDepthGPUInstancing(Texture DepthTex, Material material,Mesh mesh,ComputeShader cs)
    {
        this.DepthTex = DepthTex;
        this.material = material;
        this.mesh = mesh;
        this.cs = cs;

        positionsBuffer = new ComputeBuffer(DepthTex.width * DepthTex.height, Marshal.SizeOf<Vector3>());

        cs.SetInt(widthId, DepthTex.width);
        cs.SetInt(heightId, DepthTex.height);
        this.groupX = Mathf.CeilToInt(DepthTex.width / 8f);
        this.groupY = Mathf.CeilToInt(DepthTex.height / 8f);
        cs.SetBuffer(0,positionId,positionsBuffer);
        cs.SetTexture(0, DepthId, DepthTex);
        this.bounds = new Bounds(Vector3.zero, 100 * Vector3.one);
    }

    public void  Updateposition()
    {
        cs.Dispatch(0, groupX, groupY, 1);
    }
    public void draw()
    {
        material.SetBuffer(positionId, positionsBuffer);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);
    }

    public void Release()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }
}