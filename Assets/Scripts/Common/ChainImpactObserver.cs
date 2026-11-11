using UnityEngine;

namespace PhysicsDemo
{
    public class ChainImpactObserver : MonoBehaviour
    {
        public ChainReactionMonitor monitor;
        void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody == monitor.ball) monitor.RecordChainImpact();
        }
    }
}
