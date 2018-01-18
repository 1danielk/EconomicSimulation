﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace Nashet.UnityUIUtils
{
    public interface IRefreshable
    {
        void Refresh();
    }
    public interface IHideable
    {
        void Hide();
        void Show();        
    }
   
    abstract public class Hideable : MonoBehaviour, IHideable
    { 
        // declare delegate (type)
        public delegate void HideEventHandler(Hideable eventData);

        //declare event of type delegate
        public event HideEventHandler Hidden;

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            if (Hidden != null)// check for subscribers
                Hidden(this); //fires event for all subscribers 
        }
       
        virtual public void Show()
        {
            gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// Represent UI object that can be refreshed and hidden
    /// </summary>
    abstract public class Window : Hideable, IRefreshable
    {
        public abstract void Refresh();
        override public void Show()
        {
            base.Show();
            Refresh();
        }
    }
    /// <summary>
    /// Represents movable and hideable window
    /// </summary>
    abstract public class DragPanel : Window, IPointerDownHandler, IDragHandler
    {
        private Vector2 pointerOffset;
        private RectTransform canvasRectTransform;
        private RectTransform panelRectTransform;

        public void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRectTransform = canvas.transform as RectTransform;
                panelRectTransform = transform as RectTransform;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            panelRectTransform.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out pointerOffset);
        }

        virtual public void OnDrag(PointerEventData data)
        {
            if (panelRectTransform == null)
                return;

            //Vector2 pointerPostion = ClampToWindow(data);
            //Vector2 ert;
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                //canvasRectTransform, pointerPostion, data.pressEventCamera, out localPointerPosition
                canvasRectTransform, data.position, data.pressEventCamera, out localPointerPosition
            ))
            {
                //ert = localPointerPosition - pointerOffset;
                //panelRectTransform.localPosition = ert;
                GetComponent<RectTransform>().localPosition = localPointerPosition - pointerOffset;
                //GetComponent<RectTransform>().localPosition
            }

        }

        private Vector2 ClampToWindow(PointerEventData data)
        {
            Vector2 rawPointerPosition = data.position;

            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasCorners);

            float clampedX = Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x);
            float clampedY = Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y);

            Vector2 newPointerPosition = new Vector2(clampedX, clampedY);
            return newPointerPosition;

        }

        public override void Hide()
        {
            panelRectTransform.SetAsFirstSibling();
            base.Hide();
        }
        override public void Show()
        {
            base.Show();
            panelRectTransform.SetAsLastSibling();
        }
    }
}