using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyScripts
{

    public enum eEventType
    {
        EVENT_LOGIC,
        EVENT_INPUT,
        EVENT_UI,
        EVENT_END
    }

    public class EventManager : Singleton<EventManager>
    {
        public delegate void LogicEvent(Dictionary<string, object> message);
        public static LogicEvent OnLogicEvent;

        public delegate void InputEvent(Dictionary<string, object> message);
        public static InputEvent OnInputEvent;

        public delegate void UIEvent(Dictionary<string, object> message);
        public static UIEvent OnUIEvent;

        private void RaiseEvent(eEventType eventType, object message)
        {
            switch(eventType)
            {
                case eEventType.EVENT_LOGIC:
                    if (OnLogicEvent != null)
                    {
                        OnLogicEvent((Dictionary<string, object>)message);
                    }
                    break;

                case eEventType.EVENT_INPUT:
                    if (OnInputEvent != null)
                    {
                        OnInputEvent((Dictionary<string, object>)message);
                    }
                    break;

                case eEventType.EVENT_UI:
                    if (OnUIEvent != null)
                    {
                        OnUIEvent((Dictionary<string, object>)message);
                    }
                    break;
            }
        }

        public void SubscribeLogicEvent(LogicEvent logicEventHandler)
        {
            OnLogicEvent += logicEventHandler;
        }

        public void UnsubscribeLogicEvent(LogicEvent logicEventHandler)
        {
            OnLogicEvent -= logicEventHandler;
        }

        public void RaiseLogicEvent(string message, object parameter)
        {
            Dictionary<string, object> eventMap = new Dictionary<string, object>();
            eventMap["eventname"] = message;
            eventMap["parameter"] = parameter;

            RaiseEvent(eEventType.EVENT_LOGIC, eventMap);
        }

        public void SubscribeInputEvent(InputEvent inputEventHandler)
        {
            OnInputEvent += inputEventHandler;
        }

        public void UnsubscribeInputEvent(InputEvent inputEventHandler)
        {
            OnInputEvent -= inputEventHandler;
        }

        public void RaiseInputEvent(string message, object parameter)
        {
            Dictionary<string, object> eventMap = new Dictionary<string, object>();
            eventMap["eventname"] = message;
            eventMap["parameter"] = parameter;

            RaiseEvent(eEventType.EVENT_INPUT, eventMap);
        }


        public void SubscribeUIEvent(UIEvent uiEventHandler)
        {
            OnUIEvent += uiEventHandler;
        }

        public void UnsubscribeUIEvent(UIEvent uiEventHandler)
        {
            OnUIEvent -= uiEventHandler;
        }

        public void RaiseUIEvent(string message, object parameter)
        {
            Dictionary<string, object> eventMap = new Dictionary<string, object>();
            eventMap["eventname"] = message;
            eventMap["parameter"] = parameter;

            RaiseEvent(eEventType.EVENT_UI, eventMap);
        }
    }
}
