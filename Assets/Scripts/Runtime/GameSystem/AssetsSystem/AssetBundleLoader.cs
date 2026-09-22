using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelPack
{
    private Mesh mesh;

    private Material material;

    public Mesh Mesh
    {
        get
        {
            return mesh;
        }
    }

    public Material Material
    {
        get
        {
            return material;
        }
    }

    public ModelPack(Mesh mesh, Material material)
    {
        this.mesh = mesh;
        this.material = material;
    }
}

public class AssetBundleLoader : MonoBehaviour
{
    private static AssetBundleLoader _instance;
    public static AssetBundleLoader Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    [SerializeField]
    private Mesh pawnMesh;

    [SerializeField]
    private Material pawnMaterial;

    public ModelPack CollectPack(string modelName, string materialName)
    {
        Mesh mesh;
        switch (modelName)
        {
            default:
                mesh = pawnMesh;
                break;
        }
        Material material;
        switch (materialName)
        {
            default:
                material = pawnMaterial;
                break;
        }
        return new ModelPack(mesh, material);
    }
}
