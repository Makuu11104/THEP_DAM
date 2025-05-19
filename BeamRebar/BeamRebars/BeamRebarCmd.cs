using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BeamRebar.BeamRebars.ViewModels;
using BeamRebar.BeamRebars.Views;
using Nice3point.Revit.Toolkit.External;

namespace BeamRebar.BeamRebars
{
    [Transaction(TransactionMode.Manual)] // Thiết lập giao dịch thủ công
    public class BeamRebarCmd : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                // Khởi tạo ViewModel với tài liệu Revit
                var viewModel = new BeamRebarViewModel(Document, UiDocument);

                // Tạo và hiển thị cửa sổ
                var view = new BeamRebarView() { DataContext = viewModel };
                viewModel.BeamRebarView = view;

                // Hiển thị dialog (sửa từ showDialog() thành ShowDialog())
                view.ShowDialog();
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi cho người dùng
                TaskDialog.Show("Lỗi", $"Không thể thực hiện lệnh: {ex.Message}");
            }
        }
    }
}