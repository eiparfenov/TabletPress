using System;
using System.Linq;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Utils
{
    public class InteractablesHandler: IDisposable
    {
        private readonly Interactable[] _interactables;
        private readonly bool[] _handled;
        private readonly Interactable.OnDetachedFromHandDelegate[] _detachHandlers;
        private readonly Interactable.OnAttachedToHandDelegate[] _attachHandlers;

        public event Action onDetachAll;
        public InteractablesHandler(Interactable[] interactables)
        {
            _interactables = interactables;
            _handled = new bool[interactables.Length];
            _detachHandlers = new Interactable.OnDetachedFromHandDelegate[interactables.Length];
            _attachHandlers = new Interactable.OnAttachedToHandDelegate[interactables.Length];
            for (int i = 0; i < interactables.Length; i++)
            {
                var index = i;
                _detachHandlers[i] = hand =>
                {
                    _handled[index] = false;
                    Check();
                };
                _attachHandlers[i] = hand =>
                {
                    _handled[index] = true;
                    Check();
                };
                _interactables[i].onDetachedFromHand += _detachHandlers[i];
                _interactables[i].onAttachedToHand += _attachHandlers[i];
            }
        }

        private void Check()
        {
            Debug.Log(String.Join(" ", _handled));
            if (_handled.All(x => !x))
            {
                onDetachAll?.Invoke();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _interactables.Length; i++)
            {
                _interactables[i].onDetachedFromHand -= _detachHandlers[i];
                _interactables[i].onAttachedToHand -= _attachHandlers[i];
            }
        }
    }
}