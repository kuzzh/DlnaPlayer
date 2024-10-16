namespace DlnaPlayerApp.WebSocket.Protocol
{
    public enum WebCmdType
    {
        /// <summary>
        /// 查询播放状态
        /// </summary>
        QueryPlayState = 0,
        /// <summary>
        /// 暂停播放
        /// </summary>
        PausePlay = 1,
        /// <summary>
        /// 继续播放
        /// </summary>
        ResumePlay = 2,
        /// <summary>
        /// 播放指定视频
        /// </summary>
        PlayVideo = 3,
        /// <summary>
        /// 刷新列表
        /// </summary>
        RefreshList = 4
    }
}