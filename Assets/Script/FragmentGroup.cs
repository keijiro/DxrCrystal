using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DxrCrystal {

[ExecuteInEditMode]
sealed class FragmentGroup : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [SerializeField] Fragment.Config _fragmentConfig;
    [SerializeField] uint _instanceCount = 100;
    [SerializeField] uint _randomSeed = 1;
    [SerializeField] Mesh[] _meshes;
    [SerializeField] Material _material;

    #endregion

    #region Private fields

    TransformAccessArray _taa;
    float _time;

    #endregion

    #region ITimeControl implementation

    public void OnControlTimeStart() {}
    public void OnControlTimeStop() {}
    public void SetTime(double time) => _time = (float)time;

    #endregion

    #region IPropertyPreview implementation

    public void GatherProperties
      (PlayableDirector director, IPropertyCollector driver) {}

    #endregion

    #region Fragment object population

    void Prepare()
    {
        if (_taa.isCreated) return;

        var xforms = new Transform[_instanceCount];

        for (var i = 0u; i < _instanceCount; i++)
        {
            var go = new GameObject
              ("Fragment", typeof(MeshFilter), typeof(MeshRenderer));

            go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.parent = transform;

            go.GetComponent<MeshFilter>().
              sharedMesh = _meshes[i % _meshes.Length];

            go.GetComponent<MeshRenderer>().sharedMaterial = _material;

            xforms[i] = go.transform;
        }

        _taa = new TransformAccessArray(xforms);
    }

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        Prepare();

        new FragmentUpdateJob(_fragmentConfig, _randomSeed, _time)
           .Schedule(_taa).Complete();
    }

    void OnDisable()
    {
        if (!_taa.isCreated) return;

        for (var i = 0; i < _taa.length; i++)
            if (Application.isPlaying)
                Destroy(_taa[i].gameObject);
            else
                DestroyImmediate(_taa[i].gameObject);

        _taa.Dispose();
    }

    #endregion
}

} // namespace DxrCrystal
