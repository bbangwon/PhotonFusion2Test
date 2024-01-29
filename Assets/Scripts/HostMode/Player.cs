using UnityEngine;
using Fusion;
using TMPro;

namespace HostMode
{
    public class Player : NetworkBehaviour
    {

        [SerializeField]
        private Ball _prefabBall;

        [SerializeField]
        private PhysxBall _prefabPhysxBall;

        [Networked]
        private TickTimer delay { get; set; }

        private NetworkCharacterController _cc;
        private Vector3 _forward;

        [Networked]
        public bool spawned { get; set; }

        private ChangeDetector _changeDetector;

        public Material _material;

        private TextMeshProUGUI _messages;

        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
            _forward = transform.forward;

            _material = GetComponentInChildren<MeshRenderer>().material;
        }

        private void Update()
        {
            if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
            {
                RPC_SendMessage("Hey Mate!");
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RPC_SendMessage(string message, RpcInfo info = default)
        {
            RPC_RelayMessage(message, info.Source);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_RelayMessage(string message, PlayerRef messageSource)
        {
            if (_messages == null)
                _messages = FindObjectOfType<TextMeshProUGUI>();

            if (messageSource == Runner.LocalPlayer)
                _messages.text += $"[You] {message}\n";
            else
                _messages.text += $"{messageSource} {message}\n";

        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public override void FixedUpdateNetwork()
        {

            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _cc.Move(5 * data.direction * Runner.DeltaTime);

                if (data.direction.sqrMagnitude > 0)
                    _forward = data.direction;

                if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
                {
                    if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                    {
                        delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                        Runner.Spawn(_prefabBall,
                            transform.position + _forward,
                            Quaternion.LookRotation(_forward),
                            Object.InputAuthority, (runner, o) =>
                            {
                                //동기화 전 공을 초기화 합니다.
                                o.GetComponent<Ball>().Init();
                            });

                        spawned = !spawned;
                    }

                    else if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                    {
                        delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                        Runner.Spawn(_prefabPhysxBall,
                            transform.position + _forward,
                            Quaternion.LookRotation(_forward),
                            Object.InputAuthority, (runner, o) =>
                            {
                                //동기화 전 공을 초기화 합니다.
                                o.GetComponent<PhysxBall>().Init(10 * _forward);
                            });
                    }
                }
            }
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(spawned):
                        _material.color = Color.white;
                        break;
                    default:
                        break;
                }
            }

            _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
        }
    }

}