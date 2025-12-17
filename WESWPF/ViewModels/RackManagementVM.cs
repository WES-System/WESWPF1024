using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WES.Commons;
using WES.Helpers;
using WES.Models;
using WES.Views;

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

        #region 分页属性
        // 分页大小选项
        public List<int> PageSizes { get; set; } = new List<int> { 10, 20, 50, 100, 500, 1000 };

        private int _currentPageSize = 20;
        public int CurrentPageSize
        {
            get => _currentPageSize;
            set
            {
                if (_currentPageSize == value) return;
                _currentPageSize = value;
                DoNotify();
                CurrentPageIndex = 1; // 重置到第一页
                _ = GetSampleData();
            }
        }

        private int _currentPageIndex = 1;
        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set
            {
                if (_currentPageIndex == value) return;
                _currentPageIndex = value;
                DoNotify();
                UpdatePageButtons();
            }
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                DoNotify();
                TotalPageCount = (int)Math.Ceiling((double)value / CurrentPageSize);
            }
        }

        private int _totalPageCount;
        public int TotalPageCount
        {
            get => _totalPageCount;
            set
            {
                _totalPageCount = value;
                DoNotify();
                UpdatePageButtons();
            }
        }

        public string PageInfo => $"第 {CurrentPageIndex}/{TotalPageCount} 页，共 {TotalCount} 条记录";

        // 分页按钮状态
        private bool _canFirstPage;
        public bool CanFirstPage
        {
            get => _canFirstPage;
            set
            {
                _canFirstPage = value;
                DoNotify();
            }
        }

        private bool _canPreviousPage;
        public bool CanPreviousPage
        {
            get => _canPreviousPage;
            set
            {
                _canPreviousPage = value;
                DoNotify();
            }
        }

        private bool _canNextPage;
        public bool CanNextPage
        {
            get => _canNextPage;
            set
            {
                _canNextPage = value;
                DoNotify();
            }
        }

        private bool _canLastPage;
        public bool CanLastPage
        {
            get => _canLastPage;
            set
            {
                _canLastPage = value;
                DoNotify();
            }
        }
        #endregion

        #region 分页命令
        private CommandBase _firstPageCommand;
        public CommandBase FirstPageCommand
        {
            get
            {
                return _firstPageCommand ?? (_firstPageCommand = new CommandBase
                {
                    DoExcute = obj =>
                    {
                        CurrentPageIndex = 1;
                        _ = GetSampleData();
                    },
                    DoCanExecute = obj => CanFirstPage
                });
            }
        }

        private CommandBase _previousPageCommand;
        public CommandBase PreviousPageCommand
        {
            get
            {
                return _previousPageCommand ?? (_previousPageCommand = new CommandBase
                {
                    DoExcute = obj =>
                    {
                        CurrentPageIndex--;
                        _ = GetSampleData();
                    },
                    DoCanExecute = obj => CanPreviousPage
                });
            }
        }

        private CommandBase _nextPageCommand;
        public CommandBase NextPageCommand
        {
            get
            {
                return _nextPageCommand ?? (_nextPageCommand = new CommandBase
                {
                    DoExcute = obj =>
                    {
                        CurrentPageIndex++;
                        _ = GetSampleData();
                    },
                    DoCanExecute = obj => CanNextPage
                });
            }
        }

        private CommandBase _lastPageCommand;
        public CommandBase LastPageCommand
        {
            get
            {
                return _lastPageCommand ?? (_lastPageCommand = new CommandBase
                {
                    DoExcute = obj =>
                    {
                        CurrentPageIndex = TotalPageCount;
                        _ = GetSampleData();
                    },
                    DoCanExecute = obj => CanLastPage
                });
            }
        }
        #endregion

        //public ObservableCollection<RackCellModel> Rackcells { get; set; } = new ObservableCollection<RackCellModel>();//架位集合
        public RangeObservableCollection<RackCellModel> Rackcells { get; set; }
            = new RangeObservableCollection<RackCellModel>();

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
            var editView = new RackEditView();
            var editVm = new RackEditVM(); 
            editVm.CloseAction = () =>
            {
                editView.Close();
                CurrentPageIndex = 1;
                _ = GetSampleData();
            };
            editView.DataContext = editVm;
            editView.ShowDialog();
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
            var rack = obj as RackCellModel;
            if (rack == null) return;

            // 创建编辑窗口并显示
            var editView = new RackEditView();
            var editVm = new RackEditVM(rack);
            editVm.CloseAction = () =>
            {
                editView.Close();
                // 刷新数据
                _ = GetSampleData();
            };
            editView.DataContext = editVm;
            editView.ShowDialog();
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

        private CommandBase selectCommand;
        public CommandBase SelectCommand
        {
            get
            {
                if (selectCommand == null)
                {
                    selectCommand = new CommandBase()
                    {
                        DoExcute = Select,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return selectCommand;
            }
        }

        private async void Select(object obj)
        {
            try
            {
                // 重置到第一页
                CurrentPageIndex = 1;

                // 构建查询条件
                Expression<Func<RackCellModel, bool>> where = null;

                // 货架编号查询（模糊匹配）
                if (!string.IsNullOrWhiteSpace(Rack.RackCellCode))
                {
                    where = SQLHelper.Instance.AddWhere(where, r => r.RackCellCode.Contains(Rack.RackCellCode.Trim()));
                }

                // 行索引查询（精确匹配，仅当输入有效数字时）
                if (int.TryParse(Rack.RackCellRowIndex.ToString(), out int rowIndex) && rowIndex > 0)
                {
                    where = SQLHelper.Instance.AddWhere(where, r => r.RackCellRowIndex == rowIndex);
                }

                // 列索引查询（精确匹配，仅当输入有效数字时）
                if (int.TryParse(Rack.RackCellColumnIndex.ToString(), out int colIndex) && colIndex > 0)
                {
                    where = SQLHelper.Instance.AddWhere(where, r => r.RackCellColumnIndex == colIndex);
                }

                // 类型查询（模糊匹配）
                if (!string.IsNullOrWhiteSpace(Rack.RackCellType))
                {
                    where = SQLHelper.Instance.AddWhere(where, r => r.RackCellType.Contains(Rack.RackCellType.Trim()));
                }

                // 执行分页查询
                var pageResult = await SQLHelper.Instance.SelectPageWithResultAsync<RackCellModel>(
                    pageIndex: CurrentPageIndex,
                    pageSize: CurrentPageSize,
                    where: where,
                    orderBy: (r => r.ID) // 按ID升序排序
                );

                if (pageResult.IsSuccess)
                {
                    Rackcells.ReplaceRange(pageResult.Any1.List);
                    TotalCount = (int)pageResult.Any1.TotalCount;
                }
                else
                {
                    MessageBox.Show($"查询失败：{pageResult.Message}");
                    Rackcells.Clear();
                    TotalCount = 0;
                }

                UpdatePageButtons();
            }
            catch (Exception ex)
            {
                LogHelper.Error("查询货架数据异常", ex);
                MessageBox.Show($"查询时发生错误：{ex.Message}");
            }
        }


        private void Refresh(object obj)
        {
            CurrentPageIndex = 1;
            _ = GetSampleData();
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
                    await GetSampleData();
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
            _ = GetSampleData();
        }

        private async Task GetSampleData()
        {
            try
            {
                var pageResult = await SQLHelper.Instance.SelectPageWithResultAsync<RackCellModel>(
                    pageIndex: CurrentPageIndex,
                    pageSize: CurrentPageSize,
                    orderBy: x => x.ID
                );

                if (pageResult.IsSuccess)
                {
                    Rackcells.ReplaceRange(pageResult.Any1.List);
                    TotalCount = (int)pageResult.Any1.TotalCount;
                }
                else
                {
                    MessageBox.Show($"加载架位数据失败：{pageResult.Message}");
                    Rackcells.Clear();
                    TotalCount = 0;
                }

                UpdatePageButtons();
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"加载架位数据异常：{ex.Message}");
                MessageBox.Show($"加载数据时发生异常：{ex.Message}");
                Rackcells.Clear();
                TotalCount = 0;
                UpdatePageButtons();
            }
        }


        private void UpdatePageButtons()
        {
            CanFirstPage = CurrentPageIndex > 1;
            CanPreviousPage = CurrentPageIndex > 1;
            CanNextPage = CurrentPageIndex < TotalPageCount;
            CanLastPage = CurrentPageIndex < TotalPageCount;
            DoNotify("PageInfo");
            FirstPageCommand.RaiseCanExecuteChanged();
            PreviousPageCommand.RaiseCanExecuteChanged();
            NextPageCommand.RaiseCanExecuteChanged();
            LastPageCommand.RaiseCanExecuteChanged();
        }
    }
}
