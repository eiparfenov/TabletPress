using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace TabletPress
{
    public class UsageController: MonoBehaviour
    {
        public enum State
        {
             
        }
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
            
        }

        public void Update()
        {
            freqText.text = _displayShowText ? rotor.CurrentRotFreq.ToString("0.00") : "";
        }
    }
}