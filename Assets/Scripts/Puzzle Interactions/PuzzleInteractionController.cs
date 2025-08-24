using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Slates.PuzzleInteractions
{
    public class PuzzleInteractionController : NetworkBehaviour
    {
        private Dictionary<string, IPuzzleInteractor> _interactors = new Dictionary<string, IPuzzleInteractor>();
        private List<string> _guess = new List<string>();

        [Header("Puzzle Settings")]
        [SerializeField, Tooltip("The password to unlock/complete the puzzle")]
        private List<string> _password = new List<string>();
        [SerializeField, Tooltip("Whether the order in which the interactors are activated matter; duplicate inputs are not counted if the password is unordered")]
        private bool _ordered = false;
        [SerializeField, Tooltip("The maximum number of interactors that can be activated at a time")]
        private uint _maxLength = 0;
        [SerializeField, Tooltip("The behaviour when a false/failed attempt is submitted")]
        private PuzzleControllerBehaviour _failureBehaviour;
        [SerializeField, Tooltip("The behaviour when a successful attempt is submitted")]
        private PuzzleControllerBehaviour _successBehaviour;

        [Header("Puzzle References")]
        [SerializeField, Tooltip("The interaction sender, that will invoke its interaction handler upon attempted submission; if null, the puzzle controller will automatically attempt to submit the guess")]
        private GameObject _submitObj = null;
        [SerializeField, Tooltip("The interaction receiver, whose OnInteractionReceived() method will be called when the puzzle completes")]
        private GameObject _successObj;

        private IInteractionSender _submit;
        private IInteractionReceiver _success;

        private void Awake()
        {
            foreach (IPuzzleInteractor interactor in GetComponentsInChildren<IPuzzleInteractor>())
            {
                _interactors.Add(interactor.Key, interactor);
                interactor.Init(this);
            }

            if (_submitObj) _submit = _submitObj.GetComponent<IInteractionSender>();
            _success = _successObj?.GetComponent<IInteractionReceiver>();

            _submit?.Init(this);
            _success?.Init(this);

            if (_success is null)
            {
                Debug.LogError("Success object referenced in puzzle interaction controller has no IInteractionReceiver component.");
                Application.Quit();
            }

            // Set up the submission of the guess if referenced
            if (_submit is not null)
            {
                _submit.OnInteractionSent += Submit;
            }

            // Sanity checks
            if (_maxLength > 0 && _password.Count > _maxLength)
            {
                Debug.LogError($"Password in puzzle interaction controller has length greater than the max length (password length: {_password.Count}; max length: {_maxLength})");
                Application.Quit();
            }
        }

        public void Activate(string key)
        {
            if (_maxLength > 0 && _guess.Count == _maxLength) return;
            if (_ordered && _guess.Contains(key)) return;

            _guess.Add(key);

            // Try to submit if there is no submit interaction sender
            if (_submit is null)
            {
                if (_maxLength > 0 && _guess.Count == _maxLength) Submit();
                else if (_maxLength == 0 && _guess.Count == _password.Count) Submit();
            }
        }

        public void Deactivate(string key)
        {
            _guess.Remove(key);
        }

        public void Submit()
        {
            if (_guess.Count != _password.Count)
            {
                PuzzleFail();
                return;
            }

            if (_ordered)
            {
                for (int i = 0; i < _password.Count; i++)
                {
                    if (_guess[i] != _password[i])
                    {
                        PuzzleFail();
                        return;
                    }
                }
                PuzzleSuccess();
            }
            else
            {
                foreach (string key in _password)
                {
                    if (!_guess.Contains(key))
                    {
                        PuzzleFail();
                        return;
                    }
                }
                PuzzleSuccess();
            }
        }

        private void PuzzleFail()
        {
            RunBehaviour(_failureBehaviour);
        }
        private void PuzzleSuccess()
        {
            RunBehaviour(_successBehaviour);
            _success.OnInteractionReceived();
        }

        private void RunBehaviour(PuzzleControllerBehaviour behaviour)
        {
            if ((behaviour & PuzzleControllerBehaviour.ResetInteractors) != 0)
            {
                foreach (IPuzzleInteractor interactor in _interactors.Values)
                {
                    interactor.Reset();
                }
            }
            if ((behaviour & PuzzleControllerBehaviour.ResetGuess) != 0)
            {
                _guess.Clear();
            }
            if ((behaviour & PuzzleControllerBehaviour.DeactivateInteractors) != 0)
            {
                foreach (IPuzzleInteractor interactor in _interactors.Values)
                {
                    interactor.Disable();
                }
            }
        }
    }

    [System.Flags]
    public enum PuzzleControllerBehaviour
    {
        ResetInteractors = 1 << 0,
        ResetGuess = 1 << 1,
        DeactivateInteractors = 1 << 2,
    }

    public delegate void InteractionHandler();
}
