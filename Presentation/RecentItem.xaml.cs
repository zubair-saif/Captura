﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Captura
{
    public partial class RecentItem
    {
        string FilePath;

        public RecentItem(string FileName)
        {
            InitializeComponent();

            FilePath = FileName;
            UrlButton.Content = Path.GetFileName(FileName);

            UrlButton.CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage,
                (s, e) => Process.Start(FilePath),
                (s, e) => e.CanExecute = File.Exists(FilePath)));
        }

        public event Action Remove;

        void RemoveButton_Click(object sender, RoutedEventArgs e) => Remove?.Invoke();

        void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(FilePath) { Verb = "Print" });
        }

        void Delete_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(FilePath);
            Remove?.Invoke();
        }
    }
}
