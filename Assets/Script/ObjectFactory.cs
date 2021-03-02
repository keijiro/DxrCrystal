using UnityEngine;

namespace DxrCrystal {

static class ObjectFactory
{
    public static GameObject CreateDoubleMeshObject
      (Transform parent, Mesh mesh, Material baseMaterial, Material coverMaterial)
    {
        var rootObject = new GameObject("Instance");

        var baseObject =
          new GameObject("Base Mesh", typeof(MeshFilter), typeof(MeshRenderer));

        var coverObject =
          new GameObject("Cover Mesh", typeof(MeshFilter), typeof(MeshRenderer));

        rootObject.hideFlags = HideFlags.HideAndDontSave;

        rootObject.transform.parent = parent;
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
