using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhysicsDemo.Editor
{
    public static class RepairClothMeshes
    {
        [MenuItem("Physics Demo/Repair Station E Cloth Meshes")]
        public static void Repair()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play before repairing the cloth meshes.");
                return;
            }

            int repaired = 0;
            foreach (var renderer in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var mesh = renderer.sharedMesh;
                string path = AssetDatabase.GetAssetPath(mesh);
                if (path != "Assets/Meshes/FlagMesh.asset" && path != "Assets/Meshes/SheetMesh.asset")
                    continue;

                // These stationary cloth grids use their own transform as a single bone.
                // Identity bind pose keeps every vertex in its original local position.
                Undo.RecordObject(mesh, "Repair cloth mesh skinning");
                var weights = new BoneWeight[mesh.vertexCount];
                for (int i = 0; i < weights.Length; i++)
                    weights[i] = new BoneWeight { boneIndex0 = 0, weight0 = 1f };
                mesh.bindposes = new[] { Matrix4x4.identity };
                mesh.boneWeights = weights;
                mesh.RecalculateBounds();
                EditorUtility.SetDirty(mesh);

                Undo.RecordObject(renderer, "Repair cloth renderer");
                renderer.bones = new[] { renderer.transform };
                renderer.rootBone = renderer.transform;
                renderer.localBounds = mesh.bounds;
                renderer.updateWhenOffscreen = true;
                EditorUtility.SetDirty(renderer);
                EditorSceneManager.MarkSceneDirty(renderer.gameObject.scene);
                repaired++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Repaired {repaired} cloth renderers. Save the scene, then add Cloth to Flag and Sheet. Add Cloth Pinner only to Flag to pin its pole edge.");
        }
    }
}
