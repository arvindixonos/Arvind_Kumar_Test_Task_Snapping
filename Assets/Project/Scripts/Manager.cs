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
            Manager targetManager = targetObject as Manager;

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
            Manager targetManager = targetObject as Manager;

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

        public Snappable[] snappablePrefabs;
        private Snappable[] snappableObjectPool;

        public Transform[] snappableSpawnPoints;

        public Snappable currentSelectedSnappable;

        private StateMachine stateMachine;

        [SerializeField]
        private string currentStateName;


        public override void Awake()
        {
            base.Awake();

            mainCanvas.alpha = 0f;

            InitStateMachine();
        }

        public void InitStateMachine()
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
            mainCanvas.alpha = 0f;
            mainCanvas.DOFade(1f, 0.5f).SetDelay(1f);
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

        void ReleaseAllSnappables()
        {
            foreach(Snappable snappable in snappableObjectPool)
            {
                snappable.ClearChildSnappables();
                snappable.ReleaseFromSnappedObject();
            }
        }

        public void Update()
        {
            // Left Mouse Click Input
            if(Input.GetMouseButtonDown(0))
            {
                if(currentStateName.Equals(STATE_MENU))
                {
                    ChangeState(STATE_GAME);
                }
            }

            // Shift Input
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if(currentSelectedSnappable != null)
                {
                    currentSelectedSnappable.SnapSnappables();
                }
            }

            // Alt Input
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                ReleaseAllSnappables();
            }

            // Alt Input
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

    }
}
