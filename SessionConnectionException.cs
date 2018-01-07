using System;

namespace EncompassLibrary.CustomException
{
    /// <summary>
    /// For Session Object
    /// </summary>
    [Serializable]
    public class SessionConnectionException:System.Exception
    {

        public SessionConnectionException():base()
        {

        }

        public SessionConnectionException(string message):base(message)
        {

        }

    }
}
