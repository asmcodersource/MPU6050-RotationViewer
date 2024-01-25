using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
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
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using System.Security.Policy;
using System.Threading;
using System.Timers;

namespace SystemMPU6050
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected System.Threading.Timer timer;
        protected ArduinoReader arduinoReader { get; set; }

        public MainWindow(String port)
        {
            
            arduinoReader = new ArduinoReader(port, 9600);
            InitializeComponent();
            LoadModel();
            AddGrid();
            arduinoReader.StartReading();
            Closing += (arg1, arg2) => { System.Windows.Application.Current.Shutdown(); };
        }

        protected void LoadModel()
        {
            var reader = new ObjReader();
            planeModel.Content = reader.Read("Models3D/plane.obj");
        }

        protected void AddGrid()
        {
            LinesVisual3D lines = new LinesVisual3D();

            // Set the color and thickness of the lines
            lines.Color = Colors.Red;
            lines.Thickness = 2.0;

            // Define points for the lines
            Point3DCollection points = new Point3DCollection();
            for( int i = -50; i < 50; i++)
            {
                const double dr = 0.1, dc = 0.1;
                points.Add(new Point3D(100, 0, i));
                points.Add(new Point3D(-100, 0, i));
                points.Add(new Point3D(i, 0, 100));
                points.Add(new Point3D(i, 0, -100));
            }

            // Set the points for the lines
            lines.Thickness = 0.1;
            lines.Points = points;

            // Add the lines to the 3D scene
            mainScene.Children.Add(lines);
        }

        private void RotatePlane(object sender,String e)
        {
            try
            {
                String[] values = arduinoReader.GetData().Split(",");
                double angleX = double.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture) - 90;
                double angleY = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                double angleZ = double.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);

                RotateModel(angleX, angleY, angleZ);
            }
            catch (Exception exception) { }

        }

        private void RotateModel(double angleX, double angleY, double angleZ)
        {
            // Create a Transform3DGroup to apply multiple transformations
            Transform3DGroup transformGroup = new Transform3DGroup();

            // Add rotation transformations based on Euler angles
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), angleX)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), angleZ)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), angleY)));

            // Apply the transformations to the modelGroup
             planeModel.Content.Transform = transformGroup;
        }

        private void MoveCubeButton_Click(object sender, RoutedEventArgs e)
        {
            // Start a background thread to simulate work

            timer = new System.Threading.Timer((state) => {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            RotatePlane(this, null);
                        });
                    }
                    catch (Exception exception) { }
            }, null, 0, 10);
            
        }
    }
}
