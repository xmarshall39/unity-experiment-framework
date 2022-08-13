using UnityEngine;
using System.Linq;
//MIGHT MAKE THIS JUST FOR TRACKER ID'S. DON'T KNOW YET
namespace UPBS.Execution
{
    /// <summary>
    /// Monitors all active trackers and maintains info on them
    /// </summary>
    public class PBTrackerManager : MonoBehaviour
    {
        #region Singleton
        private static PBTrackerManager _instance;
        public static PBTrackerManager Instance
        {
            get
            { 
                return _instance;
            } 
        }
        private void Awake()
        {
            if (_instance)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion
        public int seed = 1234;
        [SerializeField, ReadOnly]
        private PBTrackerID[] _TIDReferences;
        [SerializeField, ReadOnly]
        private int[] _claimedTags;
        
        [EasyButtons.Button]
        private void RegenerateIDTable()
        {
             System.Random random = new System.Random(seed);
            _TIDReferences = FindObjectsOfType<PBTrackerID>();
            _claimedTags = new int[_TIDReferences.Length];
            

            if(_claimedTags.Length >= 999)
            {
                Debug.LogWarning("Exceeded tag limit of 999. Tag request has been denied!");
                return;
            }


            for(int i = 0; i < _claimedTags.Length; ++i)
            {
                int nextTag;
                do
                {
                    nextTag = random.Next(1, 9999);
                }
                while (_claimedTags.Contains(nextTag));

                _claimedTags[i] = nextTag;
                _TIDReferences[i].Init(nextTag);
            }
            
        }
    }
}
