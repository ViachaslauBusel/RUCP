namespace RUCP
{
    public enum NetworkStatus : int
    {
        /// <summary>
        /// Соединение закрыто
        /// </summary>
        CLOSED = 0,
        /// <summary>
        /// Соединение установлено
        /// </summary>
        СONNECTED = 1,
        /// <summary>
        /// Ожидается соединение
        /// </summary>
        LISTENING = 2
    }
}