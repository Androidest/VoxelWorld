using UnityEngine;

namespace Assets.Script.Manager
{
    public class ChunksManager : MonoBehaviour
    {
        private PlayerController targetPlayer;

        public void Start()
        {
            targetPlayer = GameManager.Instance.CurPlayerController;
        }

        public void Update()
        {
            
        }
    }
}