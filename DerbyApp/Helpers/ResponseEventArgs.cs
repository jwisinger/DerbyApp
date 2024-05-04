using System;

namespace DerbyApp.Helpers
{
    public class ResponseEventArgs : EventArgs
    {
        public bool Continue { get; set; } = false;
    }
}
