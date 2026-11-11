using UnityEngine;

namespace PhysicsDemo
{
    // Observation only: never moves the ball, cloth, or joints.
    public class ChainReactionMonitor : MonoBehaviour
    {
        public Rigidbody ball;
        public Cloth curtain;
        public Rigidbody chainBottom;
        public WorldLabel status;
        public float curtainX;
        public bool rolled, touchedCloth, hitChain, swung;
        public float rollTime, clothTime, hitTime, swingTime;
        public float maxClothDeflection, maxChainDisplacement;
        [HideInInspector] public bool captureVerification;
        bool capturedCloth, capturedSwing;
        Vector3 ballStart, chainStart;

        void Start()
        {
            ballStart = ball.position;
            chainStart = chainBottom.position;
        }

        void FixedUpdate()
        {
            if (!rolled && ball.position.x > ballStart.x + 1f)
            { rolled = true; rollTime = Time.time; }
            if (hitChain)
            {
                maxChainDisplacement = Mathf.Max(maxChainDisplacement, Mathf.Abs(chainBottom.position.x - chainStart.x));
                if (!swung && maxChainDisplacement > 0.2f)
                { swung = true; swingTime = Time.time; Debug.Log("CHALLENGE PASS: rolled -> cloth contact -> chain impact -> chain swing"); }
            }
        }

        void LateUpdate()
        {
            foreach (var vertex in curtain.vertices)
            {
                var world = curtain.transform.TransformPoint(vertex);
                float deflection = Mathf.Abs(world.x - curtainX);
                if (rolled && !hitChain && Mathf.Abs(ball.position.x - curtainX) < 1.5f)
                {
                    maxClothDeflection = Mathf.Max(maxClothDeflection, deflection);
                    if (!touchedCloth && deflection > 0.08f && Vector3.Distance(world, ball.position) < 0.75f)
                    { touchedCloth = true; clothTime = Time.time; }
                }
            }
            if (status != null)
                status.Text = $"1 Roll: {(rolled ? "OK" : "...")}   2 Cloth: {(touchedCloth ? "OK" : "...")}\n3 Chain impact: {(hitChain ? "OK" : "...")}   Swing: {(swung ? "PASS" : "...")}";
            if (captureVerification && touchedCloth && !capturedCloth && Time.time > clothTime + 0.12f)
            {
                capturedCloth = true;
                System.IO.Directory.CreateDirectory("Temp/ChallengeVerification");
                ScreenCapture.CaptureScreenshot("Temp/ChallengeVerification/cloth-contact.png");
            }
            if (captureVerification && swung && !capturedSwing && Time.time > swingTime + 0.12f)
            {
                capturedSwing = true;
                ScreenCapture.CaptureScreenshot("Temp/ChallengeVerification/chain-swing.png");
            }
        }

        public void RecordChainImpact()
        {
            if (!rolled || !touchedCloth || hitChain) return;
            hitChain = true;
            hitTime = Time.time;
        }

        void OnStationReset()
        {
            rolled = touchedCloth = hitChain = swung = false;
            rollTime = clothTime = hitTime = swingTime = 0;
            maxClothDeflection = maxChainDisplacement = 0;
            capturedCloth = capturedSwing = false;
        }
    }
}
