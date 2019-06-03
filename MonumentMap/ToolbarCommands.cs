using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonumentMap
{
    public class ToolbarCommands
    {

        public static readonly RoutedUICommand AddNewMonument = new RoutedUICommand(
            "Add new monument",
            "AddNewMonument",
            typeof(RoutedCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.N, ModifierKeys.Control)

            });

    }
}
