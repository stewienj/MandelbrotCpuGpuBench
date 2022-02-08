using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotCpuGpuBench
{
    public class PlatformOptionsViewModel : NotifyPropertyChanged
    {

        private bool _threadModelMulti = true;
        public bool ThreadModelMulti
        {
            get => _threadModelMulti;
            set => SetProperty(ref _threadModelMulti, value);
        }


        private bool _threadModelSingle = false;
        public bool ThreadModelSingle
        {
            get => _threadModelSingle;
            set => SetProperty(ref _threadModelSingle, value);
        }


        private bool _methodCpuSimd = true;
        public bool MethodCpuSimd
        {
            get => _methodCpuSimd;
            set => SetProperty(ref _methodCpuSimd, value);
        }


        private bool _methodCpuFpu = false;
        public bool MethodCpuFpu
        {
            get => _methodCpuFpu;
            set => SetProperty(ref _methodCpuFpu, value);
        }

        private bool _methodGpu = false;
        public bool MethodGpu
        {
            get => _methodGpu;
            set => SetProperty(ref _methodGpu, value);
        }


        private bool _precisionFloat32 = true;
        public bool PrecisionFloat32
        {
            get => _precisionFloat32;
            set => SetProperty(ref _precisionFloat32, value);
        }


        private bool _precisionFloat64 = false;
        public bool PrecisionFloat64
        {
            get => _precisionFloat64;
            set => SetProperty(ref _precisionFloat64, value);
        }


        private bool _precisionFloat128 = false;
        public bool PrecisionFloat128
        {
            get => _precisionFloat128;
            set => SetProperty(ref _precisionFloat128, value);
        }
    }
}
