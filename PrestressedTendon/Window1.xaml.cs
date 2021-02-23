using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;


namespace PrestressedTendon
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public bool FlatCurveSelected;
        public bool VerticalCurveSelected;
        public bool CrossBeamSelected;
        public bool Done;
        public double PTDistance;


        public Window1()
        {
            InitializeComponent();


            //模型导入器
            ModelImporter modelImporter = new ModelImporter();

            //设置材料颜色
            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.AliceBlue));
            modelImporter.DefaultMaterial = material;

            //三维模型导入
            Model3D Model = modelImporter.Load(@"C:\Users\zyx\Desktop\2RevitArcBridge\RevitArc\RevitArc\source\Chord.obj");

            //和modelview设置binding
            Binding binding = new Binding() { Source = Model };
            this.helixviewport.SetBinding(HelixViewport3D.DataContextProperty, binding);
        }




        private void FlatCurveSelection(object sender, RoutedEventArgs e)
        {
            FlatCurveSelected = true;
            this.window.Hide();
        }

        private void VerticalCurveSelection(object sender, RoutedEventArgs e)
        {
            VerticalCurveSelected = true;
            this.window.Hide();
        }

        private void DoneClick(object sender, RoutedEventArgs e)
        {
            Done = true;
            PTDistance = Convert.ToDouble(this.PTDistance);
            DialogResult = true;
        }

    }
}
