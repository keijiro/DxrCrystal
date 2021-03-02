using UnityEngine;

namespace DxrCrystal {

static class ObjectFactory
{
    public static GameObject CreateDoubleMeshObject
      (string name, Mesh mesh, Material baseMaterial, Material coverMaterial)
    {
        var rootObject = new GameObject(name);

        var baseObject =
          new GameObject("Base Mesh", typeof(MeshFilter), typeof(MeshRenderer));

        var coverObject =
          new GameObject("Cover Mesh", typeof(MeshFilter), typeof(MeshRenderer));

        baseObject.transform.parent = rootObject.transform;
        coverObject.transform.parent = rootObject.transform;

        baseObject.transform.localScale = Vector3.one * 0.99f;

        baseObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        coverObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        baseObject.GetComponent<MeshRenderer>().sharedMaterial = baseMaterial;
        coverObject.GetComponent<MeshRenderer>().sharedMaterial = coverMaterial;

        return rootObject;
    }
}

} // namespace DxrCrystal
