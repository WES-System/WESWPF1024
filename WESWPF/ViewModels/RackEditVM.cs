using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WES.Commons;
using WES.Helpers;
using WES.Models;

namespace WES.ViewModels
{
    public class RackEditVM : NotifyBase
    {
        private RackCellModel _rack;
        public RackCellModel Rack
        {
            get => _rack;
            set
            {
                _rack = value;
                DoNotify();
            }
        }
        
        public bool IsAddMode { get; set; }
        private CommandBase _saveCommand;
        public CommandBase SaveCommand
        {
            get => _saveCommand;
            set
            {
                _saveCommand = value;
                DoNotify();
            }
        }

        private CommandBase _cancelCommand;
        public CommandBase CancelCommand
        {
            get => _cancelCommand;
            set
            {
                _cancelCommand = value;
                DoNotify();
            }
        }
        public Action CloseAction { get; set; }

        public RackEditVM()
        {
            IsAddMode = true;
            Rack = new RackCellModel();
            InitCommands();
        }

        public RackEditVM(RackCellModel rack)
        {
            IsAddMode = false;
            Rack = new RackCellModel
            {
                ID = rack.ID,
                RackCellCode = rack.RackCellCode,
                RackCellRowIndex = rack.RackCellRowIndex,
                RackCellColumnIndex = rack.RackCellColumnIndex,
                RackCellType = rack.RackCellType
            };

            InitCommands();
        }

        private void InitCommands()
        {
            SaveCommand = new CommandBase
            {
                DoExcute = async (obj) =>
                {
                    try
                    {
                        if (IsAddMode)
                        {
                            // 检查编号是否已存在
                            var result = await SQLHelper.Instance.SelectWithResultAsync<RackCellModel>(
                                rc => rc.RackCellCode == Rack.RackCellCode);

                            if (result.Any1.Count > 0)
                            {
                                MessageBox.Show($"架位编号{Rack.RackCellCode}已存在，请更换");
                                return;
                            }

                            var res = await SQLHelper.Instance.InsertWithResultAsync(Rack);
                            if (res.Any1 > 0)
                            {
                                MessageBox.Show($"架位{Rack.RackCellCode}添加成功");
                            }
                            else
                            {
                                MessageBox.Show($"架位{Rack.RackCellCode}添加失败");
                                return;
                            }
                        }
                        else
                        {
                            var res = await SQLHelper.Instance.UpdateWithResultAsync(Rack);
                            if (res.Any1 > 0)
                            {

                                MessageBox.Show($"架位{Rack.RackCellCode}更新成功");
                            }
                            else
                            {
                                MessageBox.Show($"架位{Rack.RackCellCode}更新失败");
                                return;
                            }
                        }
                        CloseAction?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{(IsAddMode ? "添加" : "更新")}失败: {ex.Message}");
                    }
                },
                DoCanExecute = (obj) => { return true; }
                //!string.IsNullOrEmpty(Rack.RackCellCode) &&
                //Rack.RackCellRowIndex > 0 &&
                //Rack.RackCellColumnIndex > 0 &&
                //!string.IsNullOrEmpty(Rack.RackCellType)
            };

            CancelCommand = new CommandBase
            {
                DoExcute = (obj) => CloseAction?.Invoke(),
                DoCanExecute = (obj) => true
            };
        }
    }
}
