using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UPBS.Execution
{
    public abstract class PBVisualization : PBFrameControllerUpdateListener
    {
        [SerializeField]
        private List<int> trackerIDs;

        /// <summary>
        /// Ensure that the provided trackers exist
        /// </summary>
        public void ValidateTrackers()
        {

        }

    }
}

