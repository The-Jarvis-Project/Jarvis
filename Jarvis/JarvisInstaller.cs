using System.ComponentModel;
using System.Configuration.Install;

namespace Jarvis
{
    [RunInstaller(true)]
    public partial class JarvisInstaller : Installer
    {
        public JarvisInstaller()
        {
            InitializeComponent();
        }
    }
}
