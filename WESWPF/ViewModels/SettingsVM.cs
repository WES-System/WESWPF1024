using WES.Commons;
using WES.Helpers;
using WES.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.ViewModels
{
    public class SettingsVM : NotifyBase
    {
        private CommandBase refreshConfigCommand;
        public CommandBase RefreshConfigCommand
        {
            get
            {
                if (refreshConfigCommand == null)
                {
                    refreshConfigCommand = new CommandBase()
                    {
                        DoExcute = RefreshConfig,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return refreshConfigCommand;
            }
        }

        private CommandBase saveConfigCommand;
        public CommandBase SaveConfigCommand
        {
            get
            {
                if (saveConfigCommand == null)
                {
                    saveConfigCommand = new CommandBase()
                    {
                        DoExcute = SaveConfig,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return saveConfigCommand;
            }
        }

        private ConfigModel config;
        public ConfigModel Config
        {
            get => config;
            set
            {
                config = value;
                DoNotify();
            }
        }

        /// <summary>
        /// 场景选项绑定到ComboBox（数字键和显示名称的映射）
        /// </summary>
        public ObservableCollection<SceneOption> SceneOptions { get; set; } = new ObservableCollection<SceneOption>
        {
            new SceneOption { SceneCode = "0", SceneName = "码垛入库" },
            new SceneOption { SceneCode = "1", SceneName = "理货" },
            new SceneOption { SceneCode = "2", SceneName = "自动出库" },
            new SceneOption { SceneCode = "3", SceneName = "手动出库" }
        };

        public SettingsVM()
        {
            RefreshConfig(null);
        }

        public void RefreshConfig(object obj)
        {
            //Config = ConfigHelper.GetAppConfig<ConfigModel>(out bool result);
            Config = ConfigHelper.LoadConfigByXML<ConfigModel>("WES.exe.config");
            GlobalParams.config = Config;
        }

        private void SaveConfig(object obj)
        {
            //ConfigHelper.SaveORUpdateAppConfig(Config,out bool result);
            ConfigHelper.SaveConfigByXML(Config, "WES.exe.config");
            RefreshConfig(null);
        }
    }

    public class SceneOption
    {
        public string SceneCode { get; set; }   // 数字键，比如 "0", "1", "2"...
        public string SceneName { get; set; } // 显示的名字，比如 "码垛入库", "理货"...
        public override string ToString() => SceneName; // 用于显示 "Name" 在ComboBox中
    }

}
