using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour
{

    [SerializeField]
    private Ball _prefabBall;
    
    [Networked]
    private TickTimer delay { get; set; }
    
    private NetworkCharacterController _cc;
    private Vector3 _forward;


    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _forward = transform.forward;
    }

    public override void FixedUpdateNetwork()
    {

        if(GetInput(out NetworkInputData data))
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
                        Object.InputAuthority, (runner, o) => {
                            //동기화 전 공을 초기화 합니다.
                            o.GetComponent<Ball>().Init();
                        });
                }
            }
        }
    }
}
