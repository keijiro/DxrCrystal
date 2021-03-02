using UnityEngine;

namespace DxrCrystal {

static class ObjectFactory
{
    public static GameObject CreateDoubleMeshObject
      (Transform parent, Mesh mesh,
       Material mainMaterial, Material overlayMaterial)
    {
        var root = new GameObject("Instance");
        root.hideFlags = HideFlags.HideAndDontSave;
        root.transform.parent = parent;

        var main = new GameObject
          ("Main Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        main.transform.parent = root.transform;
        main.GetComponent<MeshFilter>().sharedMesh = mesh;
        main.GetComponent<MeshRenderer>().sharedMaterial = mainMaterial;

        if (overlayMaterial != null)
        {
            var overlay = new GameObject
              ("OverlayMesh", typeof(MeshFilter), typeof(MeshRenderer));
            overlay.transform.parent = root.transform;
            overlay.transform.localScale = Vector3.one * 1.01f;
            overlay.GetComponent<MeshFilter>().sharedMesh = mesh;
            overlay.GetComponent<MeshRenderer>().sharedMaterial = overlayMaterial;
        }

        return root;
    }
}

} // namespace DxrCrystal
