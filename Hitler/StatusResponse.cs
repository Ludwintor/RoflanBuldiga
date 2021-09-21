namespace DiscordBot.Hitler
{
    public struct StatusResponse
    {
        public bool success;
        public string errorId;

        /// <summary>
        /// Create status response. Error id can be empty if success
        /// </summary>
        /// <param name="success">Is action succeed?</param>
        /// <param name="errorId">Error id for localization (leave empty if action succeed)</param>
        public StatusResponse(bool success, string errorId)
        {
            this.success = success;
            this.errorId = errorId;
        }
    }
}
