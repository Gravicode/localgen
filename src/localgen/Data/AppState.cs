using System.Text.RegularExpressions;

namespace localgen.Data
{
    public class AppState : IDisposable
    {
        public event Action<bool> OnInternetChange;

        public void Dispose()
        {
            //do nothing
        }

        //public UserProfile CurrentUser { get; set; }
        public void RefreshInternet(bool State)
        {
            InternetStateChanged(State);
        }
        private void InternetStateChanged(bool state) => OnInternetChange?.Invoke(state);

    }
}
