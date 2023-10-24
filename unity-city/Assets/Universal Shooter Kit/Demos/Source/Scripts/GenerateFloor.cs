using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    public class GenerateFloor : MonoBehaviour
    {
        private Transform player;
        public Transform floor;
        public List<Transform> walls = new List<Transform>();

        public int wallsDistance = 5;
        [Range(0, 100)] public float wallsFrequency = 50;

        private float overpassZ;
        private float overpassX;
        private float overpassNegativeZ;
        private float overpassNegativeX;

        private float xFloorPartsCount;
        private float zFloorPartsCount;
        private float negativeXFloorPartsCount;
        private float negativeZFloorPartsCount;

        private Vector3 floorScale;
        private Vector3 originalFloorPosition;
        
        private AIArea surface;

        private bool buildNavMesh;

        private void Start()
        {
            if (floor)
            {
                originalFloorPosition = floor.position;
                
                overpassZ = originalFloorPosition.z;
                overpassX = originalFloorPosition.x;
                overpassNegativeZ = originalFloorPosition.z;
                overpassNegativeX = originalFloorPosition.x;
                
                floorScale = new Vector3(floor.lossyScale.x, floor.lossyScale.y, floor.lossyScale.z);
            }

            foreach (var wall in walls)
            {
                if (wall)
                    wall.gameObject.SetActive(false);
            }

            surface = GetComponent<AIArea>();
        }

        void Update()
        {
            if (!player)
            {
                if (surface && surface.allPlayersInScene.Count > 0)
                {
                    player = surface.allPlayersInScene[0].player.transform;
                }
                
                return;
            }
            
            if (buildNavMesh && surface)
            {
                surface.BuildNavMesh();
                buildNavMesh = false;
            }
            
            if(player.position.y < floor.position.y) return;
            
            // Z Direction
            if (player.transform.position.z > overpassZ && player.transform.position.x < overpassX)
            {
                overpassZ += floorScale.z - 5;

                var instantiatedFloor = Instantiate(floor, new Vector3(originalFloorPosition.x, originalFloorPosition.y, overpassZ), Quaternion.identity, floor.parent);

                GenerateObstacles(instantiatedFloor);

                zFloorPartsCount++;

                for (int i = 1; i <= xFloorPartsCount; i++)
                {
                    instantiatedFloor = Instantiate(floor, new Vector3(originalFloorPosition.x + floorScale.x * i, originalFloorPosition.y, overpassZ), Quaternion.identity, floor.parent);
                    GenerateObstacles(instantiatedFloor);
                }

                for (int i = 1; i <= negativeXFloorPartsCount; i++)
                {
                    instantiatedFloor = Instantiate(floor, new Vector3(originalFloorPosition.x - floorScale.x * i, originalFloorPosition.y, overpassZ), Quaternion.identity, floor.parent);
                    GenerateObstacles(instantiatedFloor);
                }

                buildNavMesh = true;
            }

            // X Direction
            if (player.transform.position.x > overpassX && player.transform.position.z < overpassZ)
            {
                overpassX += floorScale.x - 5;
                
                var instantiatedFloor = Instantiate(floor, new Vector3(overpassX, originalFloorPosition.y, originalFloorPosition.z), Quaternion.identity, floor.parent);
                
                GenerateObstacles(instantiatedFloor);
                
                xFloorPartsCount++;
                
                for (int i = 1; i <= zFloorPartsCount; i++)
                {
                    instantiatedFloor = Instantiate(floor, new Vector3(overpassX, originalFloorPosition.y, originalFloorPosition.z + floorScale.z * i), Quaternion.identity, floor.parent);
                    GenerateObstacles(instantiatedFloor);
                }
                
                for (int i = 1; i <= negativeZFloorPartsCount; i++)
                {
                    instantiatedFloor = Instantiate(floor, new Vector3(overpassX, originalFloorPosition.y, originalFloorPosition.z - floorScale.z * i), Quaternion.identity, floor.parent);
                    GenerateObstacles(instantiatedFloor);
                }

                buildNavMesh = true;
            }

            // Negative Z Direction
            if (player.transform.position.z < overpassNegativeZ)
            {
                overpassNegativeZ -= floorScale.z - 5;
                var instantiatedFloor = Instantiate(floor, new Vector3(originalFloorPosition.x, originalFloorPosition.y, overpassNegativeZ), Quaternion.identity, floor.parent);
                
                GenerateObstacles(instantiatedFloor);
                
                negativeZFloorPartsCount++;
                
                for (int i = 1; i <= xFloorPartsCount; i++)
                {
                    instantiatedFloor = Instantiate(floor, new Vector3(originalFloorPosition.x + floorScale.x * i, originalFloorPosition.y, overpassNegativeZ), Quaternion.identity, floor.parent);
                    GenerateObstacles(instantiatedFloor);
                }
                
                for (int i = 1; i <= negativeXFloorPartsCount; i++)
                {
                    instantiatedFloor= Instantiate(floor, new Vector3(originalFloorPosition.x - floorScale.x * i, originalFloorPosition.y, overpassNegativeZ), Quaternion.identity, floor.parent);
                    GenerateObstacles(instantiatedFloor);
                }
                
                buildNavMesh = true;
            }

            // Negative X Direction
            // if (player.transform.position.x < overpassNegativeX)
            // {
            //     overpassNegativeX -= floorScale.x - 5;
            //
            //     var instantiatedFloor = Instantiate(floor, new Vector3(overpassNegativeX, originalFloorPosition.y, originalFloorPosition.z), Quaternion.identity, floor.parent);
            //     
            //     GenerateObstacles(instantiatedFloor);
            //     negativeXFloorPartsCount++;
            //
            //     for (int i = 1; i <= zFloorPartsCount; i++)
            //     {
            //         Instantiate(floor, new Vector3(overpassNegativeX, originalFloorPosition.y, originalFloorPosition.z + floorScale.z * i), Quaternion.identity, floor.parent);
            //     }
            //     
            //     for (int i = 1; i <= negativeZFloorPartsCount; i++)
            //     {
            //         Instantiate(floor, new Vector3(overpassNegativeX, originalFloorPosition.y, originalFloorPosition.z - floorScale.z * i), Quaternion.identity, floor.parent);
            //     }
            //
            //     bakeNavMesh = true;
            //
            // }
        }

        void GenerateObstacles(Transform targetFloor)
        {
            if(walls.Count == 0) return;

            var position = targetFloor.position;
            
            for (var x = position.x - floorScale.x / 2; x <= position.x + floorScale.x / 2; x += wallsDistance)
            {
                for (var z = position.z - floorScale.z / 2; z <= position.z + floorScale.z / 2; z += wallsDistance)
                {
                    if (Random.value > wallsFrequency / 100)
                    {
                        var wall = walls[Random.Range(0, walls.Count)];

                        if (wall)
                        {
                            var pos = new Vector3(x, position.y + wall.lossyScale.y / 2, z);
                            var instantiate = Instantiate(wall.gameObject, pos, Quaternion.identity);
                            instantiate.transform.parent = targetFloor;
                            instantiate.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}
