using System;
using System.IO;
using System.Net;

namespace FGMCDocumentCopy.Helpers
{
    public class HttpCustomClient
    {
        public byte[] GetResponseData(HttpWebResponse response)
        {

            byte[] result = null;

            if (response == null) { return result; }

            Stream responseStream = null;
            try
            {
                if (response != null)
                {
                    responseStream = response.GetResponseStream();
                    MemoryStream memStream = new MemoryStream();
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }

                    result = new byte[memStream.Length];
                    memStream.Position = 0;
                    memStream.Read(result, 0, result.Length);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (responseStream != null)
                {
                    try
                    {
                        responseStream.Close();
                    }
                    catch (Exception ex2)
                    {
                        // Logging goes here
                    }
                }
            }

            return result;
        }

        public byte[] Post(string url, string postRequestData)
        {
            byte[] result = null;
            HttpWebRequest request = null;
            Uri uri = null;
            Stream memStream = null;
            System.Text.Encoding encoding = System.Text.Encoding.ASCII;
            uri = new Uri(url);
            request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.KeepAlive = true;
            request.Timeout = 1800000;
            memStream = new System.IO.MemoryStream();
            string postData = postRequestData;
            byte[] postDataBytes = encoding.GetBytes(postData);
            memStream.Write(postDataBytes, 0, postDataBytes.Length);

            long contentLength = memStream.Length;
            request.ContentLength = contentLength;

            Stream requestStream = request.GetRequestStream();
            memStream.Position = 0;
            byte[] tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();

            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            result = GetResponseData(response);
            return result;
        }

    }
}
