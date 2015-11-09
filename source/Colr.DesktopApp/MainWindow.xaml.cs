﻿using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Colr.DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ApplicationLayer.MainWindowModel model;

        public MainWindow()
        {
            InitializeComponent();

            this.model = new ApplicationLayer.MainWindowModel();
            DataContext = this.model;
        }

        ///////////////////////////////////////////////////////////////////////

        void openButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
                this.model.LoadImage(dialog.FileName);
        }

        void analyzeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
