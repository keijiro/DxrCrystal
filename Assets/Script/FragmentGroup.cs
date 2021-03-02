using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrCrystal {

sealed class FragmentGroup : MonoBehaviour
{
    [SerializeField] Fragment.Config _fragmentConfig;
    [SerializeField] uint _instanceCount = 100;
    [SerializeField] uint _randomSeed = 1;
    [SerializeField] Mesh[] _meshes;
    [SerializeField] Material _reflectiveMaterial;
    [SerializeField] Material _emissiveMaterial;

    NativeArray<Fragment> _frags;
    TransformAccessArray _taa;

    void Start()
    {
        var hash = new XXHash(_randomSeed);
        var frags = new Fragment[_instanceCount];
        var xforms = new Transform[_instanceCount];

        for (var i = 0u; i < _instanceCount; i++)
        {
            var mesh = _meshes[i % _meshes.Length];
            var go = ObjectFactory.CreateDoubleMeshObject
              ("Fragment", mesh, _reflectiveMaterial, _emissiveMaterial);

            frags[i] = Fragment.InitialState(hash.UInt(i), _fragmentConfig);
            xforms[i] = go.transform;
        }

        _frags = new NativeArray<Fragment>(frags, Allocator.Persistent);
        _taa = new TransformAccessArray(xforms);
    }

    void Update()
      => new FragmentUpdateJob(_frags, _fragmentConfig, Time.deltaTime)
           .Schedule(_taa).Complete();

    void OnDestroy()
    {
        _frags.Dispose();
        _taa.Dispose();
    }
}

} // namespace DxrCrystal
