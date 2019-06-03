using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonumentMap
{
    public static class ToolbarCommands
    {

        public static readonly RoutedUICommand AddNewMonument = new RoutedUICommand(
            "Add new monument",
            "AddNewMonument",
            typeof(RoutedCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.N, ModifierKeys.Control)

            });

        public static readonly RoutedUICommand AddNewType = new RoutedUICommand(
            "Add new type",
            "AddNewType",
            typeof(RoutedCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.T, ModifierKeys.Control)

            });

        
        public static readonly RoutedUICommand AddNewTag = new RoutedUICommand(
            "Add new tag",
            "AddNewTag",
            typeof(RoutedCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.T, ModifierKeys.Alt)

            });

        public static readonly RoutedUICommand IncreaseFont = new RoutedUICommand(
            "Increase font",
            "IncreaseFont",
            typeof(RoutedCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Up, ModifierKeys.Control)

            });

        public static readonly RoutedUICommand DecreaseFont = new RoutedUICommand(
            "Decrease font",
            "DecreaseFont",
            typeof(RoutedCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Down, ModifierKeys.Control)

            });


        public static readonly RoutedUICommand SearchMonuments = new RoutedUICommand(
            "Search monuments",
            "SearchMonuments",
            typeof(RoutedCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F, ModifierKeys.Control)

            });


        public static readonly RoutedUICommand ClosePopUp = new RoutedUICommand(
           "Close pop up",
           "CloePopUp",
           typeof(RoutedCommand),
           new InputGestureCollection()
           {
                new KeyGesture(Key.Escape)

           });


    }
}
