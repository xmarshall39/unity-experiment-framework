using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UPBS.Player
{
    public class PBCameraManager : MonoBehaviour
    {
        #region Singleton
        private static PBCameraManager _instance;
        public static PBCameraManager Instance
        {
            get
            {
                return _instance;
            }
        }
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        //Find all of the Playback cameras active in the scene
        //You can retrieve copies of this array and perform actions on each cam externally
        [Tooltip("Add any user-added playback cameras here!")]
        public IPBCameraBase[] additionalPlaybackCameras;


        private IPBCameraBase[] _playbackCameras;
        private int _currentCameraIndex = 0;
        
        public IPBCameraBase[] GetPlaybackCameras()
        {
            return (IPBCameraBase[])_playbackCameras.Clone();
        }

        public bool SetActiveCamera(int index)
        {
            if(index >= 0 && _playbackCameras.Length > index)
            {
                for(int i = 0; i < _playbackCameras.Length; ++i)
                {
                    if(i != index)
                    {
                        _playbackCameras[i].DisableCamera();
                    }
                }

                _playbackCameras[index].EnableCamera();
                _currentCameraIndex = index;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Halts all scene rendering cameras to optimize frame rate
        /// </summary>
        /// <returns>Index of the last active camera</returns>
        public int DisableAllCameras()
        {
            int lastCameraIndex = _currentCameraIndex;
            for (int i = 0; i < _playbackCameras.Length; ++i)
            {
                _playbackCameras[i].DisableCamera();
            }
            _currentCameraIndex = -1;

            return lastCameraIndex;
        }

        public void CycleActiveCameraUp()
        {
            if(_currentCameraIndex == _playbackCameras.Length - 1)
            {
                SetActiveCamera(0);
            }

            else
            {
                SetActiveCamera(_currentCameraIndex + 1);
            }
        }

        public void CycleActiveCameraDown()
        {
            if (_currentCameraIndex == 0)
            {
                SetActiveCamera(_playbackCameras.Length - 1);
            }

            else
            {
                SetActiveCamera(_currentCameraIndex - 1);
            }
        }

        //Default to the first found Camera Reflection. Typically, there should only be one
        public void Initialize()
        {
            var reflectedCameras = FindObjectsOfType<PBCameraReflection>();

            int serializedCameraCount = additionalPlaybackCameras == null ? 0 : _playbackCameras.Length;
            int reflectedCameraCount = reflectedCameras.Length;

            _playbackCameras = new IPBCameraBase[serializedCameraCount + reflectedCameraCount];
            for (int i = 0; i < reflectedCameraCount; ++i)
            {
                _playbackCameras[i] = reflectedCameras[i];
            }

            for (int i = 0; i < serializedCameraCount; ++i)
            {
                _playbackCameras[reflectedCameraCount + i] = additionalPlaybackCameras[i];
            }

            if (_playbackCameras.Length == 0)
            {
                Debug.LogError("No playback cameras found!");
                return;
            }

            SetActiveCamera(0);
        }
    }
}