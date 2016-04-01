/*
 * Copyright 2012 (c) Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 * Author(s):
 *    Dieter Vandroemme
 */

namespace vApus.Results {
    public class VirtualUserResult {

        #region Fields
        private string _virtualUser;

        private RequestResult[] _requestResults;
        #endregion

        #region Properties
        /// <summary>
        ///     When not entered in the test this remains null. This is set in the StressTestCore.
        /// </summary>
        public string VirtualUser {
            get { return _virtualUser; }
            set { _virtualUser = value; }
        }

        /// <summary>
        ///     Use the SetRequestResultAt function to add an item to this. (this fixes the index when using break on last run sync.)
        ///     Don't forget to initialize this the first time.
        ///     Can contain null!
        /// </summary>
        public RequestResult[] RequestResults {
            get { return _requestResults; }
            internal set { _requestResults = value; }
        }

        #endregion

        #region Constructor
        public VirtualUserResult(int logLength) {
            _virtualUser = null;
            _requestResults = new RequestResult[logLength];
        }
        #endregion
    }
}