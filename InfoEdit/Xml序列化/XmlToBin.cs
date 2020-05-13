using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace InfoEdit
{
    class XmlToBin
    {/// <summary>
     /// XML的格式转换
     /// </summary>
        public class XMLToBin
        {
            //public string a = "a";
            private static XMLToBin instance;

            public static XMLToBin Instance
            {
                get
                {
                    if (XMLToBin.instance == null)
                    {
                        XMLToBin.instance = new XMLToBin();
                    }
                    return XMLToBin.instance;
                }
                set { XMLToBin.instance = value; }
            }
            public XMLToBin()
            {
                if (XMLToBin.instance != null)
                {
                    //InstanceNoInstantiationException exp = new InstanceNoInstantiationException(typeof(XMLToBin));
                    //Console.WriteLine(exp.Message);
                    //throw exp;
                }
                else
                {
                    XMLToBin.instance = this;
                }
            }
            /// <summary>
            /// Object to XML
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="obj"></param>
            /// <param name="path"></param>
            /// <returns></returns>
            public bool Serializer<T>(object obj, string path)
            {
                FileStream xmlfile = new FileStream(path, FileMode.OpenOrCreate);

                //创建序列化对象 
                XmlSerializer xml = new XmlSerializer(typeof(T));
                try
                {    //序列化对象
                    xml.Serialize(xmlfile, obj);
                    xmlfile.Close();
                }
                catch (InvalidOperationException)
                {
                    throw;
                }

                return true;

            }
            /// <summary>
            /// XML to Object
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="path"></param>
            /// <returns></returns>
            public static T Deserializer<T>(string path)
            {
                try
                {
                    FileStream xmlfile = new FileStream(path, FileMode.Open);
                    XmlSerializer xml = new XmlSerializer(typeof(T));
                    T t = (T)xml.Deserialize(xmlfile);
                    xmlfile.Close();
                    return t;
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (FileNotFoundException)
                { throw; }
                finally
                {

                }
            }
            /// <summary>
            /// Object to Bin
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="path"></param>
            /// <returns></returns>
            public bool BinarySerializer(object obj, string path)
            {
                FileStream Stream = new FileStream(path, FileMode.OpenOrCreate);
                //创建序列化对象 
                BinaryFormatter bin = new BinaryFormatter();
                try
                {    //序列化对象
                    bin.Serialize(Stream, obj);
                    Stream.Close();
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                return true;
            }
            /// <summary>
            /// Bin to Object
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="path"></param>
            /// <returns></returns>
            public T BinaryDeserializer<T>(string path)
            {
                try
                {
                    FileStream binfile = new FileStream(path, FileMode.Open);

                    BinaryFormatter bin = new BinaryFormatter();
                    //序列化对象
                    //xmlfile.Close();
                    T t = (T)bin.Deserialize(binfile);
                    binfile.Close();
                    return t;
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (FileNotFoundException)
                { throw; }
                finally
                {

                }
            }
            /// <summary>
            /// 读取文本
            /// </summary>
            /// <param name="targetPath"></param>
            /// <returns></returns>
            public string ReadCommon(string targetPath)
            {
                if (File.Exists(targetPath))
                {
                    //using (StreamReader sr = File.OpenText(targetPath)) // 读中文将乱码
                    string bcStr = "";
                    using (StreamReader sr = new StreamReader(targetPath, UnicodeEncoding.GetEncoding("GB2312"))) // 解决中文乱码问题
                    {
                        string readStr;
                        while ((readStr = sr.ReadLine()) != null)
                        {
                            bcStr = bcStr + readStr;
                        }
                        sr.Close();
                    }
                    return bcStr;
                }
                else
                {
                    Console.WriteLine("Message , 没有找到文件{0}", targetPath);
                    return string.Empty;
                }
            }
        }
    }
}
