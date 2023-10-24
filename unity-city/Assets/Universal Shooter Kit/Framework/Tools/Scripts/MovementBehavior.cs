using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace GercStudio.USK.Scripts
{
	public class MovementBehavior : MonoBehaviour
	{
	//	public List<GameObject> Waypoints;
	

		public bool asjustment;

		public AIHelper.CheckPoint currentPoint;
		public List<AIHelper.CheckPoint> points = new List<AIHelper.CheckPoint>();

		public void ChangeIcon(AIHelper.CheckPoint point)
		{
#if UNITY_EDITOR
			switch (point.action)
			{
				case Helper.NextPointAction.NextPoint:
					Helper.AddObjectIcon(point.point, points[0] == point ? "WaypointNextGreen" : "WaypointNextYellow");
					break;
				case Helper.NextPointAction.RandomPoint:
					Helper.AddObjectIcon(point.point, points[0] == point ? "WaypointRandomGreen" : "WaypointRandomYellow");
					break;
				case Helper.NextPointAction.ClosestPoint:
					Helper.AddObjectIcon(point.point, points[0] == point ? "WaypointClosestGreen" : "WaypointClosestYellow");
					break;
				case Helper.NextPointAction.Stop:
					Helper.AddObjectIcon(point.point, "StopWaypoint");
					break;
			}
#endif
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (!asjustment)
				return;
			
			for (var i = 0; i < points.Count; i++)
			{
				if (points[i] == null) continue;
				if (!points[i].point) continue;

//				Handles.color = new Color32(0, 255, 0, 150);
//				Handles.SphereHandleCap(0, points[i].point.transform.position, Quaternion.Euler(0, 0, 0), 2, EventType.Repaint);

//				Gizmos.color = new Color32(0, 255, 0, 255);
//				Gizmos.DrawSphere(points[i].point.transform.position, 1);
				
				switch (points[i].action)
				{
					case Helper.NextPointAction.NextPoint:
					{
						var nextPoint = i + 1;
						if (nextPoint >= points.Count)
						{
							nextPoint = 0;
						}

						if (points[nextPoint] != null)
							if (points[nextPoint].point)
							{
								// points[i].nextPoint = points[nextPoint].point;
								var direction = points[nextPoint].point.transform.position - points[i].point.transform.position;

								var distance = Vector3.Distance(points[nextPoint].point.transform.position, points[i].point.transform.position);
							
								direction.Normalize();

								if (points.Count > 1)
								{
									var endPosition = points[i].point.transform.position + direction * (distance - 1);
									var endPositionForArrows = points[i].point.transform.position + direction * (distance - 4);
									var startPosition = points[i].point.transform.position + direction * 1.2f;
									
									Handles.zTest = CompareFunction.Less;
									Handles.color = new Color32(0, 255, 0, 255);
									Handles.ArrowHandleCap(0, endPositionForArrows, Quaternion.LookRotation(points[nextPoint].point.transform.position - points[i].point.transform.position), 3, EventType.Repaint);
									DrawLine(startPosition, endPosition, new Color32(0, 255, 0, 255), 6);
									
									Handles.zTest = CompareFunction.Greater;
									Handles.color = new Color32(0, 255, 0, 50);
									Handles.ArrowHandleCap(0, endPositionForArrows, Quaternion.LookRotation(points[nextPoint].point.transform.position - points[i].point.transform.position), 3, EventType.Repaint);
									DrawLine(startPosition, endPosition, new Color32(0, 255, 0, 50), 6);
								}
							}

						break;
					}

					case Helper.NextPointAction.RandomPoint:
////						Handles.ArrowHandleCap(0,
////							points[i].point.transform.position + points[i].point.transform.up * 1.2f,
////							Quaternion.Euler(-90, 0, 0), 3, EventType.Repaint);
//
//						GUIStyle style = new GUIStyle();
//						style.fontSize = 15;
//						style.fontStyle = FontStyle.Bold;
//						style.normal.textColor = new Color32(255, 166, 0, 255);
//
//						Handles.Label(points[i].point.transform.position + points[i].point.transform.up * 2, "R",
//							style);

						
						break;
				}
			}
		}
		
		void DrawLine(Vector3 pos1, Vector3 pos2, Color32 color, float thickness)
		{
			Handles.DrawBezier(pos1, pos2, pos1, pos2, color, null, thickness);
		}
#endif
	}
}
