using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.SpatialMapping;

using System.Threading;

public class PlaneDetector : MonoBehaviour {

	[Range(0, 45)]
        public float SnapToGravityThreshold = 0.0f;

        [Range(0, 10)]
        public float MinArea = 1.0f;

        public bool VisualizeSubPlanes = false;

        private List<PlaneFinding.MeshData> meshData = new List<PlaneFinding.MeshData>();
        private BoundedPlane[] planes;

		void UpdatePlanes(object _)
		{
			//Debug.Log("thread call");
			planes = (VisualizeSubPlanes) ?
                PlaneFinding.FindSubPlanes(meshData, SnapToGravityThreshold) :
                PlaneFinding.FindPlanes(meshData, SnapToGravityThreshold, MinArea);
		}
		/*
		1109 
		meshData를 채우는건 Update에서 하고 meshData를 이용해서 planes를 만드는걸 따른 thread에서 한다고하면
		만약 switching이 발생해서 Update에서 meshData를 clear해버리면 어떡함? (thread에서 meshData를 참조하는 도중에) 
		
		또한 Update에서 thread를 콜하려면, thread 현재 이 작업을 안하고 있다는것을 알아야하지 않나?
		한 번에 딱 하나의 thread만 해당 작업을 해야하는 것 아닌가? 
		*/
        private void Update()
        {
            meshData.Clear();
            foreach (MeshFilter mesh in SpatialMappingManager.Instance.GetMeshFilters())
            {
                meshData.Add(new PlaneFinding.MeshData(mesh));
            }

            ThreadPool.QueueUserWorkItem(UpdatePlanes);
        }

        private static Color[] colors = new Color[] { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.white, Color.yellow };
        private void OnDrawGizmos()
        {
            if (planes != null)
            {
                for (int i = 0; i < planes.Length; ++i)
                {
                    Vector3 center = planes[i].Bounds.Center;
                    Quaternion rotation = planes[i].Bounds.Rotation;
                    Vector3 extents = planes[i].Bounds.Extents;
                    Vector3 normal = planes[i].Plane.normal;
                    center -= planes[i].Plane.GetDistanceToPoint(center) * normal;

                    Vector3[] corners = new Vector3[4] {
                    center + rotation * new Vector3(+extents.x, +extents.y, 0),
                    center + rotation * new Vector3(-extents.x, +extents.y, 0),
                    center + rotation * new Vector3(-extents.x, -extents.y, 0),
                    center + rotation * new Vector3(+extents.x, -extents.y, 0)
                };

                    Color color = colors[i % colors.Length];

                    Gizmos.color = color;
                    Gizmos.DrawLine(corners[0], corners[1]);
                    Gizmos.DrawLine(corners[0], corners[2]);
                    Gizmos.DrawLine(corners[0], corners[3]);
                    Gizmos.DrawLine(corners[1], corners[2]);
                    Gizmos.DrawLine(corners[1], corners[3]);
                    Gizmos.DrawLine(corners[2], corners[3]);
                    Gizmos.DrawLine(center, center + normal * 0.4f);
                }
            }
        }

#if UNITY_EDITOR
        // This relies on helper functionality from the UnityEditor.Handles class, so make it UNITY_EDITOR only
        private void OnDrawGizmosSelected()
        {
            if (planes != null)
            {
                Ray cameraForward = new Ray(Camera.current.transform.position, Camera.current.transform.forward);

                // Draw planes
                for (int i = 0; i < planes.Length; ++i)
                {
                    Vector3 center = planes[i].Bounds.Center;
                    Quaternion rotation = planes[i].Bounds.Rotation;
                    Vector3 extents = planes[i].Bounds.Extents;
                    Vector3 normal = planes[i].Plane.normal;
                    center -= planes[i].Plane.GetDistanceToPoint(center) * normal;

                    Vector3[] corners = new Vector3[4] {
                    center + rotation * new Vector3(+extents.x, +extents.y, 0),
                    center + rotation * new Vector3(-extents.x, +extents.y, 0),
                    center + rotation * new Vector3(-extents.x, -extents.y, 0),
                    center + rotation * new Vector3(+extents.x, -extents.y, 0)
                };

                    Color color = colors[i % colors.Length];

                    // Draw the same plane lines using the Handles class which ignores the depth buffer
                    UnityEditor.Handles.color = color;
                    UnityEditor.Handles.DrawLine(corners[0], corners[1]);
                    UnityEditor.Handles.DrawLine(corners[0], corners[2]);
                    UnityEditor.Handles.DrawLine(corners[0], corners[3]);
                    UnityEditor.Handles.DrawLine(corners[1], corners[2]);
                    UnityEditor.Handles.DrawLine(corners[1], corners[3]);
                    UnityEditor.Handles.DrawLine(corners[2], corners[3]);
#if UNITY_2017_3_OR_NEWER
                    UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.FromToRotation(Vector3.forward, normal), 0.4f, EventType.Ignore);
#else
                    UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.FromToRotation(Vector3.forward, normal), 0.4f, EventType.ignore);
#endif

                    // If this plane is currently in the center of the camera's field of view, highlight it by drawing a
                    // solid rectangle, and display the important details about this plane.
                    float planeHitDistance;
                    if (planes[i].Plane.Raycast(cameraForward, out planeHitDistance))
                    {
                        Vector3 hitPoint = Quaternion.Inverse(rotation) * (cameraForward.GetPoint(planeHitDistance) - center);
                        if (Mathf.Abs(hitPoint.x) <= extents.x && Mathf.Abs(hitPoint.y) <= extents.y)
                        {
                            color.a = 0.1f;
                            UnityEditor.Handles.DrawSolidRectangleWithOutline(corners, color, Color.clear);

                            string text = string.Format("Area: {0} Bounds: {1}\nPlane: N{2}, D({3})",
                                planes[i].Area.ToString("F1"),
                                ((Vector2)extents).ToString("F2"),
                                normal.ToString("F3"),
                                planes[i].Plane.distance.ToString("F3"));

                            UnityEditor.Handles.Label(center, text, GUI.skin.textField);
                        }
                    }
                }
            }
        }
#endif
}
