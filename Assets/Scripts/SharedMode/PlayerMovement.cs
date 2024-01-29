using Fusion;
using UnityEngine;

namespace SharedMode
{
    public class PlayerMovement : NetworkBehaviour
    {
        private CharacterController _controller;

        public float PlayerSpeed = 2f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            //소유 플레이어만 움직일수 있음. 모든 플레이어는 소유한 플레이어 오브젝트만 컨트롤 할수 있음
            if (!HasStateAuthority)
                return;

            // FixedUpdateNetwork()에서는 Runner.DeltaTime을 사용해야함  
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;

            _controller.Move(move);

            if(move != Vector3.zero)
            {
                transform.forward = move;
            }
        }
    }

}