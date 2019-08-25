namespace  DsmSuite.DsmViewer.View.ViewModel
{
    public delegate void ProgressUpdateDelegate(int percentageComplete, string statusText);

    /// <summary>
    /// Interface for all commands - possible which affect the DsmModel
    /// </summary>
    public interface ICommand
	{
        /// <summary>
        /// Run the command
        /// </summary>
        void Execute(ProgressUpdateDelegate updateFunction);

        /// <summary>
        /// Set to true if command was run, false if error or if canceled by user
        /// </summary>
        /// <returns></returns>
        bool Completed
        {
            get;
        }
	}
}
