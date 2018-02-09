namespace SakerCore.Net
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISocketAgentClientSession
    {
        /// <summary>
        /// 
        /// </summary>
        IPipelineSocket AcceptSocket { get; }
        /// <summary>
        /// 
        /// </summary>
        int SessionID { get; }
    }
}