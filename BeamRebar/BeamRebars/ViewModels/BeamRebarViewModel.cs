using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BeamRebar.BeamRebars.Models;
using BeamRebar.BeamRebars.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace BeamRebar.BeamRebars.ViewModels
{
    public class BeamRebarViewModel : ObservableObject
    {
        private int _soLuongThepChinh = 2;
        private RebarBarType _duongKinhThepChinh;
        private int _soLuongThepGiaCuongLopTren = 2;
        private RebarBarType _duongKinhThepGiaCuongLopTren;
        private int _soLuongThepGiaCuongLopDuoi = 2;
        private RebarBarType _duongKinhThepGiaCuongLopDuoi;
        private RebarBarType _stirrupDiameter;
        private int _stirrupSpacing = 200;
        private double _cover;
        private Document _doc;
        private UIDocument _uiDoc;

        private BeamRebarModel BeamRebarModelInstance;

        public BeamRebarViewModel(Document doc, UIDocument uiDoc, BeamRebarModel beamRebarModelInstance = null)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            BeamRebarModelInstance = beamRebarModelInstance;
            GetData();
            OkCommand = new RelayCommand(Run);
            CloseCommand = new RelayCommand(Close);
        }

        private void Close()
        {
            BeamRebarView?.Close();
        }

        private void Run()
        {
            BeamRebarView?.Close();

            try
            {
                using (var transaction = new Transaction(_doc, "Create Beam Rebars"))
                {
                    transaction.Start();

                    // Create stirrups
                    CreateStirrup();

                    // Additional logic for creating main and reinforcement rebars can be added here

                    transaction.Commit();
                }
            }
            catch (OperationCanceledException)
            {
                // Handle operation cancellation gracefully
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
            }
        }

        public RelayCommand OkCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }

        public List<RebarBarType> Diameters { get; private set; }
        public BeamRebarView BeamRebarView { get; set; }

        public int SoLuongThepChinh
        {
            get => _soLuongThepChinh;
            set { if (value != _soLuongThepChinh) { _soLuongThepChinh = value; OnPropertyChanged(); } }
        }

        public RebarBarType DuongKinhThepChinh
        {
            get => _duongKinhThepChinh;
            set { if (!Equals(value, _duongKinhThepChinh)) { _duongKinhThepChinh = value; OnPropertyChanged(); } }
        }

        public int SoLuongThepGiaCuongLopTren
        {
            get => _soLuongThepGiaCuongLopTren;
            set { if (value != _soLuongThepGiaCuongLopTren) { _soLuongThepGiaCuongLopTren = value; OnPropertyChanged(); } }
        }

        public RebarBarType DuongKinhThepGiaCuongLopTren
        {
            get => _duongKinhThepGiaCuongLopTren;
            set { if (!Equals(value, _duongKinhThepGiaCuongLopTren)) { _duongKinhThepGiaCuongLopTren = value; OnPropertyChanged(); } }
        }

        public int SoLuongThepGiaCuongLopDuoi
        {
            get => _soLuongThepGiaCuongLopDuoi;
            set { if (value != _soLuongThepGiaCuongLopDuoi) { _soLuongThepGiaCuongLopDuoi = value; OnPropertyChanged(); } }
        }

        public RebarBarType DuongKinhThepGiaCuongLopDuoi
        {
            get => _duongKinhThepGiaCuongLopDuoi;
            set { if (!Equals(value, _duongKinhThepGiaCuongLopDuoi)) { _duongKinhThepGiaCuongLopDuoi = value; OnPropertyChanged(); } }
        }

        public RebarBarType StirrupDiameter
        {
            get => _stirrupDiameter;
            set { if (!Equals(value, _stirrupDiameter)) { _stirrupDiameter = value; OnPropertyChanged(); } }
        }

        public double Cover
        {
            get => _cover;
            set { if (!_cover.Equals(value)) { _cover = value; OnPropertyChanged(); } }
        }

        public int StirrupSpacing
        {
            get => _stirrupSpacing;
            set { if (value != _stirrupSpacing) { _stirrupSpacing = value; OnPropertyChanged(); } }
        }

        
        private void GetData()
        {
            Diameters = new FilteredElementCollector(_doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .OrderBy(x => x.Name)
                .ToList();

            DuongKinhThepChinh = Diameters.FirstOrDefault(x => x.BarNominalDiameter.FeetToMM() > 20);
            DuongKinhThepGiaCuongLopTren = Diameters.FirstOrDefault(x => x.BarNominalDiameter.FeetToMM() > 20);
            DuongKinhThepGiaCuongLopDuoi = Diameters.FirstOrDefault(x => x.BarNominalDiameter.FeetToMM() > 20);
            StirrupDiameter = Diameters.FirstOrDefault(x => x.BarNominalDiameter.FeetToMM() < 12);
        }
        
        public void RunCommand()
        {
            BeamRebarView?.Close();

            try
            {
                var reference = _uiDoc.Selection.PickObject(ObjectType.Element, new BeamSelectionFilter(), "Chọn dầm để tạo thép");
                var beam = _doc.GetElement(reference) as FamilyInstance;

                if (beam != null)
                {
                    MessageBox.Show("Thép đã được tạo thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Đã hủy chọn thép", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class BeamSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem is FamilyInstance familyInstance &&
                       familyInstance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming;
            }

            public bool AllowReference(Reference reference, XYZ position) => false;
        }

        private void CreateStirrup()
        {
            var shape = new FilteredElementCollector(_doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_T1");

            var o1 = BeamRebarModelInstance.StartPointTop
                .Add(BeamRebarModelInstance.Direction * 50.MMtoFeet())
                .Add(XYZ.BasisZ * -(BeamRebarModelInstance.H - 2 * Cover))
                .Add(BeamRebarModelInstance.XVector * Cover);

            var rebar = Rebar.CreateFromRebarShape(_doc, shape, StirrupDiameter, BeamRebarModelInstance.Beam, o1,
                BeamRebarModelInstance.XVector, BeamRebarModelInstance.ZVector);

            var shapeDrivenAccessor = rebar.GetShapeDrivenAccessor();
            shapeDrivenAccessor.ScaleToBox(o1,
                BeamRebarModelInstance.XVector * (BeamRebarModelInstance.B - 2 * Cover),
                BeamRebarModelInstance.ZVector * (BeamRebarModelInstance.H - 2 * Cover));

            bool normalSide = shapeDrivenAccessor.Normal.DotProduct(BeamRebarModelInstance.Direction) < 0;
            shapeDrivenAccessor.SetLayoutAsMaximumSpacing(
                StirrupSpacing.MMtoFeet(),
                (BeamRebarModelInstance.StartPointTop - BeamRebarModelInstance.EndPointTop).GetLength() - 100.MMtoFeet(),
                normalSide, true, true);
        }
    }

    public static class UnitConversionExtensions
    {
        public static double FeetToMM(this double value) => value * 304.8;
        public static double MMtoFeet(this double value) => value / 304.8;
        public static double MMtoFeet(this int value) => value / 304.8;
    }
}
