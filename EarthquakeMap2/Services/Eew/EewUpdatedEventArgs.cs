using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EewData = EewLibrary.EEW;

namespace EarthquakeMap2.Services.Eew;

internal class EewUpdatedEventArgs<T> where T : EewData
{
    public EewUpdatedEventArgs(T eew) => Eew = eew;

    public T Eew { get; }
}
