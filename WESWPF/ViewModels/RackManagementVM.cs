using WES.Commons;
using WES.Helpers;
using WES.Models;
using WES.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WES.ViewModels
{
    public class RackManagementVM : NotifyBase
    {
        private RackCellModel rack = new RackCellModel();
        public RackCellModel Rack
        {
            get => rack;
            set
            {
                rack = value;
                DoNotify();
            }
        }

        public ObservableCollection<RackCellModel> Rackcells { get; set; } = new ObservableCollection<RackCellModel>();//架位集合

        private CommandBase addRackCommand;
        public CommandBase AddRackCommand
        {
            get
            {
                if (addRackCommand == null)
                {
                    addRackCommand = new CommandBase()
                    {
                        DoExcute = AddRack,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return addRackCommand;
            }
        }

        private async void AddRack(object obj)
        {
            try
            {
                List<RackCellModel> racks = await SQLHelper.Instance.SelectAsync<RackCellModel>(c => c.Where(rc => rc.RackCellCode == Rack.RackCellCode));
                if (racks.Count > 0)//更新
                {
                    await SQLHelper.Instance.UpdateAsync(Rack);
                    MessageBox.Show($"架位{Rack.RackCellCode}信息修改成功");
                }
                else
                {
                    await SQLHelper.Instance.InsertAsync(Rack);
                    Rackcells.Add(Rack);
                    MessageBox.Show($"架位{Rack.RackCellCode}信息添加成功");
                }
                Rack = new RackCellModel();
            }
            catch (Exception e)
            {
                MessageBox.Show($"添加/修改架位信息异常:{e.Message}");
            }
        }

        private CommandBase editRackCommand;
        public CommandBase EditRackCommand
        {
            get
            {
                if (editRackCommand == null)
                {
                    editRackCommand = new CommandBase()
                    {
                        DoExcute = EditRack,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return editRackCommand;
            }
        }

        private void EditRack(object obj)
        {
            Rack = (RackCellModel)obj;
            MessageBox.Show("修改完成后请注意保存");
        }

        private CommandBase deleteRackCommand;
        public CommandBase DeleteRackCommand
        {
            get
            {
                if (deleteRackCommand == null)
                {
                    deleteRackCommand = new CommandBase()
                    {
                        DoExcute = DeleteRack,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return deleteRackCommand;
            }
        }

        private async void DeleteRack(object obj)
        {
            try
            {
                if (!(MessageBox.Show($"确认删除架位{((RackCellModel)obj).RackCellCode}") == MessageBoxResult.OK))
                {
                    return;
                }
                await SQLHelper.Instance.DeleteAsync<RackCellModel>(c => c.Where(o => o.ID == ((RackCellModel)obj).ID));
                Rackcells.Remove((RackCellModel)obj);
                MessageBox.Show($"架位{((RackCellModel)obj).RackCellCode}删除成功");
            }
            catch (Exception e)
            {
                MessageBox.Show($"删除架位信息异常:{e.Message}");
            }
        }

        private CommandBase reFreshCommand;
        public CommandBase ReFreshCommand
        {
            get
            {
                if (reFreshCommand == null)
                {
                    reFreshCommand = new CommandBase()
                    {
                        DoExcute = Refresh,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return reFreshCommand;
            }
        }

        private void Refresh(object obj)
        {
            GetSampleData();
            MessageBox.Show("架位信息已刷新");
        }

        private CommandBase changeLineCommand;
        public CommandBase ChangeLineCommand
        {
            get
            {
                if (changeLineCommand == null)
                {
                    changeLineCommand = new CommandBase()
                    {
                        DoExcute = ChangeLine,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return changeLineCommand;
            }
        }

        private async void ChangeLine(object obj)
        {
            try
            {
                ShowWindow show = new ShowWindow();
                if (show.ShowDialog() == true)
                {
                    string line = show.Line;//M00003P610601
                    List<RackCellModel> racks = SQLHelper.Instance.SelectAsync<RackCellModel>().Result;
                    foreach (RackCellModel rack in racks)
                    {
                        rack.RackCellCode = rack.RackCellCode.Substring(0, 5) + line + rack.RackCellCode.Substring(6);
                        await SQLHelper.Instance.UpdateAsync(rack);
                    }
                    GetSampleData();
                    MessageBox.Show("修改成功");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"一键改线异常:{e.Message}");
            }
        }

        public RackManagementVM()
        {
            GetSampleData();
        }

        private void GetSampleData()
        {
            try
            {
                Rackcells.Clear();
                foreach (RackCellModel item in SQLHelper.Instance.SelectAsync<RackCellModel>().Result)
                {
                    Rackcells.Add(item);
                }
            }
            catch (Exception e)
            {
                LogHelper.Debug($"初始化数据异常：{e.Message}");
            }
        }
    }
}
