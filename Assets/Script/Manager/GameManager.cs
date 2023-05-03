using UnityEngine;

namespace Assets.Script.Manager
{
    class GameManager : MonoBehaviorSingletonBase<GameManager>
    {
        [SerializeField] public CameraController CameraController;
        [SerializeField] public PlayerController CurPlayerController;
        [SerializeField] public InputController InputController;

        protected override void OnAwake()
        {
            CameraController.SetTarget(CurPlayerController.gameObject.transform);
            InputController.TargetPlayer = CurPlayerController;
        }
    }
}
