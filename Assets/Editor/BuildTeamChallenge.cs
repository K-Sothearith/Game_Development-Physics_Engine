using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PhysicsDemo.Editor
{
    public static class BuildTeamChallenge
    {
        const string RootName = "G · Team Challenge";
        static Material Material(string name) => AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");
        static T Ensure<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? Undo.AddComponent<T>(go);

        [MenuItem("Physics Demo/Complete Station E")]
        public static void CompleteE()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            RepairClothMeshes.Repair();
            var flag = GameObject.Find("Flag");
            var sheet = GameObject.Find("Sheet");
            var flagCloth = Ensure<Cloth>(flag);
            flagCloth.useGravity = true;
            flagCloth.externalAcceleration = new Vector3(0, 0, 5);
            flagCloth.randomAcceleration = new Vector3(0, 0, 10);
            flagCloth.damping = 0.2f;
            var flagPin = Ensure<ClothPinner>(flag);
            flagPin.enabled = true;
            flagPin.pinMode = ClothPinner.PinMode.MinXEdge;
            SetConstraints(flagCloth, ClothPinner.PinMode.MinXEdge);
            var sheetCloth = Ensure<Cloth>(sheet);
            sheetCloth.useGravity = true;
            sheetCloth.damping = 0.2f;
            sheetCloth.friction = 0.5f;
            var sheetPin = Ensure<ClothPinner>(sheet);
            sheetPin.enabled = true;
            sheetPin.pinMode = ClothPinner.PinMode.None;
            SetConstraints(sheetCloth, ClothPinner.PinMode.None);
            var spheres = Object.FindObjectsByType<SphereCollider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var ball = spheres.First(s => s.name == "Ball under sheet");
            var floor = spheres.First(s => s.name.StartsWith("Cloth floor"));
            sheetCloth.sphereColliders = new[] { new ClothSphereColliderPair(ball), new ClothSphereColliderPair(floor) };
            foreach (var c in new Component[] { flagCloth, flagPin, sheetCloth, sheetPin }) EditorUtility.SetDirty(c);
            EditorSceneManager.MarkSceneDirty(flag.scene);
            EditorSceneManager.SaveScene(flag.scene);
        }

        static void SetConstraints(Cloth cloth, ClothPinner.PinMode mode)
        {
            var vertices = cloth.GetComponent<SkinnedMeshRenderer>().sharedMesh.vertices;
            var coefficients = cloth.coefficients;
            float edge = mode == ClothPinner.PinMode.MaxYEdge ? vertices.Max(v => v.y) : vertices.Min(v => v.x);
            for (int i = 0; i < coefficients.Length; i++)
            {
                float coord = mode == ClothPinner.PinMode.MaxYEdge ? vertices[i].y : vertices[i].x;
                coefficients[i].maxDistance = mode != ClothPinner.PinMode.None && Mathf.Abs(coord - edge) < 0.01f ? 0 : float.MaxValue;
            }
            cloth.coefficients = coefficients;
        }

        static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, string material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = Material(material);
            return go;
        }

        static void Label(Transform parent, string name, Vector3 position, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            var label = go.AddComponent<WorldLabel>();
            label.text = text;
            label.offset = Vector3.zero;
        }

        [MenuItem("Physics Demo/Build Team Challenge")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            if (GameObject.Find(RootName) != null) throw new InvalidOperationException("Challenge already exists; edit its objects instead of rebuilding over them.");
            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Build team challenge");
            var t = root.transform;
            Box(t, "Runway", new Vector3(0, -0.25f, 40), new Vector3(24, 0.5f, 4), "Ground");
            Box(t, "Ball catch barrier", new Vector3(10, 1, 40), new Vector3(0.4f, 2, 4), "Wood");
            foreach (float z in new[] { 38.1f, 41.9f })
                Box(t, "Runway side rail", new Vector3(3, 0.25f, z), new Vector3(18, 0.5f, 0.15f), "Wood");
            var ramp = Box(t, "1 Ramp - gravity start", new Vector3(-5, 1.8f, 40), new Vector3(8, 0.3f, 3), "Wood");
            ramp.transform.rotation = Quaternion.Euler(0, 0, -22);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Rolling ball";
            ball.transform.SetParent(t);
            ball.transform.position = new Vector3(-8, 3.85f, 40);
            ball.transform.localScale = Vector3.one * 1.1f;
            ball.GetComponent<Renderer>().sharedMaterial = Material("Orange");
            var rb = ball.AddComponent<Rigidbody>();
            rb.mass = 3;
            rb.linearDamping = 0;
            rb.angularDamping = 0.01f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Box(t, "Curtain header", new Vector3(1.5f, 3.3f, 40), new Vector3(0.15f, 0.15f, 3.7f), "Wood");
            foreach (float z in new[] { 38.25f, 41.75f })
                Box(t, "Curtain post", new Vector3(1.5f, 1.6f, z), new Vector3(0.15f, 3.2f, 0.15f), "Wood");
            var curtain = new GameObject("2 Hanging cloth - top edge pinned");
            curtain.transform.SetParent(t);
            curtain.transform.position = new Vector3(1.5f, 0, 40);
            var mesh = new Mesh { name = "ChallengeCurtain" };
            int columns = 24, rows = 24;
            var vertices = new Vector3[(columns + 1) * (rows + 1)];
            var uv = new Vector2[vertices.Length];
            var weights = new BoneWeight[vertices.Length];
            var triangles = new int[columns * rows * 6];
            for (int y = 0; y <= rows; y++) for (int x = 0; x <= columns; x++)
            {
                int i = y * (columns + 1) + x;
                vertices[i] = new Vector3(0, 0.1f + 3.1f * y / rows, -1.6f + 3.2f * x / columns);
                uv[i] = new Vector2((float)x / columns, (float)y / rows);
                weights[i] = new BoneWeight { boneIndex0 = 0, weight0 = 1 };
            }
            int tri = 0;
            for (int y = 0; y < rows; y++) for (int x = 0; x < columns; x++)
            {
                int a = y * (columns + 1) + x, b = a + columns + 1;
                triangles[tri++] = a; triangles[tri++] = b; triangles[tri++] = a + 1;
                triangles[tri++] = a + 1; triangles[tri++] = b; triangles[tri++] = b + 1;
            }
            mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles;
            mesh.bindposes = new[] { Matrix4x4.identity }; mesh.boneWeights = weights;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath("Assets/Meshes/ChallengeCurtain.asset"));
            var skin = curtain.AddComponent<SkinnedMeshRenderer>();
            skin.sharedMesh = mesh; skin.bones = new[] { curtain.transform }; skin.rootBone = curtain.transform;
            skin.localBounds = mesh.bounds; skin.updateWhenOffscreen = true; skin.sharedMaterial = Material("ClothBlue");
            var cloth = curtain.AddComponent<Cloth>();
            cloth.useGravity = true; cloth.damping = 0.15f; cloth.friction = 0.3f;
            cloth.stretchingStiffness = 0.9f; cloth.bendingStiffness = 0.1f;
            cloth.enableContinuousCollision = true; cloth.useVirtualParticles = 1f;
            cloth.clothSolverFrequency = 120;
            cloth.sphereColliders = new[] { new ClothSphereColliderPair(ball.GetComponent<SphereCollider>()) };
            var pin = curtain.AddComponent<ClothPinner>(); pin.pinMode = ClothPinner.PinMode.MaxYEdge;
            SetConstraints(cloth, ClothPinner.PinMode.MaxYEdge);

            Box(t, "Chain header", new Vector3(5, 3.95f, 40), new Vector3(0.3f, 0.25f, 3.7f), "Wood");
            foreach (float z in new[] { 38.25f, 41.75f })
                Box(t, "Chain post", new Vector3(5, 1.9f, z), new Vector3(0.2f, 3.8f, 0.2f), "Wood");
            var monitor = root.AddComponent<ChainReactionMonitor>();
            monitor.ball = rb; monitor.curtain = cloth; monitor.curtainX = 1.5f;
            Rigidbody previous = null;
            for (int i = 0; i < 5; i++)
            {
                var link = new GameObject($"3 Chain link {i + 1}");
                link.transform.SetParent(t); link.transform.position = new Vector3(5, 3.5f - i * 0.65f, 40);
                var collider = link.AddComponent<BoxCollider>(); collider.size = new Vector3(0.25f, 0.65f, 0.4f);
                var visual = Box(link.transform, "Link mesh", link.transform.position, collider.size, i % 2 == 0 ? "Gold" : "Dark");
                Object.DestroyImmediate(visual.GetComponent<Collider>());
                var body = link.AddComponent<Rigidbody>(); body.mass = 0.3f; body.angularDamping = 0.1f;
                body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.Continuous;
                body.solverIterations = 20; body.solverVelocityIterations = 10;
                var hinge = link.AddComponent<HingeJoint>(); hinge.axis = Vector3.forward;
                hinge.anchor = new Vector3(0, 0.325f, 0); hinge.autoConfigureConnectedAnchor = false;
                hinge.connectedBody = previous;
                hinge.connectedAnchor = previous == null ? new Vector3(5, 3.825f, 40) : new Vector3(0, -0.325f, 0);
                var observer = link.AddComponent<ChainImpactObserver>(); observer.monitor = monitor;
                previous = body;
            }
            monitor.chainBottom = previous;
            var status = new GameObject("Challenge progress"); status.transform.SetParent(t); status.transform.position = new Vector3(0, 5.2f, 40);
            monitor.status = status.AddComponent<WorldLabel>(); monitor.status.offset = Vector3.zero;
            monitor.status.text = "Press Play: gravity roll -> cloth -> chain";
            Label(t, "Ramp label", new Vector3(-6, 4.7f, 40), "1 · Rigidbody + ramp\nGravity starts the ball");
            Label(t, "Cloth label", new Vector3(1.5f, 3.7f, 40), "2 · Cloth\nBall pushes the hanging fabric");
            Label(t, "Chain label", new Vector3(5, 4.5f, 40), "3 · Hinge joints\nImpact swings five connected links");
            var focus = Object.FindFirstObjectByType<CameraFocusMenu>();
            Undo.RecordObject(focus, "Add challenge camera focus");
            var spots = focus.spots.ToList();
            spots.Add(new CameraFocusMenu.Spot { name = "Team Challenge", point = new Vector3(-1, 2, 40), distance = 21, root = t });
            focus.spots = spots.ToArray(); EditorUtility.SetDirty(focus);
            var orbit = Object.FindFirstObjectByType<OrbitCamera>();
            orbit.yaw = 25; orbit.pitch = 16; orbit.FocusOn(new Vector3(-1, 2, 40), 21);
            EditorUtility.SetDirty(orbit);
            EditorSceneManager.MarkSceneDirty(root.scene);
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(root.scene);
            Selection.activeGameObject = root;
        }
    }
}
