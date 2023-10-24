using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace GercStudio.USK.Scripts
{
	public class SpawnZone : MonoBehaviour
	{
		public Color32 color = Color.green;
		private int layerMask;
		

#if UNITY_EDITOR
		void DrawZone(byte alpha, byte additionalAlpha, byte additionalAlpha2)
		{
			layerMask = ~ (LayerMask.GetMask("Character") | LayerMask.GetMask("Head") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Grass") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));

			var isRaycast = Physics.Raycast(transform.position, Vector3.down, out var hitInfo, 100, layerMask);

			if (isRaycast)
			{
				// Gizmos.color = new Color32(0, 255, 0, 150);
				var pos = transform.position;
				var scale = transform.localScale / 2;
				var rot = Quaternion.Euler(0, transform.eulerAngles.y, 0);

				var verts = new[]
				{
					new Vector3(pos.x - scale.x, (hitInfo.point + Vector3.up * 0.01f).y, pos.z - scale.z),
					new Vector3(pos.x - scale.x, (hitInfo.point + Vector3.up * 0.01f).y, pos.z + scale.z),
					new Vector3(pos.x + scale.x, (hitInfo.point + Vector3.up * 0.01f).y, pos.z + scale.z),
					new Vector3(pos.x + scale.x, (hitInfo.point + Vector3.up * 0.01f).y, pos.z - scale.z) // * Quaternion.Euler(0,rot.y,0) * 
				};

				for (var i = 0; i < verts.Length; i++)
				{
					verts[i] = rot * (verts[i] - pos) + pos;
				}

				var arrayPos = new Vector3(pos.x, (hitInfo.point + Vector3.up * 0.01f).y, pos.z + scale.z);
				arrayPos = rot * (arrayPos - pos) + pos;

				
				Handles.zTest = CompareFunction.Less;
				Handles.color = new Color32(color.r, color.g, color.b, alpha);
				Handles.ArrowHandleCap(0, arrayPos, Quaternion.Euler(0, transform.eulerAngles.y, 0), 2, EventType.Repaint);

				Handles.DrawSolidRectangleWithOutline(verts, new Color32(color.r, color.g, color.b, additionalAlpha), new Color32(0, 0, 0, 255));

				
				Handles.zTest = CompareFunction.Greater;
				Handles.color = new Color32(color.r, color.g, color.b, additionalAlpha);
				Handles.ArrowHandleCap(0, arrayPos, Quaternion.Euler(0, transform.eulerAngles.y, 0), 2, EventType.Repaint);

				Handles.DrawSolidRectangleWithOutline(verts, new Color32(color.r, color.g, color.b, additionalAlpha2), new Color32(0, 0, 0, 100));
			}
		}

		private void OnDrawGizmos()
		{
			DrawZone(255, 100, 30);
		}
#endif
	}
}

