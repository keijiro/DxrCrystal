using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DxrCrystal {

[ExecuteInEditMode]
sealed class LightBarGroup : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [SerializeField] LightBarConfig _config;
    [SerializeField] uint _instanceCount = 100;
    [SerializeField] Mesh _mesh;
    [SerializeField] Material _material;
    [SerializeField] uint _randomSeed = 1;

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
            // We have to insert an empty game object to avoid an issue where
            // prevents game objects with HideFlags from getting ray-traced.
            var go1 = new GameObject("LightBar");
            var go2 = new GameObject("Renderer", typeof(MeshFilter), typeof(MeshRenderer));

            go1.hideFlags = HideFlags.HideAndDontSave;
            go1.transform.parent = transform;
            go2.transform.parent = go1.transform;

            go2.GetComponent<MeshFilter>().sharedMesh = _mesh;
            go2.GetComponent<MeshRenderer>().sharedMaterial = _material;

            xforms[i] = go1.transform;
        }

        _taa = new TransformAccessArray(xforms);
    }

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        Prepare();

        new LightBarUpdateJob(_config, _randomSeed, _time)
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
