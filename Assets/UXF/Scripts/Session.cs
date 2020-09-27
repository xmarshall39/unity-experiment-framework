﻿using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using UnityEngine.Events;
using SubjectNerd.Utilities;

namespace UXF
{
    /// <summary>
    /// The Session represents a single "run" of an experiment, and contains all information about that run. 
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(FileIOManager))]
    public class Session : MonoBehaviour, ISettingsContainer, IDataAssociatable
    {
        /// <summary>
        /// Enable to automatically safely end the session when the application is quitting.
        /// </summary>
        [Tooltip("Enable to automatically safely end the session when the application is quitting.")]
        public bool endOnQuit = true;

        /// <summary>
        /// Enable to automatically safely end the session when this object is destroyed.
        /// </summary>
        [Tooltip("Enable to automatically safely end the session when this object is destroyed.")]
        public bool endOnDestroy = true;

        /// <summary>
        /// Enable to automatically end the session when the final trial has ended.
        /// </summary>
        [Tooltip("Enable to automatically end the session when the final trial has ended.")]
        public bool endAfterLastTrial = false;
        
        /// <summary>
        /// If enabled, you do not need to reference this session component in a public field, you can simply call "Session.instance".
        /// </summary>
        [Tooltip("If enabled, you do not need to reference this session component in a public field, you can simply call \"Session.instance\".")]
        public bool setAsMainInstance = true;

        /// <summary>
        /// If enabled, this GameObject will not be destroyed when you load a new scene.
        /// </summary>
        [Tooltip("If enabled, this GameObject will not be destroyed when you load a new scene.")]
        public bool dontDestroyOnLoadNewScene = false;

        /// <summary>
        /// List of blocks for this experiment
        /// </summary>
        [HideInInspector]
        public List<Block> blocks = new List<Block>();

        /// <summary>
        /// Enable to save a copy of the session.settings dictionary to the session folder as a `.json` file. This is written just as the session begins.
        /// </summary>
        [Tooltip("Enable to save a copy of the session.settings dictionary to the session folder as a .json file. This is written just as the session begins.")]
        public bool copySessionSettings = true;

        /// <summary>
        /// Enable to save a copy of the session.participantDetails dictionary to the session folder as a `.csv` file. This is written just as the session begins.
        /// </summary>
        [Tooltip("Enable to save a copy of the session.participantDetails dictionary to the session folder as a .csv file. This is written just as the session begins.")]
        public bool copyParticipantDetails = true;

        /// <summary>
        /// List of dependent variables you plan to measure in your experiment. Once set here, you can add the observations to your results dictionary on each trial.
        /// </summary>
        [Tooltip("List of dependent variables you plan to measure in your experiment. Once set here, you can add the observations to your results dictionary on each trial.")]
        [Reorderable]
        public List<string> customHeaders = new List<string>();

        /// <summary>
        /// List of settings (independent variables) you wish to log to the behavioural file for each trial.
        /// </summary>
        /// <returns></returns>
        [Tooltip("List of settings (independent variables) you wish to log to the behavioural data output for each trial.")]
        [Reorderable]
        public List<string> settingsToLog = new List<string>();

        /// <summary>
        /// List of tracked objects. Add a tracker to a GameObject in your scene and set it here to track position and rotation of the object on each Update().
        /// </summary>
        [Tooltip("List of tracked objects. Add a tracker to a GameObject in your scene and set it here to track position and rotation of the object on each Update().")]
        [Reorderable]
        public List<Tracker> trackedObjects = new List<Tracker>();

        /// <summary>
        /// Event(s) to trigger when the session is initialised. Can pass the instance of the Session as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Tooltip("Event(s) to trigger when the session is initialised. Can pass the instance of the Session as a dynamic argument")]
        public SessionEvent onSessionBegin = new SessionEvent();

        /// <summary>
        /// Event(s) to trigger when a trial begins. Can pass the instance of the Trial as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Tooltip("Event(s) to trigger when a trial begins. Can pass the instance of the Trial as a dynamic argument")]
        public TrialEvent onTrialBegin = new TrialEvent();

        /// <summary>
        /// Event(s) to trigger when a trial ends. Can pass the instance of the Trial as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Tooltip("Event(s) to trigger when a trial ends. Can pass the instance of the Trial as a dynamic argument")]
        public TrialEvent onTrialEnd = new TrialEvent();

        /// <summary>
        /// Event(s) to trigger when the session has ended and all jobs have finished. It is safe to quit the application beyond this event.
        /// </summary>
        /// <returns></returns>
        [Tooltip("Event(s) to trigger when the session has ended and all jobs have finished. It is safe to quit the application beyond this event")]
        public SessionEvent onSessionEnd = new SessionEvent();


        [SerializeField]
        private bool _hasInitialised = false;

        /// <summary>
        /// Returns true if session has been intialised
        /// </summary>
        /// <returns></returns>
        public bool hasInitialised { get { return _hasInitialised; } }

        /// <summary>
        /// Name of the experiment. Data is saved in a folder with this name.
        /// </summary>
        public string experimentName;

        /// <summary>
        /// Unique string for this participant (participant ID)
        /// </summary>
        public string ppid;

        /// <summary>
        /// Current session number for this participant
        /// </summary>
        public int number;

        /// <summary>
        /// Currently active trial number. Be careful of modifying this.
        /// </summary>
        public int currentTrialNum = 0;

        /// <summary>
        /// Currently active block number.
        /// </summary>
        public int currentBlockNum = 0;

        /// <summary>
        /// Settings for the experiment. These are provided on initialisation of the session.
        /// </summary>
        public Settings settings { get; private set; }

        /// <summary>
        /// Returns true if current trial is in progress
        /// </summary>
        public bool InTrial { get { return (currentTrialNum != 0) && (CurrentTrial.status == TrialStatus.InProgress); } }

        /// <summary>
        /// Returns the current trial object.
        /// </summary>
        public Trial CurrentTrial { get { return GetTrial(); } }

        /// <summary>
        /// Returns the next trial object (i.e. trial with trial number currentTrialNum + 1 ).
        /// </summary>
        public Trial NextTrial { get { return GetNextTrial(); } }

        /// <summary>
        /// Get the trial before the current trial.
        /// </summary>
        public Trial PrevTrial { get { return GetPrevTrial(); } }

        /// <summary>
        /// Get the first trial in the first block of the session.
        /// </summary>
        public Trial FirstTrial { get { return GetFirstTrial(); } }

        /// <summary>
        /// Get the last trial in the last block of the session.
        /// </summary>
        public Trial LastTrial { get { return GetLastTrial(); } }

        /// <summary>
        /// Returns the current block object.
        /// </summary>
        public Block CurrentBlock { get { return GetBlock(); } }

        /// <summary>
        /// Returns a list of trials for all blocks.  Modifying the order of this list will not affect trial order. Modify block.trials to change order within blocks.
        /// </summary>
        public IEnumerable<Trial> Trials { get { return blocks.SelectMany(b => b.trials); } }
        
        /// <summary>
        /// The path in which the experiment data are stored.
        /// </summary>
        public string BasePath { get; private set; }

        /// <summary>
        /// Path to the folder used for reading settings and storing the output. 
        /// </summary>
        public string ExperimentPath { get { return Path.Combine(Path.GetFullPath(BasePath), experimentName); } }

        /// <summary>
        /// Path within the experiment path for this particular particpant.
        /// </summary>
        public string ParticipantPath { get { return Path.Combine(ExperimentPath, ppid); } }

        /// <summary>
        /// Stores combined list of headers for the behavioural output.
        /// </summary>
        public List<string> Headers { get { return baseHeaders.Concat(settingsToLog).Concat(customHeaders).ToList(); } }

        /// <summary>
        /// Dictionary of objects for datapoints collected via the UI, or otherwise.
        /// </summary>
        public Dictionary<string, object> participantDetails;

        /// <summary>
        /// An event handler for a C# event.
        /// </summary>
        public delegate void EventHandler();

        /// <summary>
        /// Event raised before session finished, used for UXF functionality. Users should use the similar OnSessionEnd UnityEvent.
        /// </summary>
        public event EventHandler cleanUp;

        /// <summary>
        /// A reference to the main session instance that is currently active.
        /// </summary>
        public static Session instance;

        /// <summary>
        /// The headers that are always included in the trial_results output.
        /// </summary>
        static List<string> baseHeaders = new List<string> { "experiment", "ppid", "session_num", "trial_num", "block_num", "trial_num_in_block", "start_time", "end_time" };

        /// <summary>
        /// Reference to the associated DataHandlers which handles saving data to the cloud, etc.
        /// </summary>
        [Reorderable]
        public DataHandler[] dataHandlers = new DataHandler[]{};

        /// <summary>
        /// Provide references to other components 
        /// </summary>
        void Awake()
        {
            if (setAsMainInstance) instance = this;
            if (dontDestroyOnLoadNewScene && Application.isPlaying) DontDestroyOnLoad(gameObject);            
            if (endAfterLastTrial) onTrialEnd.AddListener(EndIfLastTrial);
        }

        /// <summary>
        /// Checks if there is a risk of overwriting data for this participant and session number
        /// </summary>
        /// <param name="experimentName"></param>
        /// <param name="participantId"></param>
        /// <param name="baseFolder"></param>
        /// <param name="sessionNumber"></param>
        /// <returns></returns>
        public bool CheckSessionExists(string rootPath, string experimentName, string participantId, int sessionNumber)
        {
            bool overwriteRisk = false;
            foreach (var dataHandler in dataHandlers) overwriteRisk = overwriteRisk || dataHandler.CheckIfRiskOfOverwrite(experimentName, participantId, sessionNumber, rootPath: rootPath);

            return overwriteRisk;
        }


        /// <summary>
        /// Initialises a Session
        /// </summary>
        /// <param name="experimentName">A name for the experiment</param>
        /// <param name="participantId">A unique ID associated with a participant</param>
        /// <param name="baseFolder">Location where data should be stored</param>
        /// <param name="sessionNumber">A number for the session (optional: default 1)</param>
        /// <param name="participantDetails">Dictionary of information about the participant to be used within the experiment (optional: default null)</param>
        /// <param name="settings">A Settings instance (optional: default empty settings)</param>
        public void Begin(string experimentName, string participantId, int sessionNumber = 1, Dictionary<string, object> participantDetails = null, Settings settings = null)
        {
            this.experimentName = experimentName;
            ppid = participantId;
            number = sessionNumber;

            if (participantDetails == null)
                participantDetails = new Dictionary<string, object>();
            this.participantDetails = participantDetails;

            if (settings == null)
                settings = Settings.empty;
            this.settings = settings;

            // Initialise DataHandlers
            foreach (var dataHandler in dataHandlers)
            {
                dataHandler.Initialise(this);
                dataHandler.SetUp();
            }
            _hasInitialised = true;

            // raise the session events
            onSessionBegin.Invoke(this);

            if (copySessionSettings)
            {
                // copy Settings to session folder
                SaveJSONSerializableObject(new Dictionary<string, object>(settings.baseDict), "settings", dataType: DataType.SessionInfo);
            }

            if (copyParticipantDetails)
            {
                // copy participant details to session folder
                // we convert to a DataTable because we know the dictionary will be "flat" (one value per key)

                UXFDataTable ppDetailsTable = new UXFDataTable(participantDetails.Keys.ToArray());
                var row = new UXFDataRow();
                foreach (var kvp in participantDetails) row.Add((kvp.Key, kvp.Value));
                ppDetailsTable.AddCompleteRow(row);
                var ppDetailsLines = ppDetailsTable.GetCSVLines();

                SaveDataTable(ppDetailsTable, "participant_details", dataType: DataType.SessionInfo);
            }


        }

        /// <summary>
        /// Create and return 1 Block, which then gets automatically added to Session.blocks  
        /// </summary>
        /// <returns></returns>
        public Block CreateBlock()
        {
            return new Block(0, this);
        }


        /// <summary>
        /// Create and return block containing a number of trials, which then gets automatically added to Session.blocks  
        /// </summary>
        /// <param name="numberOfTrials">Number of trials. Must be greater than or equal to 1.</param>
        /// <returns></returns>
        public Block CreateBlock(int numberOfTrials)
        {
            if (numberOfTrials > 0)
                return new Block((uint)numberOfTrials, this);
            else
                throw new Exception("Invalid number of trials supplied");
        }

        /// <summary>
        /// Get currently active trial. When not in a trial, gets previous trial.
        /// </summary>
        /// <returns>Currently active trial.</returns>
        public Trial GetTrial()
        {
            if (currentTrialNum == 0)
            {
                throw new NoSuchTrialException("There is no trial zero. If you are the start of the experiment please use NextTrial to get the first trial");
            }
            return Trials.ToList()[currentTrialNum - 1];
        }

        /// <summary>
        /// Get trial by trial number (non zero indexed)
        /// </summary>
        /// <returns></returns>
        public Trial GetTrial(int trialNumber)
        {
            return Trials.ToList()[trialNumber - 1];
        }

        /// <summary>
        /// Get next Trial
        /// </summary>
        /// <returns></returns>
        Trial GetNextTrial()
        {
            // non zero indexed
            try
            {
                return Trials.ToList()[currentTrialNum];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no next trial. Reached the end of trial list.");
            }
        }

        /// <summary>
        /// Ends currently running trial. Useful to call from an inspector event
        /// </summary>
        public void EndCurrentTrial()
        {
            CurrentTrial.End();
        }

        /// <summary>
        /// Begins next trial. Useful to call from an inspector event
        /// </summary>
        public void BeginNextTrial()
        {
            NextTrial.Begin();
        }

        /// <summary>
        /// Begins next trial (if one exists). Useful to call from an inspector event
        /// </summary>
        public void BeginNextTrialSafe()
        {            
            if (CurrentTrial != LastTrial)
            {
                BeginNextTrial();
            }
        }

        /// <summary>
        /// Ends the session if the supplied trial is the last trial.
        /// </summary>
        public void EndIfLastTrial(Trial trial)
        {
            if (trial == LastTrial)
            {
                End();
            }
        }

        /// <summary>
        /// Get previous Trial.
        /// </summary>
        /// <returns></returns>
        Trial GetPrevTrial()
        {
            try
            {
                // non zero indexed
                return Trials.ToList()[currentTrialNum - 2];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no previous trial. Probably, currently at the start of session.");
            }
        }

        /// <summary>
        /// Get first Trial in this session.
        /// </summary>
        /// <returns></returns>
        Trial GetFirstTrial()
        {   
            Block firstBlock;
            try
            {
                firstBlock = blocks[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no first trial because no blocks have been created!");
            }

            try
            {
                return firstBlock.trials[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no first trial. No trials exist in the first block.");
            }
        }

        /// <summary>
        /// Get last Trial in this session.
        /// </summary>
        /// <returns></returns>
        Trial GetLastTrial()
        {
            Block lastBlock;
            try
            {
                lastBlock = blocks[blocks.Count - 1];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no last trial because no blocks have been created!");
            }
            
            try
            {
                return lastBlock.trials[lastBlock.trials.Count - 1];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no last trial. No trials exist in the last block.");
            }
        }

        /// <summary>
        /// Get currently active block.
        /// </summary>
        /// <returns>Currently active block.</returns>
        Block GetBlock()
        {
            return blocks[currentBlockNum - 1];
        }

        /// <summary>
        /// Get block by block number (non-zero indexed).
        /// </summary>
        /// <returns>Block.</returns>
        public Block GetBlock(int blockNumber)
        {
            return blocks[blockNumber - 1];
        }


        /// <summary>
        /// Saves a DataTable to the storage locations(s).
        /// </summary>
        /// <param name="table">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveDataTable(UXFDataTable table, string dataName, DataType dataType = DataType.Other)
        {
            foreach(var dataHandler in dataHandlers)
            {
                string location = dataHandler.HandleDataTable(table, experimentName, ppid, number, dataName, dataType: dataType);
            }
        }

        /// <summary>
        /// Saves a JSON Serializable Object to the storage locations(s).
        /// </summary>
        /// <param name="serializableObject">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveJSONSerializableObject(List<object> serializableObject, string dataName, DataType dataType = DataType.Other)
        {
            foreach(var dataHandler in dataHandlers)
            {
                string location = dataHandler.HandleJSONSerializableObject(serializableObject, experimentName, ppid, number, dataName, dataType: dataType);
            }
        }

        /// <summary>
        /// Saves a JSON Serializable Object to the storage locations(s).
        /// </summary>
        /// <param name="serializableObject">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveJSONSerializableObject(Dictionary<string, object> serializableObject, string dataName, DataType dataType = DataType.Other)
        {
            foreach(var dataHandler in dataHandlers)
            {
                string location = dataHandler.HandleJSONSerializableObject(serializableObject, experimentName, ppid, number, dataName, dataType: dataType);
            }
        }

        /// <summary>
        /// Saves a string of text to the storage locations(s).
        /// </summary>
        /// <param name="text">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveText(string text, string dataName, DataType dataType = DataType.Other)
        {
            foreach(var dataHandler in dataHandlers)
            {
                string location = dataHandler.HandleText(text, experimentName, ppid, number, dataName, dataType: dataType);
            }
        }

        /// <summary>
        /// Saves an array of bytes to the storage locations(s).
        /// </summary>
        /// <param name="bytes">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveBytes(byte[] bytes, string dataName, DataType dataType = DataType.Other)
        {
            foreach(var dataHandler in dataHandlers)
            {
                string location = dataHandler.HandleBytes(bytes, experimentName, ppid, number, dataName, dataType: dataType);
            }
        }

        /// <summary>
        /// Ends the experiment session.
        /// </summary>
        public void End()
        {
            if (hasInitialised)
            {
                if (InTrial)
                    CurrentTrial.End();
                SaveResults();

                // raise cleanup event
                if (cleanUp != null) cleanUp();

                // end DataHandler - forces completion of tasks
                foreach (var dataHandler in dataHandlers) dataHandler.CleanUp();
                
                onSessionEnd.Invoke(this);

                currentTrialNum = 0;
                currentBlockNum = 0;
                blocks = new List<Block>();
                _hasInitialised = false;

                Debug.Log("Ended session.");
            }
        }

        void SaveResults()
        {
            // generate list of all headers possible
            // hashset keeps unique set of keys
            HashSet<string> resultsHeaders = new HashSet<string>();
            foreach (Trial t in Trials)
                if (t.result != null)
                    foreach (string key in t.result.Keys)
                        resultsHeaders.Add(key);

            UXFDataTable table = new UXFDataTable(Trials.Count(), resultsHeaders.ToArray());
            foreach (Trial t in Trials)
            {
                if (t.result != null)
                {
                    UXFDataRow row = new UXFDataRow();
                    foreach (string h in resultsHeaders)
                    {
                        if (t.result.ContainsKey(h) && t.result[h] != null)
                        {
                            row.Add(( h, t.result[h].ToString().Replace(",", "_") ));
                        }
                        else
                        {
                            row.Add(( h, string.Empty ));
                        }
                    }
                    table.AddCompleteRow(row);
                }
            }

            SaveDataTable(table, "trial_results", dataType: DataType.TrialResults);            
        }

        void OnApplicationQuit()
        {
            if (endOnQuit)
            {
                End();
            }
        }

        void OnDestroy()
        {
            if (endOnDestroy)
            {
                End();
            }
        }

        [Obsolete("Move to fileiomanager")]
        /// <summary>
        /// Convert a session number to a session name
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string SessionNumToName(int num)
        {
            return string.Format("S{0:000}", num);
        }

    }

    /// <summary>
    /// Exception thrown in cases where we try to access a trial that does not exist.
    /// </summary>
    public class NoSuchTrialException : Exception
    {
        public NoSuchTrialException()
        {
        }

        public NoSuchTrialException(string message)
            : base(message)
        {
        }

        public NoSuchTrialException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


}


