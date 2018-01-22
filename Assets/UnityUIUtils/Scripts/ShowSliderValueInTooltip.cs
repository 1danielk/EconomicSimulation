﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Nashet.UnityUIUtils
{
    public class ShowSliderValueInTooltip : ToolTipHandler
    {
        // Use this for initialization
        private void Start()
        {
            base.Start();
            this.SetDynamicString(() => "Value: " + GetComponent<Slider>().value);
        }
    }
}
