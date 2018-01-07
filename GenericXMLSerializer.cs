using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace FGMCDocumentCopy.Helpers
{
    public class GenericXmlSerializer<T>
    {
        public string SerializeObject(T obj)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                var stringBuilder = new StringBuilder();
                var stringWriter = new StringWriter(stringBuilder);
                xmlSerializer.Serialize(stringWriter, obj);

                return stringBuilder.ToString();
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to serialize data contract object to xml string:", exception);
            }
        }
        /// <summary>
        /// DeserializeXml
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public T DeserializeXml(string xml)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(new StringReader(xml));
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to deserialize xml string to data contract object:", exception);
            }
        }
    }

}
