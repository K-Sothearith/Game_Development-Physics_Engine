# W4 – Physics Engine: Template Project

This is the W4 template project. It is made from the `starter` branch of the [PhysicDemoGame](https://github.com/CADT-Gen11-GameDevelopment/PhysicDemoGame) project and contains only **one scene, the Starter Sandbox**, and the scripts and assets that scene needs. Use it in class to add physics components live in the Inspector and show what they do.

The full project (10 demo scenes, 10 Inspector Labs, main menu and the Physics Courier game) is on the `main` branch of PhysicDemoGame.

## Environment

| Item | Choice |
|---|---|
| Unity version | **6000.3.8f1** (Unity 6.3) |
| Render pipeline | **Built-in Render Pipeline** (Standard shader) |
| Input | **New Input System** (`com.unity.inputsystem` 1.18.0). *Active Input Handling* = Input System Package (New). |
| UI | **UGUI**, built from code with Unity's built-in `LegacyRuntime.ttf` font. TextMeshPro is not used. |
| Physics | Built-in only (PhysX): Rigidbody, Colliders, Joints, Cloth, ParticleSystem. No third-party assets. |

Project settings changed from the defaults:
- Layers 6–10 are `Player`, `Wall`, `Glass`, `Enemy`, `Pickup`.
- Default solver iterations are 10 and velocity iterations are 4, to make joint chains more stable.
- The Game-view gizmo lines use `Assets/Resources/DebugLines.mat` (shader `PhysicsDemo/DebugLines`).

Unity 6 API names (if you compare with older tutorials): `Rigidbody.linearVelocity` (was `velocity`), `linearDamping` / `angularDamping` (was `drag` / `angularDrag`), `PhysicsMaterial` (was `PhysicMaterial`).

## How to open and run

1. In Unity Hub, click **Add → Add project from disk** and select this folder. Open it with **6000.3.8f1**.
2. Open `Assets/Scenes/00_Starter_Sandbox.unity`.
3. Select an object in the Hierarchy, click **Add Component** in the Inspector, then press **Play**.

To get a clean scene again after you saved changes into it, discard them with git:

```bash
git checkout -- Assets/Scenes/00_Starter_Sandbox.unity
```

## Controls

| Input | Action |
|---|---|
| "Camera focus" dropdown | Jump the camera to a station |
| Right mouse drag | Orbit the camera |
| Mouse wheel | Zoom |
| W A S D / Q E | Move the camera / down, up (hold Shift to move faster) |
| T | Slow motion on/off (`Time.timeScale` = 0.2 and `Time.fixedDeltaTime` scaled with it) |
| G | Gizmos on/off (velocity arrows, contact points, joint anchors, collider wireframes) |
| Tab | Show or hide the info panel |
| R / "Reset station (R)" button | Reset only the station chosen in "Camera focus" (all stations when "Overview" is chosen) |
| Shift+R / "Reset scene (Shift+R)" button | Reload the whole scene. The camera and the "Camera focus" choice stay where they were |
| Space | Put the fast balls back at their start and fire them again |

## Stations

Each station is a group in the Hierarchy. The objects have no Rigidbody, joint, Cloth or Particle System yet: these are the components you add.

| Station | Objects | Try |
|---|---|---|
| A · Rigidbody | Cube, Sphere, Capsule floating at 3 m; Platform; Ramp; Wall | Add Rigidbody; change Mass, Use Gravity, Is Kinematic, Linear Damping |
| B · Colliders | Ghost cube (no collider), Rock (mesh, no collider), Table (parent + 5 children, no colliders) | Add a Rigidbody first: the object falls through the floor. Then add a collider. Rock: MeshCollider with Convex on. Table: Rigidbody on the parent, BoxCollider on each child (compound collider) |
| C · Collision detection | Fast ball A and B (with `StartVelocity`, 100 m/s), Thin wall A and B (0.05 m, with `CollisionCounter`), Backstop | Add a Rigidbody to both balls (before or during Play): they fire at the walls, and on Discrete they pass through. Set one ball to Continuous / Continuous Dynamic and press **Space** to put both balls back and fire again. Each wall's label counts its hits and shows the last ball's mode |
| D · Joints | Beam (anchor), Hanging box, Chain (5 links), Door post + Door, Cube A + Cube B | HingeJoint / SpringJoint on the hanging box. Chain: Rigidbody + HingeJoint on each link, Connected Body = the link above. Door: Rigidbody + HingeJoint, Axis (0, 1, 0), Anchor (0, 0, -0.5). Fixed / Spring joint from Cube B to Cube A |
| E · Cloth | Flag (on its pole), Sheet above a ball, a huge sphere collider used as the cloth floor | Add Cloth. Pin the flag's pole edge (Edit Cloth Constraints, or add `ClothPinner`). On the sheet, add the ball and the cloth floor to Cloth ▸ Sphere Colliders (cloth ignores all other colliders) |
| F · Particles | Fire emitter (points up) on a campfire, Spark emitter (points at a wall) | Add Particle System. Try the Shape, Emission, Color over Lifetime and Collision modules |

**Two resets.** *Reset station* (R) puts the focused station's objects back where they started, stops the Rigidbodies, restarts cloth and particles, fires the fast balls again and sets the wall hit counters to 0. Components you added during Play are **kept**, so you can run the same demo again. *Reset scene* (Shift+R) reloads everything, which also removes the components added during Play.

Rule to show in class: changes you make to components **during Play** are undone when Play stops. Changes made outside Play mode are kept when you save the scene.

## Station E and F walkthrough

Do the setup outside Play mode and save the scene to keep your changes.

### E: Flag and sheet

If the Inspector says the assigned mesh is missing bone weights with bind poses or blend shapes:

1. Wait for Unity to compile the editor script, then choose **Physics Demo > Repair Station E Cloth Meshes** with the Starter Sandbox open and Play stopped.
2. Save the scene. This repairs the Flag and Sheet meshes with a single bone and matching bind pose; it does not add the simulation components.
3. Select **E · Cloth > Flag**, add **Cloth**, and enable **Use Gravity**. Press Play: with no pinned constraints, the flag should fall. Do not add a Rigidbody.
4. Stop Play. Add **Cloth Pinner** to Flag, with **Pin Mode = Min X Edge**. Set Cloth **External Acceleration = (0, 0, 5)** and **Random Acceleration = (0, 0, 10)**. Play again: the pole edge stays fixed while the flag hangs and waves.
5. Stop Play. Select **Sheet**, add **Cloth**, and enable **Use Gravity**. Leave Cloth Pinner off the sheet so it can drop freely.
6. Expand the sheet's Cloth **Sphere Colliders**, set the size to **2**, and assign **Ball under sheet** to the first entry's **First** field and **Cloth floor (huge sphere collider)** to the second entry's **First** field. Leave both **Second** fields empty; these are two independent spheres.
7. Save and Play. Focus the camera on Cloth. The sheet should drape over the ball; press **R** to repeat. Cloth only uses its explicitly assigned supported colliders.

### F: Fire and sparks

1. Stop Play. Under **F · Particles**, select **Fire emitter (points up)** and add **Particle System**.
2. In its main module set **Looping = on**, **Start Lifetime = 1**, **Start Speed = 2**, **Start Size = 0.2**, and **Gravity Modifier = 0**.
3. Enable **Shape**: **Cone**, **Angle = 10**, **Radius = 0.2**. In **Emission**, start with **Rate over Time = 50**.
4. Enable **Color over Lifetime**. Edit its gradient: yellow at the start, red near the end; set alpha to **1** at the start and **0** at the end. Enable **Size over Lifetime** and make its curve shrink from **1** to **0**.
5. Select **Spark emitter (points at wall)** and add **Particle System**. Set **Start Speed = 10**, **Gravity Modifier = 1**, **Start Size = 0.1**, and **Start Lifetime = 2**. The existing emitter transform supplies its direction.
6. Enable its **Collision** module: **Type = World**, **Mode = 3D**, **Bounce = 0.5**. Make sure **Collides With** includes the wall's layer and the wall has a non-trigger collider. No Rigidbody is needed on the particles.
7. Save and Play; focus on Particles. Flames should rise and fade, and sparks should bounce off the wall. If particles are pink or invisible, assign a Built-in particle material in the **Renderer** module; use a shader that supports vertex color and transparency for the fade.

For the practice record: flag External/Random Acceleration are **(0, 0, 5)** / **(0, 0, 10)**; spark Bounce/Gravity Modifier are **0.5** / **1**.

## Team challenge: ramp, cloth, chain

The saved Starter Sandbox includes **G · Team Challenge**, with its own **Team Challenge** camera-focus entry.

Press **Play** once: the orange ball rolls down the ramp under gravity, pushes through the hanging blue cloth, then strikes the bottom of the five-link hinge chain. The top of the cloth remains pinned while its lower portion bends around the ball. The ball is explicitly assigned to the curtain's Cloth Sphere Colliders. The catch barrier and side rails keep the ball on the stage after the impact.

This uses Station A's Rigidbody, Station B's colliders, Station D's Hinge Joints, and Station E's Cloth. No scripted launch or timed movement drives the sequence. `ChainReactionMonitor` and `ChainImpactObserver` only observe rolling, cloth deformation near the ball, an actual ball/chain collision, and subsequent chain displacement; the floating status shows the results. Select **Team Challenge** in Camera focus before pressing **R** to repeat only this setup. A fresh Stop/Play also repeats it.

Unity Cloth has one-way collision response: the ball deforms the fabric, while the fabric does not exert force on the Rigidbody. The curtain briefly wraps around the passing ball; it remains hanging rather than being carried to the chain.

The editor menus **Physics Demo > Complete Station E** and **Physics Demo > Build Team Challenge** provide the setup tools. The challenge builder refuses to overwrite an existing challenge group.

## Helper components

These scripts in `Assets/Scripts/Common/` can be added with **Add Component** to make a demo easier to see or control:

| Component | What it does |
|---|---|
| `StartVelocity` | Launches the Rigidbody with a set velocity as soon as the object has one; Space puts it back and fires again (used by the fast balls) |
| `CollisionCounter` | Counts `OnCollisionEnter` hits and shows them on the object's `WorldLabel` (used by the thin walls) |
| `VelocityGizmo` | Draws the Rigidbody's velocity as an arrow in the Game view |
| `ContactGizmo` | Shows contact points and normals while two bodies touch |
| `JointGizmo` | Draws each joint's anchor and the line to its connected anchor |
| `ColliderWireframe` | Draws the colliders under an object as wireframes, so the physics shape is visible |
| `ClothPinner` | Pins cloth vertices (for example the flag's pole edge) without painting constraints by hand |
| `MouseGrabber` | Drag Rigidbodies with the mouse (uses a SpringJoint) |
| `KeyboardMover` | Moves a kinematic Rigidbody with the arrow keys (Z / X = down / up) |
| `KinematicMover` | Moves a kinematic Rigidbody back and forth with `MovePosition` / `MoveRotation` |
| `Launcher` | Click to fire a ball from the camera with `AddForce(ForceMode.Impulse)`. Assign a Rigidbody prefab to Projectile Prefab first |
| `ImpactReporter` | Reports the first collision (`OnCollisionEnter`) after it is armed |
| `Coin` | A trigger collectible (`OnTriggerEnter`) that disappears when touched |
| `GuardVision` | A rotating fan of raycasts that detects objects on the Player layer |
| `Lifetime` | Destroys the object after a delay, or when it falls far below the world |

The scene rig (`_DemoRig`) uses `DemoSceneSetup`, `DemoHUD`, `SceneHotkeys` and `StationReset` (the two resets). The camera uses `OrbitCamera` and `CameraFocusMenu`. `Assets/Materials` and `Assets/PhysicMaterials` hold ready-made materials and Physic Materials (Ice, Rubber, Wood, Bounce 0.0 / 0.5 / 0.9...).
