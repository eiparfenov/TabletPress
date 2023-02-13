using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace TabletPress
{
    public class UsageController: MonoBehaviour
    {
        [SerializeField] private Switch power;
        [SerializeField] private Switch display;
        [SerializeField] private HoverButton greenTop;
        [SerializeField] private HoverButton greenBottom;
        [SerializeField] private HoverButton redBottom;
        [Space]
        [SerializeField] private Rotor rotor;
        [SerializeField] private TextMeshProUGUI freqText;

        private bool _displayShowText;
        public async UniTask Run()
        {
            print(0);
            await UniTask.WaitUntil(() => power.TurnedOn);
            print(1);
            await UniTask.WaitUntil(() => display.TurnedOn);
            _displayShowText = true;
            print(2);
            await UniTask.WaitUntil(() => greenTop.engaged);
            print(3);
            await UniTask.WaitUntil(() => greenBottom.engaged);
            print(4);
            rotor.rotating = true;
            print(5);
            await UniTask.WaitUntil(() => redBottom.engaged);
            print(6);
            rotor.rotating = false;
            print(7);
            await UniTask.WaitWhile(() => display.TurnedOn);
            _displayShowText = false;
            print(8);
            await UniTask.WaitWhile(() => power.TurnedOn);
        }

        public void Update()
        {
            freqText.text = _displayShowText ? rotor.CurrentRotFreq.ToString("0.00") : "";
        }
    }
}