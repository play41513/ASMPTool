using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ASMPTool.Model.TestResultModel;

namespace ASMPTool.Model
{
    public class DataTableModel
    {
        private static DataTableModel? _instance;
        private DataTable _dataTable;

        private DataTableModel()
        {
            _dataTable = new DataTable();
        }

        public static DataTableModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataTableModel();
                    _instance.DataTable.Columns.Add(" ", typeof(string));
                    _instance.DataTable.Columns.Add("Test Item", typeof(string));
                    _instance.DataTable.Columns.Add("Test Step", typeof(string));
                    _instance.DataTable.Columns.Add("Result", typeof(string));
                    _instance.DataTable.Columns.Add("Spend Time", typeof(string));
                    _instance.DataTable.Columns.Add("Detail", typeof(string));


                    // 訂閱 TestResultModel 類別中的 PropertyChanged 事件
                    TestResultModel.Instance.PropertyChangedEx += _instance.Instance_PropertyChanged;

                }
                return _instance;
            }
        }

        public DataTable DataTable
        {
            get { return _dataTable; }
            set { _dataTable = value; }
        }
        private void Instance_PropertyChanged(object? sender, PropertyChangedEventArgsEx e)
        {
            // 更新 DataTable 中的資料

            if (e.PropertyName == "TableContent")
            {
                try
                {
                    var row = _dataTable.Rows[e.Index];
                    row["Result"] = " " + TestResultModel.Instance.TableContent[e.Index].Result;
                    row["Spend Time"] = TestResultModel.Instance.TableContent[e.Index].SpendTime == 0 ? "" : " " + TestResultModel.Instance.TableContent[e.Index].SpendTime;
                    row["Detail"] = " " + TestResultModel.Instance.TableContent[e.Index].Detail;
                }
                catch { }

            }
            else if (e.PropertyName == "TableContentResult")
            {
                try
                {
                    _dataTable.Rows[e.Index]["Result"] = " " + TestResultModel.Instance.TableContent[e.Index].Result;
                }
                catch { }
            }
            else if (e.PropertyName == "Clear")
            {
                try
                {
                    TestResultModel.Instance.TableContent.Clear();
                    foreach (DataRow row in _dataTable.Rows)
                    {
                        row["Result"] = DBNull.Value;
                        row["Spend Time"] = DBNull.Value;
                        row["Detail"] = DBNull.Value;
                    }
                }
                catch { }
            }
        }
    }
}
