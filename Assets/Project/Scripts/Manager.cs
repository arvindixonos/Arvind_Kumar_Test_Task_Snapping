using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using DG.Tweening;


namespace MyScripts
{
    public class Manager_State_Menu : IState
    {
        public void EnterState(object targetObject)
        {
            var targetManager = targetObject as Manager;

            targetManager.EnterMenu();
        }

        public void ExitState(object targetObject)
        {
        }

        public void UpdateState(object targetObject)
        {
        }
    }

    public class Manager_State_Game : IState
    {
        public void EnterState(object targetObject)
        {
            var targetManager = targetObject as Manager;

            targetManager.EnterGame();
        }

        public void ExitState(object targetObject)
        {
        }

        public void UpdateState(object targetObject)
        {
        }
    }

    public class Manager : Singleton<Manager>
    {
        public const string STATE_MENU = "Menu";
        public const string STATE_GAME = "Game";

        public CanvasGroup mainCanvas;

        public Texture2D normalCursor;
        public Texture2D hoverCursor;
        public Texture2D selectedCursor;
        public Texture2D invalidCursor;

        private int currentCursorID = -1;

        public Snappable[] snappablePrefabs;
        private Snappable[] snappableObjectPool;

        public Transform[] snappableSpawnPoints;

        private Snappable currentSelectedSnappable;

        private StateMachine stateMachine;

        [SerializeField]
        private string currentStateName;


        public override void Awake()
        {
            base.Awake();

            InitStateMachine();

            SetNormalCursor();
        }

        private void OnEnable()
        {
            EventManager.Instance.SubscribeLogicEvent(LogicEvent);
        }

        private void OnDisable()
        {
            EventManager.Instance.UnsubscribeLogicEvent(LogicEvent);
        }

        public void SetNormalCursor()
        {
            SetCursor(normalCursor);
        }

        public void SetHoverCursor()
        {
            SetCursor(hoverCursor);
        }

        public void SetSelectedCursor()
        {
            SetCursor(selectedCursor);
        }

        public void SetInvalidCursor()
        {
            SetCursor(invalidCursor);
        }

        private void SetCursor(Texture2D targetCursor)
        {
            if(currentSelectedSnappable == null)
            {
                if (currentCursorID != targetCursor.GetInstanceID())
                {
                    currentCursorID = targetCursor.GetInstanceID();

                    Cursor.SetCursor(targetCursor, Vector2.zero, CursorMode.ForceSoftware);
                }
            }
        }

        private void LogicEvent(Dictionary<string, object> message)
        {
            switch (message["eventname"])
            {
                case "Change State to Game":
                    {
                        if (currentStateName.Equals(STATE_MENU))
                        {
                            ChangeState(STATE_GAME);
                        }

                        break;
                    }

                case "Snap Snappables":
                    {
                        if (currentSelectedSnappable != null)
                        {
                            currentSelectedSnappable.SnapSnappables();
                        }

                        break;
                    }

                case "Release All Snappables":
                    {
                        ReleaseAllSnappables();
                        break;
                    }

                case "Reload Scene":
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        break;
                    }

                case "Selected Snappable":
                    {
                        currentSelectedSnappable = message["parameter"] as Snappable;
                        break;
                    }

                case "Deselect Current Snappable":
                    {
                        currentSelectedSnappable = null;
                        break;
                    }

                case "Mouse Hover Snappable":
                    {
                        SetHoverCursor();
                        break;
                    }

                case "Mouse Down Snappable":
                    {
                        SetSelectedCursor();
                        break;
                    }

                case "Mouse Up Snappable":
                    {
                        SetNormalCursor();
                        break;
                    }

                case "Mouse Hover Snapped":
                    {
                        SetInvalidCursor();
                        break;
                    }

                case "Mouse Down Snapped":
                    {
                        SetInvalidCursor();
                        break;
                    }

                case "Mouse Up Snapped":
                    {
                        SetNormalCursor();
                        break;
                    }

                case "Mouse Exit":
                    {
                        SetNormalCursor();
                        break;
                    }
            }
        }

        private void InitStateMachine()
        {
            stateMachine = new StateMachine(this);
            stateMachine.AddState(STATE_MENU, new Manager_State_Menu());
            stateMachine.AddState(STATE_GAME, new Manager_State_Game());

            ChangeState(STATE_MENU);
        }

        private void ChangeState(string stateName)
        {
            currentStateName = stateMachine.ChangeState(stateName);
        }

        public void ShowUI()
        {
            mainCanvas.DOKill();
            mainCanvas.DOFade(1f, 0.5f).SetDelay(0.2f);
        }

        public void HideUI()
        {
            mainCanvas.DOKill();
            mainCanvas.DOFade(0f, 1f);
        }

        public void EnterMenu()
        {
            ShowUI();
        }

        public void EnterGame()
        {
            HideUI();

            PopulateSnappables();
        }

        private void PopulateSnappables()
        {
            var numSnappableSpawnPoints = snappableSpawnPoints.Length;

            snappableObjectPool = new Snappable[numSnappableSpawnPoints];

            var numSnappableObjectPrefabs = snappablePrefabs.Length;

            Random.InitState((int)System.DateTime.Now.Ticks);

            for (int i = 0; i < numSnappableSpawnPoints; i++)
            {
                var randomSnappable = snappablePrefabs[Random.Range(0, numSnappableObjectPrefabs - 1)];

                var randomRotation = Quaternion.Euler(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));

                snappableObjectPool[i] = Snappable.Instantiate(randomSnappable, snappableSpawnPoints[i].position, randomRotation);
            }
        }

        private void ReleaseAllSnappables()
        {
            foreach(var snappable in snappableObjectPool)
            {
                snappable.ClearChildSnappables();
                snappable.ReleaseFromSnappedObject();
            }
        }

        private void Update()
        {
            // Left Mouse Click Input
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                EventManager.Instance.RaiseLogicEvent("Change State to Game");
            }

            // Shift Input
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                EventManager.Instance.RaiseLogicEvent("Snap Snappables");
            }
          
            // Alt Input
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                EventManager.Instance.RaiseLogicEvent("Release All Snappables");
            }

            // Alt Input
            if (Input.GetKeyDown(KeyCode.R))
            {
                EventManager.Instance.RaiseLogicEvent("Reload Scene");
            }
        }

    }
}
