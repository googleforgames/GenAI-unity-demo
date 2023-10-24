using UnityEngine;
using UnityEditor;
using System.Collections.Generic;



public class DBK_MeshCombine : ScriptableWizard {
    public GameObject parentOfObjectsToCombine;

    [MenuItem("DBK/Mesh Combine #d")]
    static void CreateWizard() {
        ScriptableWizard.DisplayWizard<DBK_MeshCombine>("DBK Mesh Combine");
    }

    void OnWizardCreate() {
        if(parentOfObjectsToCombine == null) return;

        Vector3 originalPosition = parentOfObjectsToCombine.transform.position;
        parentOfObjectsToCombine.transform.position = Vector3.zero;

        MeshFilter[] meshFilters = parentOfObjectsToCombine.GetComponentsInChildren<MeshFilter>();
        Dictionary<Material, List<MeshFilter>> materialToMeshFilterList = new Dictionary<Material, List<MeshFilter>>();
        List<GameObject> combinedObjects = new List<GameObject>();

        for(int i = 0; i < meshFilters.Length; i++) {
            var materials = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
            if(materials == null) continue;
            if(materials.Length > 1) {
                parentOfObjectsToCombine.transform.position = originalPosition;
                Debug.LogError("Objects with multiple materials on the same mesh are not supported. Create multiple meshes from this object's sub-meshes in an external 3D tool and assign separate materials to each.");
                return;
            }
            var material = materials[0];
            if(materialToMeshFilterList.ContainsKey(material)) materialToMeshFilterList[material].Add(meshFilters[i]);
            else materialToMeshFilterList.Add(material, new List<MeshFilter>() { meshFilters[i] });
        }

        foreach(var entry in materialToMeshFilterList) {
            List<MeshFilter> meshesWithSameMaterial = entry.Value;
            string materialName = entry.Key.ToString().Split(' ')[0];

            CombineInstance[] combine = new CombineInstance[meshesWithSameMaterial.Count];
            for(int i = 0; i < meshesWithSameMaterial.Count; i++) {
                combine[i].mesh = meshesWithSameMaterial[i].sharedMesh;
                combine[i].transform = meshesWithSameMaterial[i].transform.localToWorldMatrix;
            }

            Mesh combinedMesh = new Mesh();            
            combinedMesh.CombineMeshes(combine);
            Unwrapping.GenerateSecondaryUVSet(combinedMesh);
            materialName += "_" + combinedMesh.GetInstanceID();
            AssetDatabase.CreateAsset(combinedMesh, "Assets/DBK/CombinedMesh/Mesh_" + materialName + ".asset");

            string goName = (materialToMeshFilterList.Count > 1)? "Mesh_" + materialName : "Mesh_" + parentOfObjectsToCombine.name;
            GameObject combinedObject = new GameObject(goName);
            var filter = combinedObject.AddComponent<MeshFilter>();
            filter.sharedMesh = combinedMesh;
            var renderer = combinedObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = entry.Key;
            combinedObjects.Add(combinedObject);
        }

        GameObject resultGO = null;
        if(combinedObjects.Count > 1) {
            resultGO = new GameObject("Mesh_" + parentOfObjectsToCombine.name);
            foreach(var combinedObject in combinedObjects) combinedObject.transform.parent = resultGO.transform;
        } else {
            resultGO = combinedObjects[0];
        }

     

        parentOfObjectsToCombine.SetActive(false);
        parentOfObjectsToCombine.transform.position = originalPosition;
        resultGO.transform.position = originalPosition;
    }
}