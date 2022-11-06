using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace House_of_Quran
{
    internal static class Animation
    {
        internal static void HideAnimation(Grid grid)
        {
            grid.Opacity = 0.999;

            Timer anim = new Timer(10);
            anim.Elapsed += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (grid.Opacity <= 0.999)
                    {
                        grid.Opacity -= 0.025;
                        if (grid.Opacity <= 0.2)
                        {
                            grid.Opacity = 0.2;
                            anim.Stop();
                        }
                    }
                    else
                        anim.Stop(); // Cancel animation car l'opacité a été remis à 1       
                });
            };

            anim.Start();
        }
    }
}
