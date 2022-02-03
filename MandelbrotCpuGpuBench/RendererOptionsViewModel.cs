using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotCpuGpuBench
{
    public class RendererOptionsViewModel : NotifyPropertyChanged
    {
        private bool _languageCs = true;
        public bool LanguageCs
        {
            get => _languageCs;
            set => SetProperty(ref _languageCs, value);
        }

        private bool _languageCpp = false;
        public bool LanguageCpp
        {
            get => _languageCpp;
            set => SetProperty(ref _languageCpp, value);
        }

        public PlatformOptionsViewModel Cs { get; } = new PlatformOptionsViewModel();
        public PlatformOptionsViewModel Cpp { get; } = new PlatformOptionsViewModel();

    }
}
