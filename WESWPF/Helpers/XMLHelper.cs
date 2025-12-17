using WES.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WES.Helpers
{
    /// <summary>
    /// xml帮助类
    /// </summary>
    public class XMLHelper
    {
        #region 字段属性事件
        /// <summary>
        /// XML日志事件 输出信息
        /// </summary>
        public event Action<string> XMLEvent;

        private int _idCounter;

        private List<int> _availableIds = new List<int>();

        public string XmlFilePath { get; set; }

        public XMLHelper(string xmlFilePath)
        {
            XmlFilePath = xmlFilePath;

        }
        #endregion

        #region 操作方法(增删改查)
        /// <summary>
        /// 将xml模型对象添加到xml节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public bool AddXMLModels<T>(List<T> objects) where T : XMLModelBase, new()
        {
            try
            {
                CheckFile();
                InitializeIdManagement<T>();
                XDocument document = XDocument.Load(XmlFilePath);
                XElement rootElement = document.Element("Root");
                foreach (T obj in objects)
                {
                    //ObjToElement(obj, out XElement childElement);
                    XElement childElement = ConvertObjectToElement(obj, typeof(T).Name);
                    rootElement.Add(childElement);
                    #region 注释代码
                    //foreach (PropertyInfo prop in typeof(T).GetProperties())
                    //{
                    //    if (prop.PropertyType.IsArray)
                    //    {
                    //        Array array = (Array)prop.GetValue(obj);
                    //        if (array != null)
                    //        {
                    //            XElement arrayElement = new XElement(prop.Name);
                    //            foreach (object item in array)
                    //            {
                    //                arrayElement.Add(new XElement("Item", item));
                    //            }
                    //            element.Add(arrayElement);
                    //        }
                    //    }
                    //    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                    //    {
                    //        System.Collections.IEnumerable collection = (System.Collections.IEnumerable)prop.GetValue(obj);
                    //        if (collection != null)
                    //        {
                    //            XElement collectionElement = new XElement(prop.Name);
                    //            foreach (object item in collection)
                    //            {
                    //                collectionElement.Add(new XElement("Item", item));
                    //            }
                    //            element.Add(collectionElement);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        string value = prop.GetValue(obj)?.ToString() ?? string.Empty;
                    //        if (prop.Name == "ID")
                    //        {
                    //            element.Add(new XAttribute("ID", GetID().ToString()));
                    //            continue;
                    //        }
                    //        element.Add(new XElement(prop.Name, value));
                    //    }
                    //}
                    //rootElement.Add(element);
                    #endregion
                }
                // 对根元素下的每一类对象进行排序
                foreach (XName elementType in rootElement.Elements().Select(e => e.Name).Distinct())
                {
                    List<XElement> elementsOfType = rootElement.Elements(elementType).OrderBy(e => (int)e.Attribute("ID")).ToList();
                    rootElement.Elements(elementType).Remove();
                    rootElement.Add(elementsOfType);
                }
                document.Save(XmlFilePath);
                return true;
            }
            catch (Exception e)
            {
                XMLEvent?.Invoke($"新增{typeof(T).Name}类型XML节点异常：" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 根据条件查询XML节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<T> QueryXMLModels<T>(Func<T, bool> predicate = null) where T : XMLModelBase, new()
        {
            List<T> results = new List<T>();
            try
            {
                CheckFile();
                XDocument document = XDocument.Load(XmlFilePath);
                IEnumerable<XElement> elements = document.Descendants(typeof(T).Name);
                if (elements.Count() == 0)
                {
                    return results;
                }
                foreach (XElement element in elements)
                {
                    T obj = ConvertElementToObject<T>(element);
                    if (predicate == null || predicate(obj))
                    {
                        results.Add(obj);
                    }
                }
            }
            catch (Exception e)
            {
                XMLEvent?.Invoke($"查询{typeof(T).Name}类型XML节点异常：" + e.Message);
            }
            return results;
        }

        /// <summary>
        /// 根据条件删除XML节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool DeleteXMLModels<T>(Func<T, bool> predicate = null) where T : XMLModelBase, new()
        {
            bool hasChanges = false;
            try
            {
                CheckFile();
                XDocument document = XDocument.Load(XmlFilePath);
                XElement rootElement = document.Element("Root");
                if (rootElement == null)
                {
                    XMLEvent?.Invoke($"未找到根节点 Root");
                    return false;
                }
                IEnumerable<XElement> elementsToDelete;
                if (predicate == null)
                {
                    elementsToDelete = rootElement.Elements(typeof(T).Name).ToList();
                }
                else
                {
                    elementsToDelete = rootElement.Elements(typeof(T).Name)
                                                  .Where(element => predicate(ConvertElementToObject<T>(element)))
                                                  .ToList();
                }
                foreach (XElement element in elementsToDelete)
                {
                    element.Remove();
                    hasChanges = true;
                }
                if (hasChanges)
                {
                    document.Save(XmlFilePath);
                    return true;
                }
            }
            catch (Exception e)
            {
                XMLEvent?.Invoke($"删除{typeof(T).Name}类型XML节点异常：" + e.Message);
            }
            return false;
        }

        /// <summary>
        /// 根据条件更新XML节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        public bool UpdateXMLModel<T>(Func<T, bool> predicate, Action<T> updateAction) where T : XMLModelBase, new()
        {
            bool hasChanges = false;
            try
            {
                CheckFile();
                XDocument document = XDocument.Load(XmlFilePath);
                XElement rootElement = document.Element("Root");
                if (rootElement == null)
                {
                    XMLEvent?.Invoke($"未找到根节点 Root");
                    return false;
                }
                List<XElement> elementsToUpdate = rootElement.Elements(typeof(T).Name)
                                                             .Where(element => predicate(ConvertElementToObject<T>(element)))
                                                             .ToList();
                foreach (XElement element in elementsToUpdate)
                {
                    T obj = ConvertElementToObject<T>(element);
                    updateAction(obj);
                    #region 注释代码
                    //foreach (PropertyInfo prop in typeof(T).GetProperties())
                    //{
                    //    if (prop.PropertyType.IsArray)
                    //    {
                    //        Array array = (Array)prop.GetValue(obj);
                    //        if (array != null)
                    //        {
                    //            XElement arrayElement = new XElement(prop.Name);
                    //            foreach (object item in array)
                    //            {
                    //                arrayElement.Add(new XElement("Item", item));
                    //            }
                    //            element.Element(prop.Name)?.Remove();
                    //            element.Add(arrayElement);
                    //        }
                    //    }
                    //    else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                    //    {
                    //        IEnumerable collection = (IEnumerable)prop.GetValue(obj);
                    //        if (collection != null)
                    //        {
                    //            XElement collectionElement = new XElement(prop.Name);
                    //            foreach (object item in collection)
                    //            {
                    //                collectionElement.Add(new XElement("Item", item));
                    //            }
                    //            element.Element(prop.Name)?.Remove();
                    //            element.Add(collectionElement);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (prop.Name == "ID")
                    //        {
                    //            continue;
                    //        }
                    //        else
                    //        {
                    //            XElement propElement = element.Element(prop.Name);
                    //            if (propElement != null)
                    //            {
                    //                propElement.Value = prop.GetValue(obj)?.ToString();
                    //            }
                    //            else
                    //            {
                    //                element.Add(new XElement(prop.Name, prop.GetValue(obj)?.ToString()));
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion
                    XElement newElement = ConvertObjectToElement(obj, obj.GetType().Name, obj.ID);
                    XElement oldElement = rootElement.Element(element.Name);
                    if (oldElement != null)
                    {
                        oldElement.ReplaceWith(newElement);
                    }
                    else
                    {
                        rootElement.Add(newElement);
                    }
                    hasChanges = true;
                }
                if (hasChanges)
                {
                    document.Save(XmlFilePath);
                    return true;
                }
            }
            catch (Exception e)
            {
                XMLEvent?.Invoke($"更新{typeof(T).Name}类型XML节点异常：" + e.Message);
            }
            return false;
        }
        #endregion

        #region xml文件操作辅助方法
        /// <summary>
        /// 验证文件夹是否存在 不存在创建文件夹
        /// </summary>
        private void CheckDirectory()
        {
            string directory = Path.GetDirectoryName(XmlFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// 验证xml文件是否存在 不存在生成一个xml文件，创建默认根节点
        /// </summary>
        private void CheckFile()
        {
            CheckDirectory();
            if (!File.Exists(XmlFilePath))
            {
                XDocument document = new XDocument(new XElement("Root"));
                document.Save(XmlFilePath);
            }
        }

        /// <summary>
        /// 初始化缺失ID列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void InitializeIdManagement<T>()
        {
            CheckFile();
            XDocument document = XDocument.Load(XmlFilePath);
            List<int> idList = document.Descendants(typeof(T).Name)
                .Select(e =>
                {
                    XAttribute idAttribute = e.Attribute("ID");
                    if (idAttribute != null && int.TryParse(idAttribute.Value, out int id))
                    {
                        return (int?)id;
                    }
                    return null;
                })
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();
            int maxID = idList.DefaultIfEmpty(0).Max();
            _availableIds = Enumerable.Range(1, maxID).Except(idList).ToList();
            _idCounter = maxID + 1;
        }

        /// <summary>
        /// 初始化ID计数器，自动生成唯一ID
        /// </summary>
        private int GetID(int i = 0)
        {
            if (i != 0)
            {
                return i;
            }
            if (_availableIds.Count > 0)
            {
                int id = _availableIds[0];
                _availableIds.RemoveAt(0);
                return id;
            }
            else
            {
                return _idCounter++;
            }
        }

        /// <summary>
        /// 将对象转换为XElement（递归实现）
        /// </summary>
        private XElement ConvertObjectToElement(object obj, string elementName = null, int ID = 0)
        {
            XElement element = new XElement(elementName ?? obj.GetType().Name);
            if (IsBasicType(obj.GetType()))
            {
                element.Value = obj.ToString();
                return element;
            }
            foreach (PropertyInfo prop in obj.GetType().GetProperties().Where(p => p.CanRead))
            {
                if (prop.Name == "ID")
                {
                    element.Add(new XAttribute(prop.Name, GetID(ID)));
                    continue;
                }
                object value = prop.GetValue(obj);
                if (value == null)
                {
                    continue;
                }
                if (IsBasicType(prop.PropertyType))
                {
                    element.Add(new XElement(prop.Name, value));
                }
                else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                {
                    XElement collectionElement = new XElement(prop.Name);
                    foreach (object item in (IEnumerable)value)
                    {
                        if (item == null)
                        {
                            continue;
                        }
                        collectionElement.Add(ConvertObjectToElement(item, item.GetType().Name));
                    }
                    element.Add(collectionElement);
                }
                else
                {
                    element.Add(ConvertObjectToElement(value, prop.Name));
                }
            }
            return element;
        }

        /// <summary>
        /// 将XElement转换为对象（递归实现）
        /// </summary>
        private T ConvertElementToObject<T>(XElement element) where T : XMLModelBase, new()
        {
            T obj = new T();
            foreach (PropertyInfo prop in typeof(T).GetProperties().Where(p => p.CanWrite))
            {
                if (prop.Name == "ID")
                {
                    XAttribute idAttrib = element.Attribute("ID");
                    if (idAttrib != null)
                    {
                        prop.SetValue(obj, int.Parse(idAttrib.Value));
                    }
                    continue;
                }
                XElement propElement = element.Element(prop.Name);
                if (propElement == null)
                {
                    continue;
                }
                if (IsBasicType(prop.PropertyType))
                {
                    prop.SetValue(obj, Convert.ChangeType(propElement.Value, prop.PropertyType));
                }
                else if (prop.PropertyType.IsArray)
                {
                    // 处理数组类型
                    Type elementType = prop.PropertyType.GetElementType();
                    List<object> items = new List<object>();
                    foreach (XElement itemElement in propElement.Elements())
                    {
                        object item = ConvertElementToObject(itemElement, elementType);
                        items.Add(item);
                    }
                    Array array = Array.CreateInstance(elementType, items.Count);
                    for (int i = 0; i < items.Count; i++)
                    {
                        array.SetValue(Convert.ChangeType(items[i], elementType), i);
                    }
                    prop.SetValue(obj, array);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                {
                    // 处理泛型集合
                    object collection = Activator.CreateInstance(prop.PropertyType);
                    MethodInfo addMethod = prop.PropertyType.GetMethod("Add");
                    Type itemType = prop.PropertyType.GetGenericArguments()[0];
                    foreach (XElement itemElement in propElement.Elements())
                    {
                        object item = ConvertElementToObject(itemElement, itemType);
                        addMethod.Invoke(collection, new[] { item });
                    }

                    prop.SetValue(obj, collection);
                }
                else
                {
                    object childObj = ConvertElementToObject(propElement, prop.PropertyType);
                    prop.SetValue(obj, childObj);
                }
            }
            return obj;
        }

        /// <summary>
        /// 将XElement转换为对象（递归实现）
        /// </summary>
        private object ConvertElementToObject(XElement element, Type type)
        {
            // 处理基础类型
            if (IsBasicType(type))
            {
                return Convert.ChangeType(element.Value, type);
            }
            // 处理数组类型
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                List<object> items = new List<object>();
                foreach (XElement itemElement in element.Elements())
                {
                    object item = ConvertElementToObject(itemElement, elementType);
                    items.Add(item);
                }
                Array array = Array.CreateInstance(elementType, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    array.SetValue(Convert.ChangeType(items[i], elementType), i);
                }
                return array;
            }
            // 处理普通对象类型
            object obj = Activator.CreateInstance(type);
            foreach (PropertyInfo prop in type.GetProperties().Where(p => p.CanWrite))
            {
                XElement propElement = element.Element(prop.Name);
                if (propElement == null) continue;

                if (IsBasicType(prop.PropertyType))
                {
                    prop.SetValue(obj, Convert.ChangeType(propElement.Value, prop.PropertyType));
                }
                else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                {
                    object collection = Activator.CreateInstance(prop.PropertyType);
                    MethodInfo addMethod = prop.PropertyType.GetMethod("Add");
                    Type itemType = prop.PropertyType.GetGenericArguments()[0];

                    foreach (XElement itemElement in propElement.Elements())
                    {
                        object item = ConvertElementToObject(itemElement, itemType);
                        addMethod.Invoke(collection, new[] { item });
                    }

                    prop.SetValue(obj, collection);
                }
                else
                {
                    object childObj = ConvertElementToObject(propElement, prop.PropertyType);
                    prop.SetValue(obj, childObj);
                }
            }
            return obj;
        }

        /// <summary>
        /// 判断属性数据类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsBasicType(Type type)
        {
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(DateTime) ||
                   type == typeof(decimal) ||
                   type.IsEnum ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
        #endregion

        #region xml文件辅助方法(初始版)
        /// <summary>
        /// 将对象转换为XElement(初始版) 不支持自定义类型
        /// </summary>
        private void ObjToElement<T>(T t, out XElement element)
        {
            // 初始化element为传入对象类型的名称
            element = new XElement(typeof(T).Name);
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            // 遍历所有属性
            foreach (PropertyInfo prop in propertyInfos)
            {
                if (prop.PropertyType.IsArray)
                {
                    Array array = (Array)prop.GetValue(t);
                    Type elementType = array.GetType().GetElementType();
                    XElement arrayElement = new XElement(prop.Name);
                    if (array != null)
                    {
                        if (elementType == typeof(string))
                        {
                            foreach (string item in array)
                            {
                                XElement arrayelement = new XElement("Item", item);
                                arrayElement.Add(arrayelement);
                            }
                        }
                    }
                    element.Add(arrayElement);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                {
                    IEnumerable collection = (IEnumerable)prop.GetValue(t);
                    XElement collectionElement = new XElement(prop.Name);
                    if (collection != null)
                    {
                        foreach (object item in collection)
                        {
                            ObjToElement(item, out XElement collectionItemElement);
                            collectionElement.Add(collectionItemElement);
                        }
                    }
                    element.Add(collectionElement);
                }
                else
                {
                    string value = prop.GetValue(t)?.ToString() ?? string.Empty;
                    if (prop.Name == "ID")
                    {
                        element.Add(new XAttribute("ID", GetID()));
                    }
                    else
                    {
                        element.Add(new XElement(prop.Name, value));
                    }
                }
            }
        }

        /// <summary>
        /// 元素转对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        //private T ConvertElementToObject<T>(XElement element) where T : XMLModelBase, new()
        //{
        //    T obj = new T();
        //    foreach (PropertyInfo prop in typeof(T).GetProperties())
        //    {
        //        if (prop.PropertyType.IsArray)
        //        {
        //            Type elementType = prop.PropertyType.GetElementType();
        //            object[] items = element.Elements(prop.Name).Elements("Item")
        //                .Select(e => Convert.ChangeType(e.Value, elementType))
        //                .ToArray();
        //            Array array = Array.CreateInstance(elementType, items.Length);
        //            Array.Copy(items, array, items.Length);
        //            prop.SetValue(obj, array);
        //        }
        //        else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
        //        {
        //            Type itemType = prop.PropertyType.GetGenericArguments()[0];
        //            MethodInfo addMethod = prop.PropertyType.GetMethod("Add");
        //            if (addMethod != null)
        //            {
        //                object collection = Activator.CreateInstance(prop.PropertyType);
        //                foreach (XElement e in element.Elements(prop.Name).Elements("Item"))
        //                {
        //                    object value = Convert.ChangeType(e.Value, itemType);
        //                    addMethod.Invoke(collection, new[] { value });
        //                }
        //                prop.SetValue(obj, collection);
        //            }
        //        }
        //        else
        //        {
        //            XAttribute xmlAttribute = element.Attribute(prop.Name);
        //            XElement xmlElement = element.Element(prop.Name);
        //            if (xmlAttribute != null && prop.CanWrite)
        //            {
        //                object value = Convert.ChangeType(xmlAttribute.Value, prop.PropertyType);
        //                prop.SetValue(obj, value);
        //            }
        //            else if (xmlElement != null && prop.CanWrite)
        //            {
        //                object value = Convert.ChangeType(xmlElement.Value, prop.PropertyType);
        //                prop.SetValue(obj, value);
        //            }
        //        }
        //    }
        //    return obj;
        //}
        #endregion
    }
}
