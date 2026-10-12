using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Etiquetadora.Models;
using Etiquetadora.ViewModels;

namespace Etiquetadora.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
    }
}