using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.Rendering;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MyScripts
{
    /// <summary>
    /// Menu State of the Manager Object.
    /// </summary>
    public class Manager_State_Menu : IState
    {
        /// <summary>
        /// Shows the menu of the target manager object.
        /// </summary>
        /// <param name="targetObject">Typically target Manager object</param>
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

    /// <summary>
    /// Game state of the Manager Object.
    /// </summary>
    public class Manager_State_Game : IState
    {
        /// <summary>
        /// Hides Menu and Enters the game.
        /// </summary>
        /// <param name="targetObject">Typically target Manager object</param>
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

    /// <summary>
    /// Functionality of this class is the manage all the snappable objects, UI, displaying appropriate cursor and handle logic events.
    /// </summary>
    public class Manager : Singleton<Manager>
    {
        // Consts for state names.
        public const string STATE_MENU = "Menu";
        public const string STATE_GAME = "Game";

        // Main canvas instance.
        public CanvasGroup mainCanvas;

        // Respective cursors.
        public Texture2D normalCursor;
        public Texture2D hoverCursor;
        public Texture2D selectedCursor;

        // Instance ID of the currentCursor.
        private Texture2D currentCursor;

        // Array of Snappable Prefabs.
        public Snappable[] snappablePrefabs;

        // Object pool array of the instantiated snappables.
        private Snappable[] snappableObjectPool;

        // Array of snappable snap locations.
        public Transform[] restZones;

        // Current Selected Snappable which can snap other objects to itself.
        public Snappable currentSelectedSnappable;

        // Our state machine. Manages our states.
        private StateMachine stateMachine;

        // Name of the current state we are in.
        [SerializeField]
        private string currentStateName;

        private bool cursorInsideScreen = false;
        private bool isDefaultCursorSet = false;

        /// <summary>
        /// Monobehaviour function, initiates the statemachine and sets the cursor to normal cursor.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Initializing the statemachine
            InitStateMachine();

            // Set the initial cursor to normal cursor
            SetNormalCursor();
        }

        /// <summary>
        /// Monobehaviour function, Adds a listener to the logic event.
        /// </summary>
        private void OnEnable()
        {
            EventManager.Instance.SubscribeLogicEvent(LogicEvent);
        }

        /// <summary>
        /// Monobehaviour function, Removes the added logic event listener.
        /// </summary>
        private void OnDisable()
        {
            EventManager.Instance.UnsubscribeLogicEvent(LogicEvent);
        }

        /// <summary>
        /// Monobehaviour update loop.
        /// </summary>
        private void Update()
        {
            // Left Mouse Click Input
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                EventManager.Instance.RaiseLogicEvent("Change State to Game");
            }

            // Shift Input
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                EventManager.Instance.RaiseLogicEvent("Snap to Target");
            }

            // R Input
            if (Input.GetKeyDown(KeyCode.R))
            {
                EventManager.Instance.RaiseLogicEvent("Reload Scene");
            }

            cursorInsideScreen = MouseScreenCheck();
            if (!cursorInsideScreen)
            {
                if(!isDefaultCursorSet)
                {
                    SetDefaultCursor();
                }
            }
            else
            {
                if (isDefaultCursorSet)
                {
                    SetCurrentCursor();
                }
            }
        }

        #region CURSOR FUNCTIONS

        /// <summary>
        /// Sets the current cursor to the normal cursor referenced in the manager object.
        /// </summary>
        private void SetNormalCursor()
        {
            SetCursor(normalCursor);
        }

        /// <summary>
        /// Sets the current cursor to the hover cursor referenced in the manager object.
        /// </summary>
        private void SetHoverCursor()
        {
            SetCursor(hoverCursor);
        }

        /// <summary>
        /// Sets the current cursor to the selected cursor referenced in the manager object.
        /// </summary>
        private void SetSelectedCursor()
        {
            SetCursor(selectedCursor);
        }

        private void SetCurrentCursor()
        {
            if (currentCursor != null)
            {
                Cursor.SetCursor(currentCursor, Vector2.zero, CursorMode.ForceSoftware);

                isDefaultCursorSet = false;
            }
            else
            {
                SetDefaultCursor();
            }
        }

        private void SetDefaultCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            
            isDefaultCursorSet = true;
        }

        /// <summary>
        /// Sets the current cursor to the target cursor parameter.
        /// </summary>
        /// <param name="targetCursor">Target Cursor to set as current cursor of type Texture2D.</param>
        private void SetCursor(Texture2D targetCursor)
        {
            if (currentCursor != targetCursor)
            {
                currentCursor = targetCursor;

                Cursor.SetCursor(targetCursor, Vector2.zero, CursorMode.ForceSoftware);

                isDefaultCursorSet = false;
            }
        }

        public bool MouseScreenCheck()
        {
#if UNITY_EDITOR
            if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= Handles.GetMainGameViewSize().x - 1 || Input.mousePosition.y >= Handles.GetMainGameViewSize().y - 1)
            {
                return false;
            }
#else
        if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1) {
        return false;
        }
#endif
            else
            {
                return true;
            }
        }
        #endregion

        #region LOGIC EVENT

        /// <summary>
        /// Function gets called for handling logic events raised in the system.
        /// </summary>
        /// <param name="message">Message dict with message name as key and paramater as object.</param>
        private void LogicEvent(Dictionary<string, object> message)
        {
            switch (message["eventname"])
            {
                case "Change State to Game":
                    {
                        // Changes state to STATE_GAME if we are in the STATE_MENU state.
                        if (currentStateName.Equals(STATE_MENU))
                        {
                            ChangeState(STATE_GAME);
                        }

                        break;
                    }

                case "Snap to Target":
                    {
                        // If current selected snappable is not null, try to snap the nearest snappable to it.
                        if (currentSelectedSnappable != null)
                        {
                            currentSelectedSnappable.SnapToCurrentTarget();
                        }

                        break;
                    }

                case "Reload Scene":
                    {
                        // Reload the current active scene.
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        break;
                    }

                case "Selected Snappable":
                    {
                        // Set the current selected snappable from the paramater object of the message.
                        currentSelectedSnappable = message["parameter"] as Snappable;
                        break;
                    }

                case "Deselect Current Snappable":
                    {
                        // Nullifies the current selected snappable.
                        currentSelectedSnappable = null;
                        break;
                    }

                case "Mouse Hover Snappable":
                    {
                        // Set the cursor to hover cursor.
                        SetHoverCursor();
                        break;
                    }

                case "Mouse Down Snappable":
                    {
                        // Set the cursor to selected cursor.
                        SetSelectedCursor();
                        break;
                    }

                case "Mouse Up Snappable":
                    {
                        if(currentSelectedSnappable == null)
                        {
                            // Set the cursor to normal cursor.
                            SetNormalCursor();
                        }
                        
                        break;
                    }

                case "Mouse Exit":
                    {
                        if (currentSelectedSnappable == null)
                        {
                            // Set the cursor to normal cursor.
                            SetNormalCursor();
                        }
                        break;
                    }

                case "Snappable Snapped to Rest Zone":
                    {
                        // Set the cursor to normal cursor.
                        SetNormalCursor();
                        break;
                    }
            }
        }

        #endregion

        #region STATE-MACHINE

        /// <summary>
        /// Creates a new statemachine instance for this object and adds the states menu and game to its list.
        /// </summary>
        private void InitStateMachine()
        {
            stateMachine = new StateMachine(this);
            stateMachine.AddState(STATE_MENU, new Manager_State_Menu());
            stateMachine.AddState(STATE_GAME, new Manager_State_Game());

            // Set the initial state to menu
            ChangeState(STATE_MENU);
        }

        /// <summary>
        /// Changes the state of the object.
        /// </summary>
        /// <param name="stateName">Name of the state to change to.</param>
        private void ChangeState(string stateName)
        {
            currentStateName = stateMachine.ChangeState(stateName);
        }

        #endregion

        #region SHOW/HIDE UI

        /// <summary>
        /// Fades in the main canvas
        /// </summary>
        public void ShowUI()
        {
            mainCanvas.DOKill();
            mainCanvas.DOFade(1f, 0.5f).SetDelay(0.2f);
        }

        /// <summary>
        /// Fades out the main canvas 
        /// </summary>
        public void HideUI()
        {
            mainCanvas.DOKill();
            mainCanvas.DOFade(0f, 1f);
        }

        #endregion

        #region ENTER MENU and ENTER GAME

        /// <summary>
        /// Shows the menu UI
        /// </summary>
        public void EnterMenu()
        {
            ShowUI();
        }

        /// <summary>
        /// Hides the Menu UI and Populates the snappables object pool.
        /// </summary>
        public void EnterGame()
        {
            HideUI();

            PopulateSnappables();
        }

        #endregion

        #region SNAPPABLES

        /// <summary>
        /// Populates the snappables object pool by instantiating a random prefab from the snappable prefab list and 
        /// puts in the snappable spawn position.
        /// </summary>
        private void PopulateSnappables()
        {
            var numSnappableSpawnPoints = restZones.Length;

            snappableObjectPool = new Snappable[numSnappableSpawnPoints];

            var numSnappableObjectPrefabs = snappablePrefabs.Length;

            Random.InitState((int)System.DateTime.Now.Ticks);

            for (int i = 0; i < numSnappableSpawnPoints; i++)
            {
                var randomSnappable = snappablePrefabs[Random.Range(0, numSnappableObjectPrefabs)];

                var randomRotation = Quaternion.Euler(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));

                snappableObjectPool[i] = Snappable.Instantiate(randomSnappable, restZones[i].position, randomRotation);

                snappableObjectPool[i].InitSnappable(restZones[i].position);
            }
        }

        #endregion
    }
}
