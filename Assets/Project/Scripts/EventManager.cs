using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyScripts
{
    #region EVENT TYPES

    /// <summary>
    /// Types the events the event manager can handle.
    /// </summary>

    public enum eEventType
    {
        EVENT_LOGIC,
        EVENT_INPUT,
        EVENT_UI,
        EVENT_END
    }

    #endregion 

    /// <summary>
    /// This function of this class is to pass the events raised to the respective event listeners. This class is a singleton.
    /// Currently InputEvent and LogicEvent are commented as this project is not using it.
    /// </summary>

    public class EventManager : Singleton<EventManager>
    {
        #region VARIABLES

        // Logic Event handler and its respective delegate. Delegate object is static.
        public delegate void LogicEvent(Dictionary<string, object> message);
        public static LogicEvent OnLogicEvent;

        // Input Event handler and its respective delegate. Delegate object is static
        public delegate void InputEvent(Dictionary<string, object> message);
        public static InputEvent OnInputEvent;

        // UI Event handler and its respective delegate. Delegate object is static
        public delegate void UIEvent(Dictionary<string, object> message);
        public static UIEvent OnUIEvent;

        #endregion

        #region RAISE EVENT
        /// <summary>
        /// Private function which raise the event according to the type of the event and along with its message parameter.
        /// </summary>
        /// <param name="eventType">Type of Event</param>
        /// <param name="message">Message paramater of generic type object</param>
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

        #endregion

        #region LOGIC EVENT

        /// <summary>
        /// Helper function for any object to subscribe to the logic event.
        /// </summary>
        /// <param name="logicEventHandler">Function of type LogicEvent, which gets called in case of any logic event raised.</param>
        public void SubscribeLogicEvent(LogicEvent logicEventHandler)
        {
            OnLogicEvent += logicEventHandler;
        }

        /// <summary>
        /// Helper function for any object to un-subscribe from the logic event.
        /// </summary>
        /// <param name="logicEventHandler">Function of type LogicEvent, which needs to be removed from the list of logic events.</param>
        public void UnsubscribeLogicEvent(LogicEvent logicEventHandler)
        {
            OnLogicEvent -= logicEventHandler;
        }

        /// <summary>
        /// Raises a logic event with message and an optional paramater of type object.
        /// </summary>
        /// <param name="message">Name of the message</param>
        /// <param name="parameter">Optional paramater of type object. Could be even an array.</param>
        public void RaiseLogicEvent(string message, object parameter = null)
        {
            var eventMap = new Dictionary<string, object>();
            eventMap["eventname"] = message;
            eventMap["parameter"] = parameter;

            RaiseEvent(eEventType.EVENT_LOGIC, eventMap);
        }

        #endregion

        #region INPUT EVENT

        /// <summary>
        /// Helper function for any object to subscribe to the Input event.
        /// </summary>
        /// <param name="inputEventHandler">Function of type InputEvent, which gets called in case of any input event raised.</param>
        public void SubscribeInputEvent(InputEvent inputEventHandler)
        {
            OnInputEvent += inputEventHandler;
        }

        /// <summary>
        /// Helper function for any object to un-subscribe from the logic event.
        /// </summary>
        /// <param name="inputEventHandler">Function of type InputEvent, which needs to be removed from the list of input events.</param>
        public void UnsubscribeInputEvent(InputEvent inputEventHandler)
        {
            OnInputEvent -= inputEventHandler;
        }

        /// <summary>
        /// Raises a input event with message and an optional paramater of type object.
        /// </summary>
        /// <param name="message">Name of the message</param>
        /// <param name="parameter">Optional paramater of type object. Could be even an array.</param>
        public void RaiseInputEvent(string message, object parameter = null)
        {
            var eventMap = new Dictionary<string, object>();
            eventMap["eventname"] = message;
            eventMap["parameter"] = parameter;

            RaiseEvent(eEventType.EVENT_INPUT, eventMap);
        }

        #endregion

        #region UI EVENT

        /// <summary>
        /// Helper function for any object to subscribe to the UI event.
        /// </summary>
        /// <param name="uiEventHandler">Function of type UIEvent, which gets called in case of any ui event raised.</param>
        public void SubscribeUIEvent(UIEvent uiEventHandler)
        {
            OnUIEvent += uiEventHandler;
        }

        /// <summary>
        /// Helper function for any object to un-subscribe from the ui event.
        /// </summary>
        /// <param name="uiEventHandler">Function of type UIEvent, which needs to be removed from the list of ui events.</param>
        public void UnsubscribeUIEvent(UIEvent uiEventHandler)
        {
            OnUIEvent -= uiEventHandler;
        }

        /// <summary>
        /// Raises a ui event with message and an optional paramater of type object.
        /// </summary>
        /// <param name="message">Name of the message</param>
        /// <param name="parameter">Optional paramater of type object. Could be even an array.</param>

        public void RaiseUIEvent(string message, object parameter = null)
        {
            var eventMap = new Dictionary<string, object>();
            eventMap["eventname"] = message;
            eventMap["parameter"] = parameter;

            RaiseEvent(eEventType.EVENT_UI, eventMap);
        }

        #endregion
    }
}
