using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HoloToolkit.Unity.SpatialMapping;
public class DebuggingSpatialSource : MonoBehaviour {
    public TextMesh text;
    SpatialMappingSource source;

	// Use this for initialization
	void Start () {
        source = GetComponent<SpatialMappingSource>();
        if(source == null)
        {
            Debug.Log("Source not detected");
        }

        text.text = source.GetType().ToString();
        source.SurfaceAdded += DebugSurfaceAdded;
        source.SurfaceRemoved += DebugSurfaceDeleted;
	}

    public void DebugSurfaceAdded(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e)
    {
        SpatialMappingSource.SurfaceObject obj = e.Data;
        string str = "Added : " + obj.ID;
        Debug.Log(str);
        text.text = str;
    }

    public void DebugSurfaceDeleted(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e)
    {
        SpatialMappingSource.SurfaceObject obj = e.Data;
        string str = "Removed : " + obj.ID;
        Debug.Log(str);
        text.text = str;
    }
}
