using UnityEngine;
using UnityEngine.Jobs;

namespace DxrCrystal {

sealed class FragmentGroup : MonoBehaviour
{
    [SerializeField] Fragment.Config _fragmentConfig;
    [SerializeField] uint _instanceCount = 100;
    [SerializeField] uint _randomSeed = 1;
    [SerializeField] Mesh[] _meshes;
    [SerializeField] Material _reflectiveMaterial;
    [SerializeField] Material _emissiveMaterial;

    TransformAccessArray _taa;

    void Start()
    {
        var xforms = new Transform[_instanceCount];

        for (var i = 0u; i < _instanceCount; i++)
        {
            var mesh = _meshes[i % _meshes.Length];
            var go = ObjectFactory.CreateDoubleMeshObject
              ("Fragment", mesh, _reflectiveMaterial, _emissiveMaterial);

            xforms[i] = go.transform;
        }

        _taa = new TransformAccessArray(xforms);
    }

    void Update()
      => new FragmentUpdateJob(_fragmentConfig, _randomSeed, Time.time)
           .Schedule(_taa).Complete();

    void OnDestroy()
      => _taa.Dispose();
}

} // namespace DxrCrystal
