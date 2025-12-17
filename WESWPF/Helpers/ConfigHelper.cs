using WES.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace WES.Helpers
{
    public class ConfigHelper
    {
        /// <summary>
        /// 已知app.config配置中的key值，获取对应的value值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetSingleConfig(string key, out bool result)
        {
            if (string.IsNullOrEmpty(key))
            {
                result = false;
                return "key不能为空";
            }

            try
            {
                // 获取指定键值对应的value
                string value = ConfigurationManager.AppSettings[key];
                result = true;
                return $"获取{key}配置{value}成功"; //返回value
            }
            catch (Exception ex)
            {
                result = false;
                return $"获取{key}配置异常:{ex.Message}";
            }
        }


        /// <summary>
        /// 已知app.config配置中的key值，新增or更新对应的value值并保存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string SaveSingleConfig(string key, string value, out bool result)
        {
            if (string.IsNullOrEmpty(key))
            {
                result = false;
                return "key不能为空";
            }
            try
            {
                // 加载App.config
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // 获取AppSettings节点
                KeyValueConfigurationCollection appSettings = config.AppSettings.Settings;

                if (appSettings[key] != null)
                {
                    // 如果key已经存在，则更新value
                    appSettings[key].Value = value;
                }
                else
                {
                    // 如果key不存在，则新增key-value对
                    appSettings.Add(key, value);
                }

                // 保存更改到App.config
                config.Save(ConfigurationSaveMode.Modified);

                // 刷新AppSettings以加载新修改的配置信息
                ConfigurationManager.RefreshSection("appSettings");
                result = true;
                return $"key:{key},value:{value}保存成功"; // 返回保存后的key和value值
            }
            catch (Exception ex)
            {
                result = false;
                return $"key:{key},value:{value}保存失败:{ex.Message}";
            }
        }


        /// <summary>
        /// 从app.config获取参数设置，并填充到传入的模型类型实例中。
        /// </summary>
        /// <typeparam name="T">目标模型类型</typeparam>
        /// <returns>包含App.config中配置值的模型实例</returns>
        public static T GetAppConfig<T>(out bool result) where T : new()
        {
            // 创建一个模型实例
            T instance = new T();

            // 获取所有public属性
            System.Reflection.PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (System.Reflection.PropertyInfo property in properties)
            {
                // 从App.config中找到与属性名称匹配的键
                string configValue = ConfigurationManager.AppSettings[property.Name];

                // 如果找到值并且类型匹配，则赋值
                if (configValue != null)
                {
                    try
                    {
                        // 将配置值转换为目标属性的类型
                        object convertedValue = Convert.ChangeType(configValue, property.PropertyType);

                        // 将值设置到模型属性中
                        property.SetValue(instance, convertedValue);
                    }
                    catch
                    {
                        MessageBox.Show(property.Name);
                        instance = default;
                        result = false;
                        return instance;
                    }
                }
                else
                {
                    MessageBox.Show(property.Name);
                    instance = default;
                    result = false;
                    return instance;
                }
            }
            result = true;
            return instance;
        }


        /// <summary>
        /// 根据传入的实例添加(没有配置)保存或更新配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string SaveORUpdateAppConfig<T>(T t, out bool result)
        {
            if (t == null)
            {
                result = false;
                return "配置对象不能为空";
            }

            try
            {
                // 加载App.config
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // 获取AppSettings节点
                KeyValueConfigurationCollection appSettings = config.AppSettings.Settings;

                // 遍历传入对象的所有可设置的公共属性
                foreach (System.Reflection.PropertyInfo property in typeof(T).GetProperties().Where(p => p.CanRead))
                {
                    string key = property.Name; // 属性名称作为配置Key
                    object value = property.GetValue(t, null); // 获取属性值作为配置Value

                    // 如果key已存在，则更新value，否则新增
                    if (appSettings[key] != null)
                    {
                        appSettings[key].Value = value?.ToString() ?? string.Empty;
                    }
                    else
                    {
                        appSettings.Add(key, value?.ToString() ?? string.Empty);
                    }
                }

                // 保存更改到App.config
                config.Save(ConfigurationSaveMode.Modified);

                // 刷新AppSettings以加载新修改的配置信息
                ConfigurationManager.RefreshSection("appSettings");

                result = true;
                return "配置保存成功"; // 保存成功
            }
            catch (Exception ex)
            {
                result = false;
                return $"配置保存失败:{ex}"; // 保存失败
            }
        }


        /// <summary>
        /// 判断App.config中的配置是否完整（每个模型属性对应的key都存在）
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>返回一个布尔值，表示是否完整，以及缺失的属性名称集合</returns>
        public static (bool IsComplete, List<string> MissingKeys) CheckModelConfigIntegrity<T>()
        {
            // 获取目标模型的所有公共属性
            System.Reflection.PropertyInfo[] properties = typeof(T).GetProperties();

            if (properties.Length == 0)
            {
                throw new ArgumentException($"{typeof(T).Name} 没有可检测的公共属性", nameof(T));
            }

            // 存放缺失的Key
            List<string> missingKeys = new List<string>();

            try
            {
                // 遍历属性，检测对应的Key是否存在于AppSettings中
                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    string key = property.Name;

                    // 检查键是否存在于AppSettings，注意值为空也会被认为是缺失
                    if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]))
                    {
                        missingKeys.Add(key); // 缺失的键加入列表
                    }
                }

                // 返回集合是否为空（即是否完整）和缺失的Key列表
                return (missingKeys.Count == 0, missingKeys);
            }
            catch (Exception ex)
            {
                // 可以选择记录日志或抛出异常
                //Console.WriteLine($"检查模型配置完整性时发生错误：{ex.Message}");
                throw ex;
            }
        }


        /// <summary>
        /// 使用XML加载配置到模型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configFilePath"></param>
        /// <returns></returns>
        public static T LoadConfigByXML<T>(string configFilePath) where T : new()
        {
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"配置文件未找到: {configFilePath}");
            }

            // 创建一个配置模型实例
            T model = new T();

            // 加载 XML 文档
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFilePath);

            // 定位到 appSettings 节点
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");
            if (appSettingsNode == null)
            {
                throw new InvalidOperationException("配置文件中未找到 <appSettings> 节点");
            }

            // 获取所有公共属性并尝试将 XML 配置值映射到模型
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                string key = property.Name;

                // 定位到 key 对应的 XML 节点
                XmlNode settingNode = appSettingsNode.SelectSingleNode($"add[@key='{key}']");
                if (settingNode != null && settingNode.Attributes?["value"] != null)
                {
                    // 转换 XML 值到对应数据类型
                    string value = settingNode.Attributes["value"].Value;
                    object convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(model, convertedValue); // 将值赋给属性
                }
            }

            return model; // 返回加载的配置模型
        }


        /// <summary>
        /// 使用XML保存配置到XML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="configFilePath"></param>
        public static void SaveConfigByXML<T>(T model, string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"配置文件未找到: {configFilePath}");
            }

            // 加载 XML 文档
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFilePath);

            // 定位到 appSettings 节点
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");
            if (appSettingsNode == null)
            {
                throw new InvalidOperationException("配置文件中未找到 <appSettings> 节点");
            }

            // 获取所有公共属性并保存
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                string key = property.Name;
                object value = property.GetValue(model, null);

                // 查找现有 key 的节点
                XmlNode settingNode = appSettingsNode.SelectSingleNode($"add[@key='{key}']");
                if (settingNode != null)
                {
                    // 更新现有的值
                    if (settingNode.Attributes["value"] != null)
                    {
                        settingNode.Attributes["value"].Value = value?.ToString() ?? string.Empty;
                    }
                }
                else
                {
                    // 如果 key 不存在，则创建新的 <add> 节点
                    XmlElement newElement = xmlDoc.CreateElement("add");
                    newElement.SetAttribute("key", key);
                    newElement.SetAttribute("value", value?.ToString() ?? string.Empty);
                    appSettingsNode.AppendChild(newElement);
                }
            }

            // 保存 XML 文档
            xmlDoc.Save(configFilePath);
        }
    }
}
