using UnityEngine;

namespace Util
{
    [DefaultExecutionOrder(-10000)]
    public class BalanceProvider : MonoBehaviour
    {
        public static GameBalance Balance { get; private set; }

        [SerializeField] private GameBalance balance;

        private void Awake()
        {
            if (Balance != null)
            {
                Destroy(gameObject);
                return;
            }

            Balance = Instantiate(balance);
           // DontDestroyOnLoad(gameObject);
        }
        private void OnDestroy()
        {
            // Important if you are NOT using DontDestroyOnLoad and you switch scenes
            if (Balance != null) Balance = null;
        }
    }
}