namespace AillieoUtils.EasyLAN
{
    public enum NetPlayerState
    {
        Initialized = 1, // 初始化 未连接
        Connected = 2, // 已建立连接
        Authenticated = 3, // authentication
        Disconnected = 4, // 连接断开 尝试重新连接或等待游戏结束
    }
}
