using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TabletPress
{
    public class Rotor: MonoBehaviour
    {
        [SerializeField] private Transform rotor;
        [SerializeField] private Transform shaft;
        [SerializeField] private float rotationFreq;
        [SerializeField] private float rotationAcceleration;
        [Space] 
        [SerializeField] private Transform[] pistons;

        [SerializeField] private float startY;

        [SerializeField] private RotorSpeedChanger rotorSpeedChanger;

        [SerializeField] private RotorSettings settings;
        [Space] 
        [SerializeField] private Transform tabletSpawnPoint;
        [SerializeField] private float tabletStartSpeed;
        [SerializeField] private Rigidbody tabletPref;
        [SerializeField] private int installedPistons;
        [field:SerializeField] public bool rotating { get; set; }
        public float CurrentRotFreq { get; private set; }
        
       
        private void Update()
        {
            if (rotating)
            {
                CurrentRotFreq += rotationFreq * rotorSpeedChanger.SpeedMult / rotationAcceleration * Time.deltaTime;
            }
            else
            {
                CurrentRotFreq -= rotationFreq * rotorSpeedChanger.SpeedMult / rotationAcceleration * Time.deltaTime;
            }

            CurrentRotFreq = Mathf.Clamp(CurrentRotFreq, 0, rotationFreq * rotorSpeedChanger.SpeedMult);
            
            shaft.localRotation *= Quaternion.Euler(Vector3.forward * CurrentRotFreq * 360 * Time.deltaTime);

            var mainAngle = rotor.localEulerAngles.y / 360f;
            for (int i = 0; i < pistons.Length; i++)
            {
                var angle = mainAngle;
                var pos = pistons[i].localPosition;
                angle += settings.offset;
                angle -= ((float) i) / ((float)pistons.Length) - 1;
                angle = angle > 1 ? angle - 1 : angle;
                angle = angle > 1 ? angle - 1 : angle;
                
                pos.y = startY + settings.downDistance * (1-settings.pistonMotion.Evaluate(angle));
                pistons[i].localPosition = pos;
            }
            rotor.localRotation = Quaternion.Euler(0, shaft.localEulerAngles.z, 0);
        }

        public async void Start()
        {
            await UniTask.WaitUntil(() => rotating);
            var progress = - rotationFreq * installedPistons / 24;
            while (rotating)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                progress += CurrentRotFreq * Time.deltaTime * installedPistons;
                if (progress > 1)
                {
                    var tablet = Instantiate(tabletPref, tabletSpawnPoint.position, Quaternion.identity);
                    tablet.velocity = -tabletSpawnPoint.forward * tabletStartSpeed;
                    progress -= 1;
                }
            }
        }
    }
}