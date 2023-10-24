using UnityEngine;

namespace GercStudio.USK.Scripts
{
    public class StealthZone : MonoBehaviour
    {
        public AIArea aiArea;

        public bool hideOnlyWhenSquatting;

        private Transform transformComponent;

        private void Start()
        {
            transformComponent = transform;

            aiArea = FindObjectOfType<AIArea>();
        }

        private void Update()
        {
            var stealthZonePosition = transformComponent.position;
            var stealthZoneScale = transformComponent.localScale;
            
            var zoneMinPoint = new Vector3(stealthZonePosition.x - stealthZoneScale.x / 2, 0, stealthZonePosition.z - stealthZoneScale.z / 2);
            var zoneMaxPoint = new Vector3(stealthZonePosition.x + stealthZoneScale.x / 2, 0, stealthZonePosition.z + stealthZoneScale.z / 2);

            foreach (var player in aiArea.allPlayersInScene)
            {
                if(!player.controller) continue;
                
                var position = player.controller.transform.position;
                
                var inStealthZone = position.x > zoneMinPoint.x && position.z > zoneMinPoint.z && position.x < zoneMaxPoint.x && position.z < zoneMaxPoint.z;

                if (inStealthZone)
                {
                    player.controller.inGrass = !hideOnlyWhenSquatting || player.controller.isCrouch;

                    if (!player.controller.inGrass) player.controller.currentGrassID = -1;
                    else player.controller.currentGrassID = gameObject.GetInstanceID();
                }
                else if (player.controller.inGrass && player.controller.currentGrassID == gameObject.GetInstanceID())
                {
                    player.controller.inGrass = false;
                    player.controller.currentGrassID = -1;
                }
            }
        }

        Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
        {
            var dir = point - pivot;
            dir = Quaternion.Euler(angle) * dir;
            point = dir + pivot;
            return point;
        }

#if UNITY_EDITOR
        void DrawZone()
        {
            var position = transform.position;
			
            var layerMask = ~ (LayerMask.GetMask("Character") | LayerMask.GetMask("Head") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Grass") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));
            var isRaycast = Physics.Raycast(position, Vector3.down, out var hitInfo, 100, layerMask);

            if (!isRaycast) return;

            var pos = new Vector3(position.x, (hitInfo.point + Vector3.up * 0.01f).y, position.z);

            Helper.DrawGizmoRectangle(pos, transform.localScale / 2, transform.eulerAngles, Color.yellow);
        }

        private void OnDrawGizmos()
        {
            DrawZone();
        }
#endif
    }
}
