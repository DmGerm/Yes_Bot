namespace DNS_YES_BOT.RouteTelegramData
{
    internal interface IRouteData : IDisposable
    {
        public Task SendDataAsync();
    }
}
