using UnityEngine;

namespace Util
{
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

            Balance = balance;
            DontDestroyOnLoad(gameObject);
        }
    }
}