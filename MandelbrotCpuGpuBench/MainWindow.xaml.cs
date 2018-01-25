﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MandelbrotCpuGpuBench
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private Point _lastMousePos;
    public MainWindow()
    {
      InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      Workspace.Closing = true;
      base.OnClosing(e);
    }

    private void Grid_MouseMove(object sender, MouseEventArgs e)
    {
      if (Mouse.LeftButton == MouseButtonState.Pressed)
      {
        Workspace.ChangeSize((int)_image.ActualWidth, (int)_image.ActualHeight);
        var position = e.GetPosition(this);
        Workspace.Move(_lastMousePos - position);
        _lastMousePos = position;
      }
    }

    private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      Workspace.ChangeSize((int)_image.ActualWidth, (int)_image.ActualHeight);
      Workspace.Zoom(e.Delta, e.GetPosition(_image));
      
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Mouse.LeftButton == MouseButtonState.Pressed)
      {
        _lastMousePos = e.GetPosition(this);
      }
    }

    private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
    {
    }

    private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      Workspace.ChangeSize((int)_image.ActualWidth, (int)_image.ActualHeight);
    }

    private void Render_Click(object sender, RoutedEventArgs e)
    {
      Workspace.DoRender();
    }

    public Workspace Workspace { get; } = new Workspace();

  }
}