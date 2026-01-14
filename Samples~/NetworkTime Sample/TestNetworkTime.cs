using UnityEngine;

namespace DBD.NetworkTime.Sample
{
    public class TestNetworkTime : MonoBehaviour
    {
        void Start()
        {
            TestNetworkTimeManager.Instance.GetNetworkTime();
        }

        private void OnEnable()
        {
            TestNetworkTimeManager.OnCompleted += OnCompleted;
        }

        private void OnDisable()
        {
            TestNetworkTimeManager.OnCompleted -= OnCompleted;
        }

        private void OnCompleted()
        {
            Debug.Log($"datdb - {TestNetworkTimeManager.Instance.GetDateTime()}");
        }
    }
}