using System;

namespace Guytp.Data.DatabaseDeltaApplicator
{
    /// <summary>
    /// This class contains the results of delta application.
    /// </summary>
    public class DeltaApplicationResults
    {
        #region Properties
        /// <summary>
        /// Gets a list of deltas that had already been applied.
        /// </summary>
        public int[] AlreadyAppliedDeltas { get; }

        /// <summary>
        /// Gets a list of the successfully applied deltas.
        /// </summary>
        public int[] SuccessfullyAppliedDeltas { get; }

        /// <summary>
        /// Gets whether or not the process was a success.
        /// </summary>
        public bool IsSuccess => !FailedDelta.HasValue;

        /// <summary>
        /// Gets the delta that application failed at, if application of any delta failed.
        /// </summary>
        public int? FailedDelta { get; }

        /// <summary>
        /// Gets the reason delta application failed.
        /// </summary>
        public string FailureReason { get; }

        /// <summary>
        /// Gets the deltas that skipped application if a failed delta was encountered.
        /// </summary>
        public int[] SkippedDeltas { get; }

        /// <summary>
        /// Gets the status messages from this delta application.
        /// </summary>
        public string[] StatusMessages { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="alreadyAppliedDeltas">
        /// A list of deltas that had already been applied.
        /// </param>
        /// <param name="successfullyAppliedDeltas">
        /// A list of the successfully applied deltas.
        /// </param>
        /// <param name="statusMessages">
        /// The status messages from this delta application.
        /// </param>
        public DeltaApplicationResults(int[] alreadyAppliedDeltas, int[] successfullyAppliedDeltas, string[] statusMessages)
        {
            AlreadyAppliedDeltas = alreadyAppliedDeltas;
            SuccessfullyAppliedDeltas = successfullyAppliedDeltas;
            StatusMessages = statusMessages;
        }

        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="alreadyAppliedDeltas">
        /// A list of deltas that had already been applied.
        /// </param>
        /// <param name="successfullyAppliedDeltas">
        /// A list of the successfully applied deltas.
        /// </param>
        /// <param name="statusMessages">
        /// The status messages from this delta application.
        /// </param>
        /// <param name="failedDelta">
        /// The delta that application failed at, if application of any delta failed.
        /// </param>
        /// <param name="skippedDeltas">
        /// The deltas that skipped application if a failed delta was encountered.
        /// </param>
        /// <param name="failureReason">
        /// The reason the delta application failed.
        /// </param>
        public DeltaApplicationResults(int[] alreadyAppliedDeltas, int[] successfullyAppliedDeltas, string[] statusMessages, int failedDelta, int[] skippedDeltas, string failureReason)
            : this (alreadyAppliedDeltas, successfullyAppliedDeltas, statusMessages)
        {
            AlreadyAppliedDeltas = alreadyAppliedDeltas;
            SuccessfullyAppliedDeltas = successfullyAppliedDeltas;
            FailedDelta = failedDelta;
            SkippedDeltas = skippedDeltas;
            FailureReason = failureReason;
        }
        #endregion

        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        /// <returns>
        /// A string describing this object.
        /// </returns>
        public override string ToString()
        {
            string returnMessage = string.Empty;
            if (StatusMessages != null)
                foreach (string message in StatusMessages)
                    returnMessage += message + Environment.NewLine;
            if (FailedDelta.HasValue && (SuccessfullyAppliedDeltas == null || SuccessfullyAppliedDeltas.Length == 0))
                return returnMessage + "Failed to apply any deltas, failed at " + FailedDelta.Value + "." + (AlreadyAppliedDeltas != null && AlreadyAppliedDeltas.Length > 0 ? "  There are " + AlreadyAppliedDeltas.Length + " deltas already applied before this run." : string.Empty) + Environment.NewLine + Environment.NewLine + FailureReason;
            if (FailedDelta.HasValue)
                return returnMessage + "Failed after applying " + SuccessfullyAppliedDeltas.Length + " deltas, failed at " + FailedDelta.Value + "." + (AlreadyAppliedDeltas != null && AlreadyAppliedDeltas.Length > 0 ? "  There are " + AlreadyAppliedDeltas.Length + " deltas already applied before this run." : string.Empty) + Environment.NewLine + Environment.NewLine + FailureReason;
            if (SuccessfullyAppliedDeltas == null || SuccessfullyAppliedDeltas.Length == 0)
                return returnMessage + "No deltas to apply, there are " + AlreadyAppliedDeltas.Length + " deltas already applied before this run.";
            if (AlreadyAppliedDeltas != null && AlreadyAppliedDeltas.Length > 0)
                return returnMessage + "Successfully applied " + SuccessfullyAppliedDeltas.Length + " deltas with " + AlreadyAppliedDeltas.Length + " applied before this run";
            return returnMessage + "Successfully applied " + SuccessfullyAppliedDeltas.Length + " deltas on first run";
        }
    }
}