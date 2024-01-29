using Fusion;
using UnityEngine;

namespace SharedMode
{
    public class PlayerMovement : NetworkBehaviour
    {
        private Vector3 _velocity;
        private bool _jumpPressed;

        private CharacterController _controller;

        public float PlayerSpeed = 2f;

        public float JumpForce = 5f;
        public float GravityValue = -9.81f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if(Input.GetButtonDown("Jump"))
            {
                _jumpPressed = true;
            }
        }

        public override void FixedUpdateNetwork()
        {
            //소유 플레이어만 움직일수 있음. 모든 플레이어는 소유한 플레이어 오브젝트만 컨트롤 할수 있음
            if (!HasStateAuthority)
                return;

            if(_controller.isGrounded)
            {
                //작은 경사면을 걸을때도 KCC가 지면에 고정되어 있고, 점프를 해야지만 지면을 떠날수 있도록 하는데 사용됨
                _velocity = new Vector3(0, -1, 0);
            }

            // FixedUpdateNetwork()에서는 Runner.DeltaTime을 사용해야함  
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;

            _velocity.y += GravityValue * Runner.DeltaTime;
            if(_jumpPressed && _controller.isGrounded)
            {
                _velocity.y += JumpForce;
            }
            _controller.Move(move + _velocity * Runner.DeltaTime);

            if(move != Vector3.zero)
            {
                transform.forward = move;
            }

            _jumpPressed = false;
        }
    }

}